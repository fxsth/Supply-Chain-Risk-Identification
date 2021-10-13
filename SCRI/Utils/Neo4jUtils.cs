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
