using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCRI.Utils
{
    public static class Neo4jTypeConverter
    {
        public static Models.Supplier CreateSupplierFromINode(Neo4j.Driver.INode node)
        {
            return new Models.Supplier( (int) node.Id, node.Labels, node.Properties.ToDictionary(i => i.Key, i => i.Value.ToString()));
        }

        public static Models.SupplierRelationship CreateSupplierRelationshipFromIRelationship(Neo4j.Driver.IRelationship relationship, Models.Supplier source, Models.Supplier target)
        {
            return new Models.SupplierRelationship((int)relationship.Id, source, target,relationship.Type);
        }
    }
}
