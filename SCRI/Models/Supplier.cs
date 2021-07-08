using System.Collections.Generic;
using System.Linq;

namespace SCRI.Models
{
    public class Supplier
    {
        public Supplier(int id, IReadOnlyList<string> label, Dictionary<string, string> properties)
        {
            Label = label;
            Properties = properties;
            ID = id;
        }

        public IReadOnlyList<string> Label { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public int ID { get; }
        public override string ToString() => Label.FirstOrDefault() + " " + ID;
    }
}
