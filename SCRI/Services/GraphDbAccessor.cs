﻿using Microsoft.Msagl.Drawing;
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
            using (var session = _driver.AsyncSession())
            {
                // get available graphs
                var availableDatabases = await session.ReadTransactionAsync(tx => CypherTransactions.GetDatabasesAsync(tx));
                _graphStore.AnnounceAvailableGraphs(availableDatabases);
                // default database for initial display
                _graphStore.defaultGraph = await session.ReadTransactionAsync(tx => CypherTransactions.GetDefaultDatabaseAsync(tx));
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
            using (var session = _driver.AsyncSession(o => o.WithDatabase(databaseName)))
            {
                // Get graph from db
                SupplyNetwork graphData = await session.ReadTransactionAsync(tx => CypherTransactions.GetCompleteGraphAsync(tx));
                _graphStore.StoreGraph(databaseName, graphData);

                // retrieve schema
                var dbSchema = await session.ReadTransactionAsync(tx => CypherTransactions.GetDatabaseSchemaAsync(tx));
                _graphStore.StoreSchema(databaseName, dbSchema);

                await UpdateCentralityMeasuresInDb(session);
            }
            return true;
        }

        public GraphViewerSettings CreateGraphViewerSettings()
        {
            return new GraphViewerSettings(_graphStore);
        }

        private async Task<bool> UpdateCentralityMeasuresInDb(IAsyncSession session)
        {
            // Check plugins
            var procedures = await session.ReadTransactionAsync(tx => CypherTransactions.GetAvailableProceduresAsync(tx));
            bool apoc = Neo4jUtils.isAPOCEnabled(procedures);
            bool gds = Neo4jUtils.isGraphDataScienceLibraryEnabled(procedures);
            if (!apoc || !gds)
                return false;
            // Execute Centrality Algorithms and write results to graph properties
            await session.WriteTransactionAsync(tx => CypherTransactions.WriteCentralityMeasuresToPropertyAsync(tx));
            return true;
        }
    }
}
