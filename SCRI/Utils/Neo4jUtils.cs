using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCRI.Utils
{
    public static class Neo4jUtils
    {
        public static bool isAPOCEnabled(IEnumerable<string> listOfProcedures)
        {
            return listOfProcedures.Any(str => str.Contains("apoc."));
        }

        public static bool isGraphDataScienceLibraryEnabled(IEnumerable<string> listOfProcedures)
        {
            return listOfProcedures.Any(str => str.Contains("gds."));
        }
    }
}
