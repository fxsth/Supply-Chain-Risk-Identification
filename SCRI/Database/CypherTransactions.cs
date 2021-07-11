using Neo4j.Driver;
using SCRI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCRI.Database
{
    public static class CypherTransactions
    {

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

        public static SupplyNetwork GetCompleteGraph(ITransaction tx)
        {
            var nodes = tx.Run("MATCH (n) RETURN n").ToList();
            var edges = tx.Run("MATCH ()-[r]->() Return r").ToList();
            var suppliers = nodes.ConvertAll(
                new Converter<IRecord, Supplier>(x => Utils.Neo4jTypeConverter.CreateSupplierFromINode(x.Values.FirstOrDefault().Value.As<INode>()))
                );
            Models.SupplyNetwork supplyNetwork = new SupplyNetwork();
            supplyNetwork.AddVertexRange(suppliers);
            var suppliersDict = suppliers.ToDictionary(x => x.ID, x => x);
            foreach (var edge in edges)
            {

                var r = Utils.Neo4jTypeConverter.CreateSupplierRelationshipFromIRelationship(edge.Values["r"].As<IRelationship>(), suppliersDict[(int)edge.Values["r"].As<IRelationship>().StartNodeId], suppliersDict[(int)edge.Values["r"].As<IRelationship>().EndNodeId]);
                supplyNetwork.AddEdge(r);
            }
            return supplyNetwork;
        }

        public static List<string> GetAvailableProcedures(ITransaction tx)
        {
            var res = tx.Run("CALL dbms.procedures()");
            var list = res.ToList();
            return list.ConvertAll(
                new Converter<IRecord, string>(x => x.Values.FirstOrDefault().Value.ToString())
                );
        }

        public static List<string> GetDatabases(ITransaction tx)
        {
            var res = tx.Run("Show Databases");
            var list = res.ToList();
            return list.ConvertAll(
                new Converter<IRecord, string>(x => x.Values.FirstOrDefault().Value.ToString())
                );
        }

        public static DbSchema GetDatabaseSchema(ITransaction tx)
        {
            var res = tx.Run("call db.schema.visualization()");
            var list = res.ToList();
            var nodesDictionary = list.First().Values["nodes"].As<IEnumerable<INode>>().ToDictionary(x => x.Id);
            var edges = list.First().Values["relationships"].As<IEnumerable<IRelationship>>();
            var schema = new DbSchema();
            foreach (var edge in edges)
            {
                schema.AddEdgeTypeBetweenTwoNodes(
                   nodesDictionary[edge.StartNodeId].Labels.First(),
                   nodesDictionary[edge.EndNodeId].Labels.First(),
                   edge.Type
                );
            }
            return schema;
        }


        // TODO:
        // Named Graph for Graph Data Science Library
        // Transactions for GDS-Procedure-Calls
        // Transaction retrieving scheme
    }
}
