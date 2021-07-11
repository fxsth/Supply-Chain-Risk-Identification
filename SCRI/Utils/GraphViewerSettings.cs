using Microsoft.Msagl.Drawing;
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

        private Color GetRandomMSAGLColor()
        {
            Random rnd = new Random();
            Byte[] b = new Byte[3];
            rnd.NextBytes(b);
            return new Color(b[0], b[1], b[2]);
        }
    }
}
