using GraphX.Common.Models;
using QuikGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCRI.Models
{
    public class Supplier : VertexBase
    {
        public Supplier(int id, IReadOnlyList<string> label, Dictionary<string, string> properties)
        {
            Label = label;
            Properties = properties;
            base.ID = id;
        }

        public IReadOnlyList<string> Label { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public override string ToString() => Label.FirstOrDefault() + " " + ID;
    }
}
