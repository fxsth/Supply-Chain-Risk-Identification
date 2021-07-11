using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCRI.Models
{
    /// <summary>
    /// Stores a Schema from Neo4j as edge types between node types (labels)
    /// </summary>
    public class DbSchema
    {
        public List<EdgeTypeBetweenTwoNodes> Schema { get; set; }

        public void AddEdgeTypeBetweenTwoNodes(string sourceNodeLabel, string targetNodeLabel, string edgeType)
        {
            if (Schema == null)
                Schema = new List<EdgeTypeBetweenTwoNodes>();
            Schema.Add(new EdgeTypeBetweenTwoNodes()
            {
                SourceNodeLabel = sourceNodeLabel,
                TargetNodeLabel = targetNodeLabel,
                EdgeType = edgeType
            });
        }

        public IEnumerable<string> getUniqueNodeLabels()
        {
            return Schema.SelectMany(x => new[] { x.SourceNodeLabel, x.TargetNodeLabel }).Distinct();
        }

        public IEnumerable<string> getUniqueEdgeTypes()
        {
            return Schema.Select(x => x.EdgeType).Distinct();
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
