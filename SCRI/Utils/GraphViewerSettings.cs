using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Drawing;
using SCRI.Models;
using SCRI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using P2 = Microsoft.Msagl.Core.Geometry.Point;

namespace SCRI.Utils
{
    public class GraphViewerSettings
    {
        private readonly string _defaultgraph;
        public NodeSizeDependsOn selectedNodeSizeDependence;
        private Dictionary<string, Color> NodeLabelColor;
        private readonly IGraphStore _graphStore;

        public GraphViewerSettings(IGraphStore graphStore)
        {
            _graphStore = graphStore;
            _defaultgraph = _graphStore.defaultGraph;
        }

        public enum NodeSizeDependsOn
        {
            None,
            DegreeCentrality,
            ClosenessCentrality,
            BetweennessCentrality
        }

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

        public double CentralityMeasureToNodeSize(Supplier node)
        {
            switch (selectedNodeSizeDependence)
            {
                case NodeSizeDependsOn.None:
                    return 50;
                case NodeSizeDependsOn.DegreeCentrality:
                    if (node.Properties.ContainsKey("degree"))
                        return Convert.ToDouble(node.Properties["degree"]) * 20;
                    break;
                case NodeSizeDependsOn.ClosenessCentrality:
                    if (node.Properties.ContainsKey("closeness"))
                        return Convert.ToDouble(node.Properties["closeness"]) * 100 + 20;
                    break;
                case NodeSizeDependsOn.BetweennessCentrality:
                    if (node.Properties.ContainsKey("betweenness"))
                        return Convert.ToDouble(node.Properties["betweenness"]) + 30;
                    break;
            }
            return 50;
        }

        public Graph GetDefaultMSAGLGraph()
        {
            return GetMSAGLGraph(_graphStore.defaultGraph);
        }

        public Graph GetMSAGLGraph(string graphName)
        {
            Graph graph = new Graph();
            var dbSchema = _graphStore.GetDbSchema(graphName);
            var supplyNetwork = _graphStore.GetGraph(graphName);
            AssignColorsToLabels(dbSchema.GetUniqueNodeLabels());
            if (supplyNetwork.Edges.Any())
            {
                foreach (var edge in supplyNetwork.Edges)
                {
                    var n1 = graph.AddNode(edge.Source.ID.ToString());
                    n1.LabelText = edge.Source.ToString();
                    n1.Attr.FillColor = GetLabelColor(edge.Source.Label.First());
                    n1.Attr.Shape = Shape.Circle;
                    n1.NodeBoundaryDelegate = new DelegateToSetNodeBoundary(x => CustomCircleNodeBoundaryCurve(x, CentralityMeasureToNodeSize(edge.Source)));

                    var n2 = graph.AddNode(edge.Target.ID.ToString());
                    n2.LabelText = edge.Target.ToString();
                    n2.Attr.FillColor = GetLabelColor(edge.Target.Label.First());
                    n2.Attr.Shape = Shape.Circle; ;
                    n2.NodeBoundaryDelegate = new DelegateToSetNodeBoundary(x => CustomCircleNodeBoundaryCurve(x, CentralityMeasureToNodeSize(edge.Source)));

                    var e = graph.AddEdge(n1.Id, n2.Id);
                }
            }
            else
            {
                foreach (var vertex in supplyNetwork.Vertices)
                {
                    var n = graph.AddNode(vertex.ID.ToString());
                    n.LabelText = vertex.ToString();
                    n.Attr.FillColor = GetLabelColor(vertex.Label.First());
                    n.Attr.Shape = Shape.Circle;
                    //n.NodeBoundaryDelegate = new DelegateToSetNodeBoundary(x => CustomCircleNodeBoundaryCurve(x, CentralityMeasureToNodeSize(vertex)));

                }
            }
            return graph;
        }

        private static ICurve CustomCircleNodeBoundaryCurve(Node node, double radius)
        {
            return CurveFactory.CreateEllipse(radius, radius, new P2(0, 0));
        }

        private static ICurve GetCustomNodeBoundaryCurve(Node node, double width, double height)
        {
            if (node == null)
                throw new InvalidOperationException();
            NodeAttr nodeAttr = node.Attr;

            switch (nodeAttr.Shape)
            {
                case Shape.Ellipse:
                case Shape.DoubleCircle:
                    return CurveFactory.CreateEllipse(width, height, new P2(0, 0));
                case Shape.Circle:
                    {
                        double r = Math.Max(width / 2, height / 2);
                        return CurveFactory.CreateEllipse(r, r, new P2(0, 0));
                    }

                case Shape.Box:
                    if (nodeAttr.XRadius != 0 || nodeAttr.YRadius != 0)
                        return CurveFactory.CreateRectangleWithRoundedCorners(width, height, nodeAttr.XRadius,
                                                                             nodeAttr.YRadius, new P2(0, 0));
                    return CurveFactory.CreateRectangle(width, height, new P2(0, 0));


                case Shape.Diamond:
                    return CurveFactory.CreateDiamond(
                      width, height, new P2(0, 0));

                case Shape.House:
                    return CurveFactory.CreateHouse(width, height, new P2());

                case Shape.InvHouse:
                    return CurveFactory.CreateInvertedHouse(width, height, new P2());
                case Shape.Hexagon:
                    return CurveFactory.CreateHexagon(width, height, new P2());
                case Shape.Octagon:
                    return CurveFactory.CreateOctagon(width, height, new P2());

                default:
                    {
                        return new Ellipse(
                          new P2(width / 2, 0), new P2(0, height / 2), new P2());
                    }
            }
        }

        private string StringForcedToSize(string input, int size)
        {
            if (size < input.Length)
                return input.Substring(0, size);
            else
                return input + new string(' ', size - input.Length);
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
