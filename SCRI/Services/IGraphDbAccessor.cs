using SCRI.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SCRI.Services
{
    public interface IGraphDbAccessor
    {
        Task Init();
        string GetDefaultGraphName();
        Dictionary<int, Dictionary<string, string>> GetGraphPropertiesAndValues(string graphName);
        IEnumerable<string> GetAvailableGraphs();
        Task<bool> RetrieveGraphFromDatabase(string databaseName);
        GraphViewerSettings CreateGraphViewerSettings();
    }
}