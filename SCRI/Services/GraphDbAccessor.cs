using Neo4j.Driver;
using SCRI.Database;
using SCRI.Models;
using SCRI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MachineLearning;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using SupplyChainLinkFeatures = MachineLearning.Models.SupplyChainLinkFeatures;

namespace SCRI.Services
{
    class GraphDbAccessor : IGraphDbAccessor
    {
        private readonly IDriverFactory _driverFactory;
        private IDriver _driver;
        private IGraphStore _graphStore;

        private const string SUPPLIER_LABEL = "Supplier";

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
                var availableDatabases =
                    await session.ReadTransactionAsync(tx => CypherTransactions.GetDatabasesAsync(tx));
                _graphStore.AnnounceAvailableGraphs(availableDatabases);
                // default database for initial display
                _graphStore.defaultGraph =
                    await session.ReadTransactionAsync(tx => CypherTransactions.GetDefaultDatabaseAsync(tx));
                await RetrieveGraphFromDatabase(_graphStore.defaultGraph);
            }
        }

        public string GetDefaultGraphName()
        {
            return _graphStore.defaultGraph;
        }

        public IEnumerable<string> GetAvailableGraphs() => _graphStore.availableGraphs;

        public Dictionary<int, Dictionary<string, string>> GetGraphPropertiesAndValues(string graphName)
        {
            return Neo4jUtils.GetGraphPropertiesAndValues(_graphStore.GetGraph(graphName));
        }

        public IEnumerable<string> GetLabelsInGraphSchema(string graphName)
        {
            return _graphStore.GetDbSchema(graphName).GetUniqueNodeLabels();
        }

        public async Task<bool> RetrieveGraphFromDatabase(string databaseName, IEnumerable<string> labelFilter = null)
        {
            if (!_graphStore.availableGraphs.Any(x => x == databaseName))
                return false;
            using (var session = _driver.AsyncSession(o => o.WithDatabase(databaseName)))
            {
                await UpdateCentralityMeasuresInDb(session);

                // Get graph from Db
                SupplyNetwork graphData;
                if (labelFilter is null)
                    graphData = await session.ReadTransactionAsync(tx => CypherTransactions.GetCompleteGraphAsync(tx));
                else
                    graphData = await session.ReadTransactionAsync(tx =>
                        CypherTransactions.GetGraphFilteredByLabelsAsync(tx, labelFilter));
                _graphStore.StoreGraph(databaseName, graphData);

                // retrieve schema
                var dbSchema = await session.ReadTransactionAsync(tx => CypherTransactions.GetDatabaseSchemaAsync(tx));
                _graphStore.StoreSchema(databaseName, dbSchema);
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
            var procedures =
                await session.ReadTransactionAsync(tx => CypherTransactions.GetAvailableProceduresAsync(tx));
            bool apoc = Neo4jUtils.isAPOCEnabled(procedures);
            bool gds = Neo4jUtils.isGraphDataScienceLibraryEnabled(procedures);
            if (!apoc || !gds)
                return false;
            // Execute Centrality Algorithms and write results to graph properties
            await session.WriteTransactionAsync(tx => CypherTransactions.WriteCentralityMeasuresToPropertyAsync(tx));
            return true;
        }

        public async Task CalculateLinkFeatures(string databaseName)
        {
            using (var session = _driver.AsyncSession(o => o.WithDatabase(databaseName)))
            {
                // Performance can be improved by a query that gets complete connected graph with every score as properties

                var outsourcingAssociations = await session.ReadTransactionAsync(tx =>
                    CypherTransactions.GetOutsourcingAssociations(tx, SUPPLIER_LABEL, "Product"));
                var buyerAssociations =
                    await session.ReadTransactionAsync(
                        tx => CypherTransactions.GetBuyerAssociations(tx, SUPPLIER_LABEL));
                var competitionAssociations = await session.ReadTransactionAsync(tx =>
                    CypherTransactions.GetCompetitionAssociations(tx, SUPPLIER_LABEL));
                var degrees =
                    await session.ReadTransactionAsync(tx => CypherTransactions.GetDegreeCentralityAsStreamAsync(tx));
                var crossProduct =
                    await session.ReadTransactionAsync(tx => CypherTransactions.GetCrossProduct(tx, SUPPLIER_LABEL));
                var existingEdgesBetweenSuppliers =
                    await session.ReadTransactionAsync(tx => CypherTransactions.GetAllEdgesAsync(tx, SUPPLIER_LABEL));

                // Associations exist between two Suppliers
                // aggregate scores for relations
                Dictionary<(int, int), SupplyChainLinkFeatures> linkPredictionScores =
                    crossProduct.ToDictionary(key => key, value => new SupplyChainLinkFeatures());

                // Increase association score on matching node-pair   
                foreach (var association in outsourcingAssociations)
                {
                    linkPredictionScores[GetSortedInts(association)].OutsourcingAssociation += 1;
                }

                foreach (var association in buyerAssociations)
                {
                    linkPredictionScores[GetSortedInts(association)].BuyerAssociation += 1;
                }

                foreach (var association in competitionAssociations)
                {
                    linkPredictionScores[GetSortedInts(association)].CompetitionAssociation += 1;
                }

                // Iterate through node-degree-pairs 
                foreach (var degree in degrees)
                {
                    // and set degree on FIRST node in node-pair
                    foreach (var keyValuePair in linkPredictionScores.Where(x => x.Key.Item1 == degree.Key))
                    {
                        keyValuePair.Value.Degree = Convert.ToInt32(degree.Value);
                    }
                }

                foreach (var existingEdge in existingEdgesBetweenSuppliers)
                {
                    linkPredictionScores[
                            GetSortedInts(
                                new KeyValuePair<int, int>((int) existingEdge.Key.Id, (int) existingEdge.Value.Id))]
                        .Exists = true;
                }

                _graphStore.StoreLinkFeatures(databaseName, linkPredictionScores);
            }


            //TODO: Calculate link prediction measures by iterating through associations
            // Get cross product and set values to create traing data set
        }

        public Dictionary<(int, int), SupplyChainLinkFeatures> GetLinkFeatures(string graphName) => _graphStore.GetLinkFeatures(graphName);

        public bool ExistLinkFeatures(string graphName) => _graphStore.ExistLinkFeatures(graphName);

        /// <summary>
        /// Simple method that orders node-IDs to omid duplicate edges
        /// Link Prediction Features are based on undirected edges
        /// </summary>
        /// <param name="keyValuePair"></param>
        /// <returns></returns>
        private (int, int) GetSortedInts(KeyValuePair<int, int> keyValuePair)
        {
            int s1 = keyValuePair.Key, s2 = keyValuePair.Value;
            if (s1 > s2)
            {
                return (s2, s1);
            }

            return (s1, s2);
        }
    }
}