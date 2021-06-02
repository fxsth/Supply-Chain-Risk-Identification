using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCRI.Database
{
    public interface IDriverFactory
    {
        public IDriver CreateDriver();
        //    string uri) => GraphDatabase.Driver(uri);
        //public IDriver CreateDriver(Uri uri) => GraphDatabase.Driver(uri);
        //public IDriver CreateDriver(string uri, Action<ConfigBuilder> action) => GraphDatabase.Driver(uri, action);
        //public IDriver CreateDriver(Uri uri, Action<ConfigBuilder> action) => GraphDatabase.Driver(uri, action);
        //public IDriver CreateDriver(string uri, IAuthToken authToken) => GraphDatabase.Driver(uri, authToken);
        //public IDriver CreateDriver(Uri uri, IAuthToken authToken) => GraphDatabase.Driver(uri, authToken);
        //public IDriver CreateDriver(string uri, IAuthToken authToken, Action<ConfigBuilder> action) => GraphDatabase.Driver(uri, authToken, action);
        //public IDriver CreateDriver(Uri uri, IAuthToken authToken, Action<ConfigBuilder> action) => GraphDatabase.Driver(uri, authToken, action);
    }
}
