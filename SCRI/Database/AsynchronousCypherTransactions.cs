using Neo4j.Driver;
using SCRI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCRI.Database
{
    public static partial class CypherTransactions
    {
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
        
        public static async Task<List<KeyValuePair<INode, INode>>> GetAllEdgesAsync(IAsyncTransaction tx, string labelFilter)
        {
            List<IRecord> edges = new List<IRecord>();
            var reader = await tx.RunAsync($"MATCH (a : {labelFilter})-[]->(b:{labelFilter}) Return a,b");
            while (await reader.FetchAsync())
            {
                edges.Add(reader.Current as IRecord);
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

        public static async Task<SupplyNetwork> GetGraphFilteredByLabelsAsync(IAsyncTransaction tx, IEnumerable<string> labelFilter)
        {
            var nodesResult = await tx.RunAsync(CreateCypherQueryNodeFilterByLabels(labelFilter));
            var edgesResult = await tx.RunAsync(CreateCypherQueryEdgeFilterByNodeLabels(labelFilter));
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
            var res = await tx.RunAsync(@"CALL gds.degree.stream('$graphProjection')
                            YIELD nodeId, score
                            RETURN nodeId, score", graphProjection);
            while (await res.FetchAsync())
            {
                var record = (res.Current as IRecord);
                dict[record.Values["nodeId"].As<int>()] = record.Values["score"].As<double>();
            }
            return dict;
        }

        public static async Task<Dictionary<int, double>> GetDegreeCentralityAsStreamAsync(IAsyncTransaction tx)
        {
            Dictionary<int, double> dict = new Dictionary<int, double>();
            var res = await tx.RunAsync(@"CALL gds.degree.stream({nodeProjection: '*', relationshipProjection: '*'})
                            YIELD nodeId, score
                            RETURN nodeId, score");
            while (await res.FetchAsync())
            {
                var record = (res.Current as IRecord);
                dict[record.Values["nodeId"].As<int>()] = record.Values["score"].As<double>();
            }
            return dict;
        }

        public static async Task WriteDegreeCentralityToPropertyAsync(IAsyncTransaction tx)
        {
            await tx.RunAsync("CALL gds.degree.write({nodeProjection: '*', relationshipProjection: '*', writeProperty: 'degree'})");
        }

        public static async Task WriteBetweennessCentralityToPropertyAsync(IAsyncTransaction tx)
        {
            await tx.RunAsync("CALL gds.betweenness.write({nodeProjection: '*', relationshipProjection: '*', writeProperty: 'betweenness'})");
        }

        public static async Task WriteClosenessCentralityToPropertyAsync(IAsyncTransaction tx)
        {
            await tx.RunAsync("CALL gds.alpha.closeness.write({nodeProjection: '*', relationshipProjection: '*', writeProperty: 'closeness'})");
        }

        public static async Task WriteCentralityMeasuresToPropertyAsync(IAsyncTransaction tx)
        {
            var degreeTask = WriteDegreeCentralityToPropertyAsync(tx);
            var betweennessTask = WriteBetweennessCentralityToPropertyAsync(tx);
            var closenessTask = WriteClosenessCentralityToPropertyAsync(tx);
            // execute parallel
            await Task.WhenAll(degreeTask, betweennessTask, closenessTask);
        }

        public static async Task<Dictionary<int, int>> GetOutsourcingAssociations(IAsyncTransaction tx, string labelSupplier, string labelProduct)
        {
            string query = $"MATCH (s1:{labelSupplier})-[]->(p1:{labelProduct})-[]->(p2:{labelProduct})-[]-(s2:{labelSupplier}) RETURN s1, s2";
            var res = await tx.RunAsync(query);
            Dictionary<int, int> supplierOutsourcingAssociations = new Dictionary<int, int>();
            while (await res.FetchAsync())
            {
                var s1 = (res.Current[0] as INode);
                var s2 = (res.Current[1] as INode);
                supplierOutsourcingAssociations[s1.Id.As<int>()] = s2.Id.As<int>();
            }
            return supplierOutsourcingAssociations;
        }

        public static async Task<Dictionary<int, int>> GetBuyerAssociations(IAsyncTransaction tx, string labelSupplier)
        {
            string query = $"MATCH (seller1:{labelSupplier})-[]->(buyer:{labelSupplier})<-[]-(seller2:{labelSupplier}) RETURN seller1, seller2";
            var res = await tx.RunAsync(query);
            Dictionary<int, int> supplierOutsourcingAssociations = new Dictionary<int, int>();
            while (await res.FetchAsync())
            {
                var seller1 = (res.Current[0] as INode);
                var seller2 = (res.Current[1] as INode);
                supplierOutsourcingAssociations[seller1.Id.As<int>()] = seller2.Id.As<int>();
            }
            return supplierOutsourcingAssociations;
        }

        public static async Task<Dictionary<int, int>> GetCompetitionAssociations(IAsyncTransaction tx, string labelSupplier)
        {
            string query = $"MATCH (s1:{labelSupplier})-[]->(product:{labelSupplier})<-[]-(s2:{labelSupplier}) RETURN s1, s2";
            var res = await tx.RunAsync(query);
            Dictionary<int, int> competitionOutsourcingAssociations = new Dictionary<int, int>();
            while (await res.FetchAsync())
            {
                var s1 = (res.Current[0] as INode);
                var s2 = (res.Current[1] as INode);
                competitionOutsourcingAssociations[s1.Id.As<int>()] = s2.Id.As<int>();
            }
            return competitionOutsourcingAssociations;
        }

        public static async Task<List<(int, int)>> GetCrossProduct(IAsyncTransaction tx, string labelSupplier)
        {
            string query = $"MATCH (s1:{labelSupplier}), (s2:{labelSupplier}) RETURN s1, s2";
            var res = await tx.RunAsync(query);
            List<(int, int)> crossProduct = new List<(int, int)>();
            while (await res.FetchAsync())
            {
                var s1 = (res.Current[0] as INode);
                var s2 = (res.Current[1] as INode);
                crossProduct.Add((s1.Id.As<int>(), s2.Id.As<int>()));
            }
            return crossProduct;
        }


        private static string CreateCypherQueryNodeFilterByLabels(IEnumerable<string> labels)
        {
            if (!labels.Any())
                return "MATCH (n) RETURN n";
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var label in labels)
            {
                if (stringBuilder.Length == 0)
                {
                    stringBuilder.Append($"MATCH (n) WHERE n:{label}");
                }
                else
                {
                    stringBuilder.Append($" OR n:{label}");
                }
            }
            stringBuilder.Append(" RETURN n");
            return stringBuilder.ToString();
        }

        private static string CreateCypherQueryEdgeFilterByNodeLabels(IEnumerable<string> labels)
        {
            if (!labels.Any())
                return "MATCH ()-[r]->() Return r";
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var label in labels)
            {
                if (stringBuilder.Length == 0)
                {
                    stringBuilder.Append($"MATCH (l1)-[r]->(l2) WHERE l1:{label} AND l2:{label}");
                }
                else
                {
                    stringBuilder.Append($" OR l1:{label} AND l2:{label}");
                }
            }
            stringBuilder.Append(" RETURN r");
            return stringBuilder.ToString();
        }
    }
}
