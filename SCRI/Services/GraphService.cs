using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MachineLearning;
using Microsoft.Extensions.Configuration;
using Neo4j.Driver;
using SCRI.Configuration;
using SCRI.Database;
using SCRI.Models;
using SCRI.Utils;
using MachineLearning.Models;

namespace SCRI.Services
{
    class GraphService : IGraphService
    {
        private readonly IDriver _driver;
        private readonly IGraphStore _graphStore;

        private readonly string _supplierLabel;
        private readonly string _productLabel;
        private readonly uint _mlTrainingTimeInSeconds;

        public GraphService(IDriverFactory driverFactory, IGraphStore graphStore, IConfiguration configuration)
        {
            _graphStore = graphStore;
            _driver = driverFactory.CreateDriver();
            _supplierLabel = configuration.GetGraphSettings().LabelSupplier;
            _productLabel = configuration.GetGraphSettings().LabelProduct;
            _mlTrainingTimeInSeconds = configuration.GetMLSettings().TrainingTimeSpanInSeconds;
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

        public IEnumerable<string> GetAvailableGraphs()
        {
            return _graphStore.availableGraphs;
        }

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
            bool gds = Neo4jUtils.IsGraphDataScienceLibraryEnabled(procedures);
            if (!gds)
                return false;
            // Execute Centrality Algorithms and write results to graph properties
            await session.WriteTransactionAsync(tx =>
                CypherTransactions.WriteCentralityMeasuresToPropertyAsync(tx, filterByNodeLabel));
            return true;
        }

        public async Task TrainLinkPredictionOnGivenGraph(string databaseName)
        {
            Dictionary<(int, int), SupplyChainLinkFeatures> featuresList = await CalculateLinkFeatures(databaseName);

            LinkPredictor linkPredictor = new LinkPredictor();
            linkPredictor.SetData(featuresList.Values);

            // run in another thread
            await Task.Run(()=>linkPredictor.TrainModel(_mlTrainingTimeInSeconds, null));
            await linkPredictor.SaveTrainingResults();
            linkPredictor.SaveModel(linkPredictor.GetBestModel());
        }

        public async Task<Dictionary<(int, int), PredictedSupplyChainLink>> ExecuteLinkPredictionOnGivenGraph(string databaseName)
        {
            Dictionary<(int, int), SupplyChainLinkFeatures> featuresList = await CalculateLinkFeatures(databaseName);

            LinkPredictor linkPredictor = new LinkPredictor();

            // run in another thread
            return await Task.Run(() => linkPredictor.PredictLinkExistences(featuresList));
        }

        private async Task<Dictionary<(int, int),SupplyChainLinkFeatures>>CalculateLinkFeatures(string databaseName)
        {
            await using var session = _driver.AsyncSession(o => o.WithDatabase(databaseName));
            // Performance can be improved by a query that gets complete connected graph with every score as properties

            var crossProduct =
                await session.ReadTransactionAsync(tx => CypherTransactions.GetCrossProduct(tx, _supplierLabel));
            // Associations exist between two Suppliers
            // aggregate scores for relations
            Dictionary<(int, int), SupplyChainLinkFeatures> linkPredictionFeatureSet =
                crossProduct.ToDictionary(key => key, _ => new SupplyChainLinkFeatures());

            var outsourcingAssociations = await session.ReadTransactionAsync(tx =>
                CypherTransactions.GetOutsourcingAssociations(tx, _supplierLabel, _productLabel));
            // Increase association score on matching node-pair   
            foreach (var association in outsourcingAssociations)
            {
                // increase score on both node pairs a,b and node pair b,a
                linkPredictionFeatureSet[(association.Key, association.Value)].OutsourcingAssociation += 1;
                linkPredictionFeatureSet[(association.Value, association.Key)].OutsourcingAssociation += 1;
            }

            var buyerAssociations =
                await session.ReadTransactionAsync(
                    tx => CypherTransactions.GetBuyerAssociations(tx, _supplierLabel));
            foreach (var association in buyerAssociations)
            {
                linkPredictionFeatureSet[(association.Key, association.Value)].BuyerAssociation += 1;
                linkPredictionFeatureSet[(association.Value, association.Key)].BuyerAssociation += 1;
            }

            var competitionAssociations = await session.ReadTransactionAsync(tx =>
                CypherTransactions.GetCompetitionAssociations(tx, _supplierLabel, _productLabel));
            foreach (var association in competitionAssociations)
            {
                linkPredictionFeatureSet[(association.Key, association.Value)].CompetitionAssociation += 1;
                linkPredictionFeatureSet[(association.Value, association.Key)].CompetitionAssociation += 1;
            }

            var degrees =
                await session.ReadTransactionAsync(tx =>
                    CypherTransactions.GetDegreeCentralityAsStreamAsync(tx, _supplierLabel));
            // Iterate through node-degree-pairs 
            foreach (var degree in degrees)
            {
                // and set degree on FIRST node in node-pair (start node of possible edge)
                foreach (var keyValuePair in linkPredictionFeatureSet.Where(x => x.Key.Item1 == degree.Key))
                {
                    keyValuePair.Value.Degree = Convert.ToInt32(degree.Value);
                }
            }

            // set label
            var existingEdgesBetweenSuppliers =
                await session.ReadTransactionAsync(tx => CypherTransactions.GetAllEdgesAsync(tx, _supplierLabel));
            foreach (var existingEdge in existingEdgesBetweenSuppliers)
            {
                linkPredictionFeatureSet[((int) existingEdge.Key.Id, (int) existingEdge.Value.Id)].Exists = true;
                linkPredictionFeatureSet[((int) existingEdge.Value.Id, (int) existingEdge.Key.Id)].Exists = true;
            }

            _graphStore.StoreLinkFeatures(databaseName, linkPredictionFeatureSet);
            return linkPredictionFeatureSet;
        }

        public Dictionary<(int, int), SupplyChainLinkFeatures> GetLinkFeatureSet(string graphName) =>
            _graphStore.GetLinkFeatureSet(graphName);

        public bool ExistLinkFeatureSet(string graphName) => _graphStore.ExistLinkFeatureSet(graphName);
    }
}