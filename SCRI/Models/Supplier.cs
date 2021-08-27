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

        /// <summary>
        /// Get node's name or title 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string nodeText;
            var properties = Properties.Keys.AsEnumerable();
            if (properties.Any(x => x.Contains("name", System.StringComparison.OrdinalIgnoreCase)))
                nodeText = Properties.Where(x => x.Key.Contains("name", System.StringComparison.OrdinalIgnoreCase)).Select(x => x.Value).FirstOrDefault();
            else if (properties.Any(x => x.Contains("title", System.StringComparison.OrdinalIgnoreCase)))
                nodeText = Properties.Where(x => x.Key.Contains("title", System.StringComparison.OrdinalIgnoreCase)).Select(x => x.Value).FirstOrDefault();
            else
                nodeText = Label.FirstOrDefault() + " " + ID;
            return nodeText;
        }
    }
}
