using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Drawing;
using SCRI.Models;
using SCRI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using MachineLearning.Models;
using P2 = Microsoft.Msagl.Core.Geometry.Point;

namespace SCRI.Utils
{
    public enum NodeSizeDependsOn
    {
        None,
        DegreeCentrality,
        ClosenessCentrality,
        BetweennessCentrality
    }

    public class GraphViewerSettings
    {
        public NodeSizeDependsOn SelectedNodeSizeDependence;
        private readonly Dictionary<string, Color> _nodeLabelColor = new Dictionary<string, Color>();
        private readonly IGraphStore _graphStore;
        private double? _minScore;
        private double? _maxScore;

        private static readonly string[] ColourValues = new string[]
        {
            "#85D2D6", "#82C95E", "#D5CD81", "#859AD6", "#859AD6", "#895EC9", "#226067",
            "#5A7E2A", "#B4A63C", "#2A7728", "#C18844", "#BAD071", "#91307B", "A83840",
            "#2471A3", "#F0B27A", "#E74C3C", "#1ABC9C", "#C0392B ", "#BB8FCE", "#8E44AD",
            "#F39C12", "#2980B9", "#7DCEA0", "#3498DB", "#ECF0F1", "#16A085", "#D35400",
            "#27AE60", "#86402D", "#54521C", "#1B5039", "#1B2E50", "#C34B51"
        };
        
        
        public enum ShowPredictedLinks
        {
            OnlyAdditionalExisting,
            AllExisting,
            AllNonExisting
        }

        public GraphViewerSettings(IGraphStore graphStore)
        {
            _graphStore = graphStore;
        }

        private void AssignColorsToLabels(IEnumerable<string> labels)
        {
            var enumerable = labels.ToList();
            for (int i = 0; i < enumerable.Count(); i++)
            {
                var color = System.Drawing.ColorTranslator
                    .FromHtml(ColourValues[i]); // rnd.Next(ColourValues.Length-1)]);
                _nodeLabelColor[enumerable.ElementAt(i)] = new Color(color.R, color.G, color.B);
            }
        }

        public Color GetLabelColor(string label)
        {
            if (label == null)
                return Color.White;
            return _nodeLabelColor[label];
        }

        private void PrepareNodeSizeNormalization(string graphname)
        {
            var dict = new Dictionary<NodeSizeDependsOn, string>
            {
                {NodeSizeDependsOn.DegreeCentrality, "degree"},
                {NodeSizeDependsOn.ClosenessCentrality, "closeness"},
                {NodeSizeDependsOn.BetweennessCentrality, "betweenness"}
            };
            if (!dict.ContainsKey(SelectedNodeSizeDependence))
                return;
            string scoreProperty = dict[SelectedNodeSizeDependence];
            var graph = _graphStore.GetGraph(graphname);
            _minScore = null;
            _maxScore = null;
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
            if (_minScore is null || _maxScore is null || _maxScore == 0)
                return nodesize;
            double range = (double) (_maxScore - _minScore);
            switch (SelectedNodeSizeDependence)
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
            foreach (var vertex in supplyNetwork.Vertices)
            {
                var n = graph.AddNode(vertex.ID.ToString());
                n.LabelText = vertex.ToString();
                n.Attr.FillColor = GetLabelColor(vertex.Label.First());
                n.Attr.Shape = Shape.Circle;
                n.NodeBoundaryDelegate = x => CustomCircleNodeBoundaryCurve(x, CentralityMeasureToNodeSize(vertex));
            }
            foreach (var edge in supplyNetwork.Edges)
            {
                var e = graph.AddEdge(edge.Source.ID.ToString(), edge.Target.ID.ToString());
            }

            return graph;
        }

        public Graph AddPredictedLinksToGraph(Graph graph, Dictionary<(int, int), PredictedSupplyChainLink> predictedExistingLinks,
            ShowPredictedLinks showPredictedLinks)
        {
            List<(int, int)> edges =
                graph.Edges.Select(edge => (Convert.ToInt32(edge.Source), Convert.ToInt32(edge.Target))).ToList();
            // Remove node pair duplicate (a,b) == (b,a)
            var deduplicatedPredictingLinks = DeduplicateNodePairs(predictedExistingLinks);
            IEnumerable<(int, int)> addedLinksToGraph = deduplicatedPredictingLinks.Keys;
            Color edgeColor = Color.Green;
            switch (showPredictedLinks)
            {
                case ShowPredictedLinks.AllNonExisting:
                    addedLinksToGraph = deduplicatedPredictingLinks.Where(exists => !exists.Value.PredictedLinkExistence).Select(x => x.Key);
                    edgeColor = Color.Red;
                    break;
                case ShowPredictedLinks.AllExisting:
                    addedLinksToGraph = deduplicatedPredictingLinks.Where(exists => exists.Value.PredictedLinkExistence).Select(x => x.Key);
                    edgeColor = Color.Green;
                    break;
                case ShowPredictedLinks.OnlyAdditionalExisting:
                    addedLinksToGraph = deduplicatedPredictingLinks.Where(exists => exists.Value.PredictedLinkExistence).Select(x => x.Key)
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

        private static Dictionary<(int, int), PredictedSupplyChainLink> DeduplicateNodePairs(Dictionary<(int, int), PredictedSupplyChainLink> predictedSupplyChainLinks)
        {
            Dictionary<(int, int), PredictedSupplyChainLink> deduplicatedLinks =
                new Dictionary<(int, int), PredictedSupplyChainLink>();
            foreach (var nodePair in predictedSupplyChainLinks)
            {
                var sortedPair = (Math.Min(nodePair.Key.Item1, nodePair.Key.Item2),
                    Math.Max(nodePair.Key.Item1, nodePair.Key.Item2));
                if (!deduplicatedLinks.ContainsKey(sortedPair) || deduplicatedLinks.ContainsKey(sortedPair) &&
                    deduplicatedLinks[sortedPair] == null)
                    deduplicatedLinks[sortedPair] = nodePair.Value;
            }

            return deduplicatedLinks;
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