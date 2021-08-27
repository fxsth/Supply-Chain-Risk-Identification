using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace SCRI.Database
{
    /// <summary>
    /// Factory for Neo4j driver
    /// Creates Driver with user definied credentials
    /// </summary>
    class DriverFactory : IDriverFactory
    {
        public string URI { get; set; }
        public Action<ConfigBuilder> Action { get; set; }
        public IAuthToken AuthToken { get; set; }

        public IDriver CreateDriver()
        {
            try
            {
                return GraphDatabase.Driver(URI, AuthToken, Action);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
