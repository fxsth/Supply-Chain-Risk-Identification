using QuikGraph;
using SCRI.Models;
using System.Collections.Generic;
using SupplyChainLinkFeatures = MachineLearning.Models.SupplyChainLinkFeatures;

namespace SCRI.Services
{
    public interface IGraphStore
    {
        string defaultGraph { get; set; }
        SupplyNetwork GetGraph(string graphName);
        void StoreGraph(string graphName, SupplyNetwork graph);
        DbSchema GetDbSchema(string graphName);
        void StoreSchema(string graphName, DbSchema schema);
        IEnumerable<string> availableGraphs { get; }
        void AnnounceAvailableGraphs(IEnumerable<string> graphNames);
        public void StoreLinkFeatures(string graphName, Dictionary<(int, int), SupplyChainLinkFeatures> featuresMap);
        public Dictionary<(int, int), SupplyChainLinkFeatures> GetLinkFeatureSet(string graphName);
        public bool ExistLinkFeatureSet(string graphName);
    }
}