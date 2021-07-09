using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver;
using SCRI.Models;

namespace SCRI.Database
{
    public class GraphDbConnection : IDisposable
    {
        private IDriver _driver;
        public string connectionStatus { get; set; }

        public bool SetUpDriver(string uri, string user, string password)
        {
            connectionStatus = "Not connected";
            try
            {
                _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password), o => o.WithConnectionAcquisitionTimeout(new TimeSpan(3600)));
            }
            catch (Exception e)
            {
                connectionStatus = e.Message;
                return false;
            }
            return true;
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

        public ISession GetSession() => _driver.Session();

        public void Dispose()
        {
            _driver?.Dispose();
        }
    }
}
