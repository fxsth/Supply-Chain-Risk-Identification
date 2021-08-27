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

                var r = Utils.Neo4jTypeConverter.CreateSupplierRelationshipFromIRelationship(
                    edge.Values["r"].As<IRelationship>(),
                    suppliersDict[(int)edge.Values["r"].As<IRelationship>().StartNodeId],
                    suppliersDict[(int)edge.Values["r"].As<IRelationship>().EndNodeId]
                    );
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
            var databases = list.ConvertAll(
                new Converter<IRecord, string>(x => x.Values.FirstOrDefault().Value.ToString())
                );
            databases.Remove("system");
            return databases;
        }

        public static string GetDefaultDatabase(ITransaction tx)
        {
            var res = tx.Run("Show Default Database");
            var list = res.ToList();
            return list.Single().Values.FirstOrDefault().Value.ToString();
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
            schema.AddLooseNodeTypes(nodesDictionary.Values.Select(x => x.Labels.FirstOrDefault()).Distinct());
            return schema;
        }


        public static string CreateGraphProjection(ITransaction tx, string graphProjection)
        {
            var res = tx.Run(@"CALL gds.graph.create('$graphProjection', '*', '*')
                                YIELD createdGraphProjection;", graphProjection);
            return res.Peek().Values.First().Value.ToString();
        }

        public static IEnumerable<string> GetGraphProjections(ITransaction tx)
        {
            var res = tx.Run("CALL gds.graph.list");
            return res.ToList().ConvertAll(
                new Converter<IRecord, string>(x => x.Values["graphName"].ToString())
                ); ;
        }

        public static Dictionary<int, double> GetDegreeCentralityAsStream(ITransaction tx, string graphProjection)
        {
            var res = tx.Run(@"CALL gds.alpha.degree.stream('$graphProjection')
                            YIELD nodeId, score
                            RETURN nodeId, score", graphProjection);
            return res.ToDictionary(x => x.Values["nodeId"].As<int>(), x => x.Values["score"].As<double>());
        }

        public static Dictionary<int, double> GetDegreeCentralityAsStream(ITransaction tx)
        {
            var res = tx.Run(@"CALL gds.alpha.degree.stream({nodeProjection: '*', relationshipProjection: '*'})
                            YIELD nodeId, score
                            RETURN nodeId, score");
            return res.ToDictionary(x => x.Values["nodeId"].As<int>(), x => x.Values["score"].As<double>());
        }

        public static void WriteDegreeCentralityToProperty(ITransaction tx)
        {
            tx.Run("CALL gds.alpha.degree.write({nodeProjection: '*', relationshipProjection: '*', writeProperty: 'degree'})");
        }

        public static void WriteBetweennessCentralityToProperty(ITransaction tx)
        {
            tx.Run("CALL gds.betweenness.write({nodeProjection: '*', relationshipProjection: '*', writeProperty: 'betweenness'})");
        }

        public static void WriteClosenessCentralityToProperty(ITransaction tx)
        {
            tx.Run("CALL gds.alpha.closeness.write({nodeProjection: '*', relationshipProjection: '*', writeProperty: 'closeness'})");
        }

        public static object WriteCentralityMeasuresToProperty(ITransaction tx)
        {
            WriteDegreeCentralityToProperty(tx);
            WriteBetweennessCentralityToProperty(tx);
            WriteClosenessCentralityToProperty(tx);
            return null;
        }




        public static async Task<List<INode>> GetAllNodesAsync(IAsyncTransaction tx)
        {
            List<IRecord> nodes = new List<IRecord>();
            var reader = await tx.RunAsync("MATCH (n) Return n");
            while (await reader.FetchAsync())
            {
                nodes.Add(reader.Current[0] as IRecord);
            }
            return nodes.ConvertAll(
                new Converter<IRecord, INode>(x => x.Values.Values.First().As<INode>())
                );
        }

        public static async Task<List<KeyValuePair<INode, INode>>> GetAllEdgesAsync(IAsyncTransaction tx)
        {
            List<IRecord> edges = new List<IRecord>();
            var reader = await tx.RunAsync("MATCH (a)-[]->(b) Return a,b");
            while (await reader.FetchAsync())
            {
                edges.Add(reader.Current[0] as IRecord);
            }
            return edges.ConvertAll(
            new Converter<IRecord, KeyValuePair<INode, INode>>(x => new KeyValuePair<INode, INode>(x.Values.Values.ElementAt(0).As<INode>(), x.Values.Values.ElementAt(1).As<INode>()))
            );
        }

        public static async Task<SupplyNetwork> GetCompleteGraphAsync(IAsyncTransaction tx)
        {
            var nodesResult = await tx.RunAsync("MATCH (n) RETURN n");
            var edgesResult = await tx.RunAsync("MATCH ()-[r]->() Return r");
            var nodes = await nodesResult.ToListAsync();
            var edges = await edgesResult.ToListAsync();
            var suppliers = nodes.ConvertAll(
                new Converter<IRecord, Supplier>(x => Utils.Neo4jTypeConverter.CreateSupplierFromINode(x.Values.FirstOrDefault().Value.As<INode>()))
                );
            Models.SupplyNetwork supplyNetwork = new SupplyNetwork();
            supplyNetwork.AddVertexRange(suppliers);
            var suppliersDict = suppliers.ToDictionary(x => x.ID, x => x);
            foreach (var edge in edges)
            {

                var r = Utils.Neo4jTypeConverter.CreateSupplierRelationshipFromIRelationship(
                    edge.Values["r"].As<IRelationship>(),
                    suppliersDict[(int)edge.Values["r"].As<IRelationship>().StartNodeId],
                    suppliersDict[(int)edge.Values["r"].As<IRelationship>().EndNodeId]
                    );
                supplyNetwork.AddEdge(r);
            }
            return supplyNetwork;
        }

        public static async Task<List<string>> GetAvailableProceduresAsync(IAsyncTransaction tx)
        {
            var res = await tx.RunAsync("CALL dbms.procedures()");
            var list = await res.ToListAsync();
            return list.ConvertAll(
                new Converter<IRecord, string>(x => x.Values.FirstOrDefault().Value.ToString())
                );
        }

        public static async Task<List<string>> GetDatabasesAsync(IAsyncTransaction tx)
        {
            var res = await tx.RunAsync("Show Databases");
            var list = await res.ToListAsync();
            var databases = list.ConvertAll(
                new Converter<IRecord, string>(x => x.Values.FirstOrDefault().Value.ToString())
                );
            databases.Remove("system");
            return databases;
        }

        public static async Task<string> GetDefaultDatabaseAsync(IAsyncTransaction tx)
        {
            var res = await tx.RunAsync("Show Default Database");
            var list = await res.ToListAsync();
            return list.Single().Values.FirstOrDefault().Value.ToString();
        }

        public static async Task<DbSchema> GetDatabaseSchemaAsync(IAsyncTransaction tx)
        {
            var res = await tx.RunAsync("call db.schema.visualization()");
            var list = await res.ToListAsync();
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
            schema.AddLooseNodeTypes(nodesDictionary.Values.Select(x => x.Labels.FirstOrDefault()).Distinct());
            return schema;
        }


        public static async Task<string> CreateGraphProjectionAsync(IAsyncTransaction tx, string graphProjection)
        {
            var res = await tx.RunAsync(@"CALL gds.graph.create('$graphProjection', '*', '*')
                                YIELD createdGraphProjection;", graphProjection);
            await res.FetchAsync();
            return (res.Current[0] as IRecord).Values.First().Value.ToString();
        }

        public static async Task<IEnumerable<string>> GetGraphProjectionsAsync(IAsyncTransaction tx)
        {
            var res = await tx.RunAsync("CALL gds.graph.list");
            return (await res.ToListAsync()).ConvertAll(
                new Converter<IRecord, string>(x => x.Values["graphName"].ToString())
                ); ;
        }

        public static async Task<Dictionary<int, double>> GetDegreeCentralityAsStreamAsync(IAsyncTransaction tx, string graphProjection)
        {
            Dictionary<int, double> dict = new Dictionary<int, double>();
            var res = await tx.RunAsync(@"CALL gds.alpha.degree.stream('$graphProjection')
                            YIELD nodeId, score
                            RETURN nodeId, score", graphProjection);
            while (await res.FetchAsync())
            {
                var record = (res.Current[0] as IRecord);
                dict[record.Values["nodeId"].As<int>()] = record.Values["score"].As<double>();
            }
            return dict;
        }

        public static async Task<Dictionary<int, double>> GetDegreeCentralityAsStreamAsync(IAsyncTransaction tx)
        {
            Dictionary<int, double> dict = new Dictionary<int, double>();
            var res = await tx.RunAsync(@"CALL gds.alpha.degree.stream({nodeProjection: '*', relationshipProjection: '*'})
                            YIELD nodeId, score
                            RETURN nodeId, score");
            while (await res.FetchAsync())
            {
                var record = (res.Current[0] as IRecord);
                dict[record.Values["nodeId"].As<int>()] = record.Values["score"].As<double>();
            }
            return dict;
        }

        public static async Task WriteDegreeCentralityToPropertyAsync(IAsyncTransaction tx)
        {
            await tx.RunAsync("CALL gds.alpha.degree.write({nodeProjection: '*', relationshipProjection: '*', writeProperty: 'degree'})");
        }

        public static async Task WriteBetweennessCentralityToPropertyAsync(IAsyncTransaction tx)
        {
            await tx.RunAsync("CALL gds.betweenness.write({nodeProjection: '*', relationshipProjection: '*', writeProperty: 'betweenness'})");
        }

        public static async Task WriteClosenessCentralityToPropertyAsync(IAsyncTransaction tx)
        {
            await tx .RunAsync("CALL gds.alpha.closeness.write({nodeProjection: '*', relationshipProjection: '*', writeProperty: 'closeness'})");
        }

        public static async Task WriteCentralityMeasuresToPropertyAsync(IAsyncTransaction tx)
        {
            var degreeTask = WriteDegreeCentralityToPropertyAsync(tx);
            var betweennessTask = WriteBetweennessCentralityToPropertyAsync(tx);
            var closenessTask = WriteClosenessCentralityToPropertyAsync(tx);
            await Task.WhenAll(degreeTask, betweennessTask, closenessTask);
        }
    }
}
