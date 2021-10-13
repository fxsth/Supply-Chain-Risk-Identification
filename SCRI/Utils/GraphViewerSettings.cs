using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Drawing;
using SCRI.Models;
using SCRI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using P2 = Microsoft.Msagl.Core.Geometry.Point;

namespace SCRI.Utils
{
    public class GraphViewerSettings
    {
        private readonly string _defaultgraph;
        public NodeSizeDependsOn selectedNodeSizeDependence;
        private Dictionary<string, Color> NodeLabelColor = new Dictionary<string, Color>();
        private readonly IGraphStore _graphStore;
        private double? _minScore;
        private double? _maxScore;

        //    private static string[] ColourValues = new string[] {
        //    "#2471A3", "#F0B27A", "#E74C3C", "#1ABC9C", "#C0392B ", "#BB8FCE", "#8E44AD",
        //    "#F39C12", "#2980B9", "#7DCEA0", "#3498DB", "#ECF0F1", "#16A085", "#D35400",
        //    "#27AE60", "#00C000", "#2ECC71 ", "#C0C000", "#C000C0", "#00C0C0", "#C0C0C0",
        //    "#400000", "#004000", "#000040", "#404000", "#400040", "#004040", "#404040",
        //    "#200000", "#002000", "#000020", "#202000", "#200020", "#002020", "#202020",
        //    "#600000", "#006000", "#000060", "#606000", "#600060", "#006060", "#606060",
        //    "#A00000", "#00A000", "#0000A0", "#A0A000", "#A000A0", "#00A0A0", "#A0A0A0",
        //    "#E00000", "#00E000", "#0000E0", "#E0E000", "#E000E0", "#00E0E0", "#E0E0E0",
        //};
        private static string[] ColourValues = new string[]
        {
            "#85D2D6", "#82C95E", "#D5CD81", "#859AD6", "#859AD6", "#895EC9", "#226067",
            "#5A7E2A", "#B4A63C", "#2A7728", "#C18844", "#BAD071", "#91307B", "A83840",
            "#2471A3", "#F0B27A", "#E74C3C", "#1ABC9C", "#C0392B ", "#BB8FCE", "#8E44AD",
            "#F39C12", "#2980B9", "#7DCEA0", "#3498DB", "#ECF0F1", "#16A085", "#D35400",
            "#27AE60", "#86402D", "#54521C", "#1B5039", "#1B2E50", "#C34B51"
        };

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

        public enum ShowPredictedLinks
        {
            OnlyAdditionalExisting,
            AllExisting,
            AllNonExisting
        }

        public void AssignColorsToLabels(IEnumerable<string> labels)
        {
            for (int i = 0; i < labels.Count(); i++)
            {
                Random rnd = new Random();
                var color = System.Drawing.ColorTranslator
                    .FromHtml(ColourValues[i]); // rnd.Next(ColourValues.Length-1)]);
                NodeLabelColor[labels.ElementAt(i)] = new Color(color.R, color.G, color.B);
            }
        }

        public Color GetLabelColor(string label)
        {
            if (label == null)
                return Color.White;
            return NodeLabelColor[label];
        }

        private void PrepareNodeSizeNormalization(string graphname)
        {
            var dict = new Dictionary<NodeSizeDependsOn, string>
            {
                {NodeSizeDependsOn.DegreeCentrality, "degree"},
                {NodeSizeDependsOn.ClosenessCentrality, "closeness"},
                {NodeSizeDependsOn.BetweennessCentrality, "betweenness"}
            };
            if (!dict.ContainsKey(selectedNodeSizeDependence))
                return;
            string scoreProperty = dict[selectedNodeSizeDependence];
            var graph = _graphStore.GetGraph(graphname);
            foreach (var node in graph.Vertices)
            {
                double score = Convert.ToDouble(node.Properties[scoreProperty]);
                if (_minScore is null || score < _minScore)
                    _minScore = score;
                if (_maxScore is null || score > _maxScore)
                    _maxScore = score;
            }
        }

        public double CentralityMeasureToNodeSize(Supplier node)
        {
            double nodesize = 50;
            if (_minScore is null || _maxScore is null)
                return 50;
            double range = (double) (_maxScore - _minScore);
            switch (selectedNodeSizeDependence)
            {
                case NodeSizeDependsOn.None:
                    break;
                case NodeSizeDependsOn.DegreeCentrality:
                    if (node.Properties.ContainsKey("degree"))
                        nodesize = Convert.ToDouble(node.Properties["degree"]) / range * 60 + 30;
                    break;
                case NodeSizeDependsOn.ClosenessCentrality:
                    if (node.Properties.ContainsKey("closeness"))
                        nodesize = Convert.ToDouble(node.Properties["closeness"]) / range * 60 + 30;
                    break;
                case NodeSizeDependsOn.BetweennessCentrality:
                    if (node.Properties.ContainsKey("betweenness"))
                        nodesize = Convert.ToDouble(node.Properties["betweenness"]) / range * 60 + 30;
                    break;
            }

            return nodesize == 0 ? 10 : nodesize;
        }

        public Graph GetDefaultMsaglGraph()
        {
            return GetMsaglGraph(_graphStore.defaultGraph);
        }

        public Graph GetMsaglGraph(string graphName)
        {
            Graph graph = new Graph();
            var dbSchema = _graphStore.GetDbSchema(graphName);
            var supplyNetwork = _graphStore.GetGraph(graphName);
            AssignColorsToLabels(dbSchema.GetUniqueNodeLabels());
            PrepareNodeSizeNormalization(graphName);
            if (supplyNetwork.Edges.Any())
            {
                foreach (var edge in supplyNetwork.Edges)
                {
                    var n1 = graph.AddNode(edge.Source.ID.ToString());
                    n1.LabelText = edge.Source.ToString();
                    n1.Attr.FillColor = GetLabelColor(edge.Source.Label.First());
                    n1.Attr.Shape = Shape.Circle;
                    n1.NodeBoundaryDelegate = new DelegateToSetNodeBoundary(x =>
                        CustomCircleNodeBoundaryCurve(x, CentralityMeasureToNodeSize(edge.Source)));

                    var n2 = graph.AddNode(edge.Target.ID.ToString());
                    n2.LabelText = edge.Target.ToString();
                    n2.Attr.FillColor = GetLabelColor(edge.Target.Label.First());
                    n2.Attr.Shape = Shape.Circle;
                    ;
                    n2.NodeBoundaryDelegate = new DelegateToSetNodeBoundary(x =>
                        CustomCircleNodeBoundaryCurve(x, CentralityMeasureToNodeSize(edge.Target)));

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

        public Graph AddPredictedLinksToGraph(Graph graph, Dictionary<(int, int), bool> predictedExistingLinks,
            ShowPredictedLinks showPredictedLinks)
        {
            List<(int, int)> edges =
                graph.Edges.Select(edge => (Convert.ToInt32(edge.Source), Convert.ToInt32(edge.Target))).ToList();
            IEnumerable<(int, int)> addedLinksToGraph = predictedExistingLinks.Keys;
            Color edgeColor = Color.Green;
            switch (showPredictedLinks)
            {
                case ShowPredictedLinks.AllNonExisting:
                    addedLinksToGraph = predictedExistingLinks.Where(exists => !exists.Value).Select(x => x.Key);
                    edgeColor = Color.Red;
                    break;
                case ShowPredictedLinks.AllExisting:
                    addedLinksToGraph = predictedExistingLinks.Where(exists => exists.Value).Select(x => x.Key);
                    edgeColor = Color.Green;
                    break;
                case ShowPredictedLinks.OnlyAdditionalExisting:
                    addedLinksToGraph = predictedExistingLinks.Where(exists => exists.Value).Select(x => x.Key)
                        .Where(x => !edges.Contains(x) && !edges.Contains((x.Item2, x.Item1)));
                    edgeColor = Color.Green;
                    break;
            }

            foreach (var addEdge in addedLinksToGraph)
            {
                var edge = graph.AddEdge(addEdge.Item1.ToString(), addEdge.Item2.ToString());
                edge.Attr.Color = edgeColor;
                edge.LabelText = "Predicted Link";
                edge.Attr.ArrowheadAtSource = ArrowStyle.Diamond;
                edge.Attr.ArrowheadAtTarget = ArrowStyle.Diamond;
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

        private Color GetRandomMsaglColor()
        {
            Random rnd = new Random();
            Byte[] b = new Byte[3];
            rnd.NextBytes(b);
            return new Color(b[0], b[1], b[2]);
        }
    }
}