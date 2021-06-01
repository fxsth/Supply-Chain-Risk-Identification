using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace SCRI.Database
{
    class Neo4JConnection : IDisposable
    {
        private readonly IDriver _driver;
        public string connectionStatus { get; set; }

        public Neo4JConnection(string uri, string user, string password)
        {
            connectionStatus = "Not connected";
            _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password), o => o.WithConnectionAcquisitionTimeout(new TimeSpan(3600)));
        }

        public async Task<string> checkConnectionStatus()
        {
            try
            {
                var connecting = _driver.VerifyConnectivityAsync();
                connectionStatus = "Connecting...";
                await connecting;
                connectionStatus = "Connected";
            }
            catch (Exception e)
            {
                connectionStatus = e.Message;
            }
            finally
            {
                await _driver.CloseAsync();
            }
            return connectionStatus;
        }

        public List<string> GetAllNodeProperties()
        {
            var session = _driver.Session();
            var nodes = session.ReadTransaction(tx => GetAllNodes(tx));
            return nodes.ConvertAll(
                new Converter<INode, string>(n => n.Properties.FirstOrDefault().Value.ToString())
                );
        }

        private static List<INode> GetAllNodes(ITransaction tx)
        {
            var res = tx.Run("MATCH (n) Return n");
            var list = res.ToList();
            return list.ConvertAll(
                new Converter<IRecord, INode>(x => x.Values.Values.First().As<INode>())
                );
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }
    }
}
