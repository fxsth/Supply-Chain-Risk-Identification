using SCRI.Models;
using System.Collections.Generic;
using System.Linq;
using SupplyChainLinkFeatures = MachineLearning.Models.SupplyChainLinkFeatures; 

namespace SCRI.Services
{
    /// <summary>
    /// Stores the graphs from the database
    /// </summary>
    public class GraphStore : IGraphStore
    {
        private readonly Dictionary<string, SupplyNetwork> _graphDictionary = new();
        private readonly Dictionary<string, DbSchema> _schemaDictionary = new();

        private readonly Dictionary<string, Dictionary<(int, int), SupplyChainLinkFeatures>> _featuresMap = new();

        public string defaultGraph { get; set; }
        public IEnumerable<string> availableGraphs => _graphDictionary.Select(x => x.Key);

        public void AnnounceAvailableGraphs(IEnumerable<string> graphNames)
        {
            foreach (var graph in graphNames)
            {
                _graphDictionary[graph] = null;
            }
        }

        public SupplyNetwork GetGraph(string graphName)
        {
            if (_graphDictionary.ContainsKey(graphName))
                return _graphDictionary[graphName];
            else
                return null;
        }

        public void StoreGraph(string graphName, SupplyNetwork graph)
        {
            _graphDictionary[graphName] = graph;
        }

        public DbSchema GetDbSchema(string graphName)
        {
            if (_schemaDictionary.ContainsKey(graphName))
                return _schemaDictionary[graphName];
            else
                return null;
        }

        public void StoreSchema(string graphName, DbSchema schema)
        {
            _schemaDictionary[graphName] = schema;
        }

        public void StoreLinkFeatures(string graphName, Dictionary<(int, int), SupplyChainLinkFeatures> featuresMap)
        {
            _featuresMap[graphName] = featuresMap;
        }

        public Dictionary<(int, int), SupplyChainLinkFeatures> GetLinkFeatureSet(string graphName) => _featuresMap[graphName];
        public bool ExistLinkFeatureSet(string graphName) => _featuresMap.ContainsKey(graphName);
    }
}