
using QuikGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCRI.Models
{
    public class SupplierRelationship : IEdge<Supplier>
    {
        public SupplierRelationship(int id, Supplier source, Supplier target, string type)
        {
            ID = id;
            Type = type;
            Source = source;
            Target = target;
        }

        public int ID { get; set; }
        public string Type { get; }
        public Dictionary<string, string> Properties { get; set; }

        public Supplier Source { get; set; }
        public Supplier Target { get; set; }

        public override string ToString() => Type;
    }
}
