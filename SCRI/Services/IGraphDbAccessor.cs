using SCRI.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using MachineLearning.Models;

namespace SCRI.Services
{
    public interface IGraphDbAccessor
    {
        Task Init();
        string GetDefaultGraphName();
        Dictionary<int, Dictionary<string, string>> GetGraphPropertiesAndValues(string graphName);
        IEnumerable<string> GetAvailableGraphs();
        IEnumerable<string> GetLabelsInGraphSchema(string graphName);
        Task<bool> RetrieveGraphFromDatabase(string databaseName, IEnumerable<string> labelFilter = null);
        GraphViewerSettings CreateGraphViewerSettings();
        public Task CalculateLinkFeatures(string databaseName);

        public Dictionary<(int, int), SupplyChainLinkFeatures> GetLinkFeatures(string graphName);
        public bool ExistLinkFeatures(string graphName);
    }
}