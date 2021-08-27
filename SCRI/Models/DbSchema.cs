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
        private List<EdgeTypeBetweenTwoNodes> _connectedEdgeAndNodeTypes { get; set; }
        private List<string> _looseNodeTypes { get; set; }

        public DbSchema()
        {
            _connectedEdgeAndNodeTypes = new List<EdgeTypeBetweenTwoNodes>();
            _looseNodeTypes = new List<string>();
        }

        public void AddLooseNodeType(string nodeLabel)
        {
            _looseNodeTypes.Add(nodeLabel);
        }

        public void AddLooseNodeTypes(IEnumerable<string> nodeLabels)
        {
            _looseNodeTypes.AddRange(nodeLabels);
        }

        public void AddEdgeTypeBetweenTwoNodes(string sourceNodeLabel, string targetNodeLabel, string edgeType)
        {
            _connectedEdgeAndNodeTypes.Add(new EdgeTypeBetweenTwoNodes()
            {
                SourceNodeLabel = sourceNodeLabel,
                TargetNodeLabel = targetNodeLabel,
                EdgeType = edgeType
            });
        }

        public IEnumerable<string> GetUniqueNodeLabels()
        {
            return _connectedEdgeAndNodeTypes
                .SelectMany(x => new[] { x.SourceNodeLabel, x.TargetNodeLabel })
                .Union(_looseNodeTypes)
                .Distinct()
                .Where(s => !string.IsNullOrEmpty(s));
        }

        public IEnumerable<string> GetUniqueEdgeTypes()
        {
            return _connectedEdgeAndNodeTypes.Select(x => x.EdgeType).Distinct().Where(s => !string.IsNullOrEmpty(s));
        }

        /// <summary>
        /// Class that represent a data structure like: NodeLabel-[EdgeType]->NodeLabel
        /// </summary>
        public class EdgeTypeBetweenTwoNodes
        {
            public string SourceNodeLabel { get; set; }
            public string TargetNodeLabel { get; set; }
            public string EdgeType { get; set; }
        }
    }


}
