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

        public List<string> GetAllNodeProperties()
        {
            var session = _driver.Session();
            var nodes = session.ReadTransaction(tx => GetAllNodes(tx));
            return nodes.ConvertAll(
                new Converter<INode, string>(n => n.Properties.FirstOrDefault().Value.ToString())
                );
        }

        public static List<INode> GetAllNodes(ITransaction tx)
        {
            var res = tx.Run("MATCH (n) Return n");
            var list = res.ToList();
            return list.ConvertAll(
                new Converter<IRecord, INode>(x => x.Values.Values.First().As<INode>())
                );
        }

        public static List<KeyValuePair<INode, INode>> GetAllEdges(ITransaction tx)
        {
            var res = tx.Run("MATCH (a)-[]->(b) Return a,b");
            var list = res.ToList();
            return list.ConvertAll(
            new Converter<IRecord, KeyValuePair<INode, INode>>(x => new KeyValuePair<INode, INode>(x.Values.Values.ElementAt(0).As<INode>(), x.Values.Values.ElementAt(1).As<INode>()))
            );
        }

        public static Models.SupplyNetwork GetCompleteGraph(ITransaction tx)
        {
            var nodes = tx.Run("MATCH (n) RETURN n").ToList();
            var edges = tx.Run("MATCH ()-[r]->() Return r").ToList();
            var suppliers = nodes.ConvertAll(
                new Converter<IRecord, Supplier>(x => Utils.Neo4jTypeConverter.CreateSupplierFromINode(x.Values.FirstOrDefault().Value.As<INode>()))
                );
            Models.SupplyNetwork supplyNetwork = new SupplyNetwork();
            supplyNetwork.AddVertexRange(suppliers);
            var suppliersDict = suppliers.ToDictionary( x => x.ID, x=> x);
            foreach (var edge in edges)
            {
                
                var r = Utils.Neo4jTypeConverter.CreateSupplierRelationshipFromIRelationship(edge.Values["r"].As<IRelationship>(), suppliersDict[(int) edge.Values["r"].As<IRelationship>().StartNodeId], suppliersDict[(int) edge.Values["r"].As<IRelationship>().EndNodeId]);
                supplyNetwork.AddEdge(r);      
            }
            return supplyNetwork;
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }
    }
}
