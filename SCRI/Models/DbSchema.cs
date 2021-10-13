using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCRI.Models
{
    /// <summary>
    /// Stores a Schema from Neo4j as edge types between node types (labels) and loose node types
    /// </summary>
    public class DbSchema
    {
        private List<EdgeTypeBetweenTwoNodes> ConnectedEdgeAndNodeTypes { get; set; }
        private List<string> LooseNodeTypes { get; set; }

        public DbSchema()
        {
            ConnectedEdgeAndNodeTypes = new List<EdgeTypeBetweenTwoNodes>();
            LooseNodeTypes = new List<string>();
        }

        public void AddLooseNodeType(string nodeLabel)
        {
            LooseNodeTypes.Add(nodeLabel);
        }

        public void AddLooseNodeTypes(IEnumerable<string> nodeLabels)
        {
            LooseNodeTypes.AddRange(nodeLabels);
        }

        public void AddEdgeTypeBetweenTwoNodes(string sourceNodeLabel, string targetNodeLabel, string edgeType)
        {
            ConnectedEdgeAndNodeTypes.Add(new EdgeTypeBetweenTwoNodes()
            {
                SourceNodeLabel = sourceNodeLabel,
                TargetNodeLabel = targetNodeLabel,
                EdgeType = edgeType
            });
        }

        public IEnumerable<string> GetUniqueNodeLabels()
        {
            return ConnectedEdgeAndNodeTypes
                .SelectMany(x => new[] { x.SourceNodeLabel, x.TargetNodeLabel })
                .Union(LooseNodeTypes)
                .Distinct()
                .Where(s => !string.IsNullOrEmpty(s));
        }

        public IEnumerable<string> GetUniqueEdgeTypes()
        {
            return ConnectedEdgeAndNodeTypes.Select(x => x.EdgeType).Distinct().Where(s => !string.IsNullOrEmpty(s));
        }

        /// <summary>
        /// Class that represent a data structure like: NodeLabel-[EdgeType]->NodeLabel
        /// </summary>
        private class EdgeTypeBetweenTwoNodes
        {
            public string SourceNodeLabel { get; set; }
            public string TargetNodeLabel { get; set; }
            public string EdgeType { get; set; }
        }
    }


}
