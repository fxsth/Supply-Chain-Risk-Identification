using SCRI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCRI.Utils
{
    public static class Neo4jUtils
    {
        public static bool IsAPOCEnabled(IEnumerable<string> listOfProcedures)
        {
            return listOfProcedures.Any(str => str.Contains("apoc."));
        }

        public static bool IsGraphDataScienceLibraryEnabled(IEnumerable<string> listOfProcedures)
        {
            return listOfProcedures.Any(str => str.Contains("gds."));
        }
        
        /// <summary>
        /// Problem: some algorithms in GraphDataScienceLibrary are still in alpha-Version and can have different names
        /// e.g. gds.closeness.write vs gds.alpha.closeness.write
        /// Therefore, pick algorithm out of available procedures that contains algorithm name
        /// </summary>
        public static string GetSpecificProcedure(IEnumerable<string> listOfProcedures, string searchTerm)
        {
            return listOfProcedures.FirstOrDefault(str => str.Contains(searchTerm));
        }

        public static Dictionary<int, Dictionary<string, string>> GetGraphPropertiesAndValues(SupplyNetwork supplyNetwork)
        {
            return supplyNetwork.Vertices.ToDictionary(x => x.ID, x => x.Properties);
        }
    }
}
