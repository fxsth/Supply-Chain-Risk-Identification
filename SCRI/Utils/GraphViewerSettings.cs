using Microsoft.Msagl.Drawing;
using SCRI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCRI.Utils
{
    public class GraphViewerSettings
    {
        private Dictionary<string, Color> NodeLabelColor;

        public void AssignColorsToLabels(IEnumerable<string> labels)
        {
            
            NodeLabelColor = labels.ToDictionary(x => x, x => GetRandomMSAGLColor());
        }

        public Color GetLabelColor(string label)
        {
            if (label == null)
                return Color.White;
            return NodeLabelColor[label];
        }

        public Graph GetGraph(SupplyNetwork supplyNetwork, DbSchema dbSchema)
        {
            Graph graph = new Graph();
            AssignColorsToLabels(dbSchema.getUniqueNodeLabels());
            foreach (var edge in supplyNetwork.Edges)
            {
                var n1 = graph.AddNode(edge.Source.ID.ToString());
                n1.LabelText = edge.Source.ToString();
                n1.Attr.FillColor = GetLabelColor(edge.Source.Label.First());
                var n2 = graph.AddNode(edge.Target.ID.ToString());
                n2.LabelText = edge.Target.ToString();
                n2.Attr.FillColor = GetLabelColor(edge.Target.Label.First());
                var e = graph.AddEdge(n1.Id, n2.Id);
            }
            return graph;
        }

        private Color GetRandomMSAGLColor()
        {
            Random rnd = new Random();
            Byte[] b = new Byte[3];
            rnd.NextBytes(b);
            return new Color(b[0], b[1], b[2]);
        }
    }
}
