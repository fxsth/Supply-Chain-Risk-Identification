using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver;
using SCRI.Database;
using SCRI.Models;
using SCRI.Utils;
using SupplyChainLinkFeatures = MachineLearning.Models.SupplyChainLinkFeatures;

namespace SCRI.Services
{
    class GraphService : IGraphService
    {
        private readonly IDriver _driver;
        private readonly IGraphStore _graphStore;

        private const string SupplierLabel = "Supplier";
        private const string ProductLabel = "Product";

        public GraphService(IDriverFactory driverFactory, IGraphStore graphStore)
        {
            _graphStore = graphStore;
            _driver = driverFactory.CreateDriver();
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
            if (_graphStore.availableGraphs.All(x => x != databaseName))
                return false;
            using (var session = _driver.AsyncSession(o => o.WithDatabase(databaseName)))
            {
                if (labelFilter?.Count() == 1)
                    await UpdateCentralityMeasuresInDb(session, labelFilter.Single());
                else
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
                var dbSchema = await session.ReadTransactionAsync(CypherTransactions.GetDatabaseSchemaAsync);
                _graphStore.StoreSchema(databaseName, dbSchema);
            }

            return true;
        }

        public GraphViewerSettings CreateGraphViewerSettings()
        {
            return new GraphViewerSettings(_graphStore);
        }

        private async Task<bool> UpdateCentralityMeasuresInDb(IAsyncSession session, string filterByNodeLabel = "*")
        {
            // Check plugins
            var procedures =
                await session.ReadTransactionAsync(CypherTransactions.GetAvailableProceduresAsync);
            bool apoc = Neo4jUtils.IsAPOCEnabled(procedures);
            bool gds = Neo4jUtils.IsGraphDataScienceLibraryEnabled(procedures);
            if (!apoc || !gds)
                return false;
            // Execute Centrality Algorithms and write results to graph properties
            await session.WriteTransactionAsync(tx =>
                CypherTransactions.WriteCentralityMeasuresToPropertyAsync(tx, filterByNodeLabel));
            return true;
        }

        public async Task CalculateLinkFeatures(string databaseName)
        {
            await using var session = _driver.AsyncSession(o => o.WithDatabase(databaseName));
            // Performance can be improved by a query that gets complete connected graph with every score as properties

            var crossProduct =
                await session.ReadTransactionAsync(tx => CypherTransactions.GetCrossProduct(tx, SupplierLabel));
            // Associations exist between two Suppliers
            // aggregate scores for relations
            Dictionary<(int, int), SupplyChainLinkFeatures> linkPredictionScores =
                crossProduct.ToDictionary(key => key, value => new SupplyChainLinkFeatures());

            var outsourcingAssociations = await session.ReadTransactionAsync(tx =>
                CypherTransactions.GetOutsourcingAssociations(tx, SupplierLabel, ProductLabel));
            // Increase association score on matching node-pair   
            foreach (var association in outsourcingAssociations)
            {
                // increase score on both node pairs a,b and node pair b,a
                linkPredictionScores[(association.Key, association.Value)].OutsourcingAssociation += 1;
                linkPredictionScores[(association.Value, association.Key)].OutsourcingAssociation += 1;
            }

            var buyerAssociations =
                await session.ReadTransactionAsync(
                    tx => CypherTransactions.GetBuyerAssociations(tx, SupplierLabel));
            foreach (var association in buyerAssociations)
            {
                linkPredictionScores[(association.Key, association.Value)].BuyerAssociation += 1;
                linkPredictionScores[(association.Value, association.Key)].BuyerAssociation += 1;
            }

            var competitionAssociations = await session.ReadTransactionAsync(tx =>
                CypherTransactions.GetCompetitionAssociations(tx, SupplierLabel, ProductLabel));
            foreach (var association in competitionAssociations)
            {
                linkPredictionScores[(association.Key, association.Value)].CompetitionAssociation += 1;
                linkPredictionScores[(association.Value, association.Key)].CompetitionAssociation += 1;
            }

            var degrees =
                await session.ReadTransactionAsync(tx =>
                    CypherTransactions.GetDegreeCentralityAsStreamAsync(tx, SupplierLabel));
            // Iterate through node-degree-pairs 
            foreach (var degree in degrees)
            {
                // and set degree on FIRST node in node-pair (start node of possible edge)
                foreach (var keyValuePair in linkPredictionScores.Where(x => x.Key.Item1 == degree.Key))
                {
                    keyValuePair.Value.Degree = Convert.ToInt32(degree.Value);
                }
            }

            // set label
            var existingEdgesBetweenSuppliers =
                await session.ReadTransactionAsync(tx => CypherTransactions.GetAllEdgesAsync(tx, SupplierLabel));
            foreach (var existingEdge in existingEdgesBetweenSuppliers)
            {
                linkPredictionScores[((int) existingEdge.Key.Id, (int) existingEdge.Value.Id)].Exists = true;
                linkPredictionScores[((int) existingEdge.Value.Id, (int) existingEdge.Key.Id)].Exists = true;
            }

            _graphStore.StoreLinkFeatures(databaseName, linkPredictionScores);
        }

        public Dictionary<(int, int), SupplyChainLinkFeatures> GetLinkFeatures(string graphName) =>
            _graphStore.GetLinkFeatures(graphName);

        public bool ExistLinkFeatures(string graphName) => _graphStore.ExistLinkFeatures(graphName);
    }
}