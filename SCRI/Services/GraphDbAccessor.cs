using Microsoft.Msagl.Drawing;
using Neo4j.Driver;
using SCRI.Database;
using SCRI.Models;
using SCRI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCRI.Services
{
    class GraphDbAccessor : IGraphDbAccessor
    {
        private readonly IDriverFactory _driverFactory;
        private IDriver _driver;
        private IGraphStore _graphStore;


        public GraphDbAccessor(IDriverFactory driverFactory, IGraphStore graphStore)
        {
            _driverFactory = driverFactory;
            _graphStore = graphStore;
            _driver = _driverFactory.CreateDriver();
        }

        public async Task Init()
        {
            using (var session = _driver.Session())
            {
                // get available graphs
                var availableDatabases = session.ReadTransaction(tx => CypherTransactions.GetDatabases(tx));
                _graphStore.AnnounceAvailableGraphs(availableDatabases);
                // default database for initial display
                _graphStore.defaultGraph = session.ReadTransaction(tx => CypherTransactions.GetDefaultDatabase(tx));
                await RetrieveGraphFromDatabase(_graphStore.defaultGraph);
            }
        }

        public string GetDefaultGraph()
        {
            return _graphStore.defaultGraph;
        }

        public IEnumerable<string> GetAvailableGraphs() => _graphStore.availableGraphs;

        public Dictionary<int, Dictionary<string, string>> GetGraphPropertiesAndValues(string graphName)
        {
            return Neo4jUtils.GetGraphPropertiesAndValues(_graphStore.GetGraph(graphName));
        }

        public async Task<bool> RetrieveGraphFromDatabase(string databaseName)
        {
            if (!_graphStore.availableGraphs.Any(x => x == databaseName))
                return false;
            using (var session = _driver.Session(o => o.WithDatabase(databaseName)))
            {
                // Get graph from db
                SupplyNetwork graphData = session.ReadTransaction(tx => CypherTransactions.GetCompleteGraph(tx));
                _graphStore.StoreGraph(databaseName, graphData);

                // retrieve schema
                var dbSchema = session.ReadTransaction(tx => CypherTransactions.GetDatabaseSchema(tx));
                _graphStore.StoreSchema(databaseName, dbSchema);
            }
            return true;
        }

        public GraphViewerSettings CreateGraphViewerSettings()
        {
            return new GraphViewerSettings(_graphStore);
        }

        private async Task<bool> UpdateCentralityMeasuresInDb(ISession session)
        {
            // Check plugins
            var procedures = session.ReadTransaction(tx => CypherTransactions.GetAvailableProcedures(tx));
            bool apoc = Neo4jUtils.isAPOCEnabled(procedures);
            bool gds = Neo4jUtils.isGraphDataScienceLibraryEnabled(procedures);
            if (!apoc || !gds)
                return false;
            // Execute Centrality Algorithms and write results to graph properties
            session.WriteTransaction<object>(tx => CypherTransactions.WriteCentralityMeasuresToProperty(tx));
            return true;
        }
    }
}
