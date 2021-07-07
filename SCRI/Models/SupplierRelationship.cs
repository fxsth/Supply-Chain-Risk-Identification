using GraphX.Common.Models;
using QuikGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCRI.Models
{
    public class SupplierRelationship : EdgeBase<Supplier>, IEdge<Supplier>
    {
        public SupplierRelationship(int id, Supplier source, Supplier target, string type)
            : base(source, target)
        {
            Id = id;
            Type = type;
        }

        public int Id { get; set; }
        public string Type { get; }
        public Dictionary<string, string> Properties { get; set; }
        public override string ToString() => Type;
    }
}
