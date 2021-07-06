using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Layout.Layered;
using Neo4j.Driver;
using SCRI.Database;
using Ellipse = Microsoft.Msagl.Core.Geometry.Curves.Ellipse;
using Node = Microsoft.Msagl.Core.Layout.Node;
using Shape = Microsoft.Msagl.Drawing.Shape;
using Microsoft.Msagl.Miscellaneous;
using Microsoft.Msagl.Routing;
using P = Microsoft.Msagl.Core.Geometry.Point;
using Edge = Microsoft.Msagl.Core.Layout.Edge;
using GraphX.Controls;
using QuickGraph;
using GraphX.Logic.Models;
using GraphX.Common.Models;
using GraphX.Logic.Algorithms.LayoutAlgorithms;
using GraphX.Common.Enums;
using GraphX.Logic.Algorithms.OverlapRemoval;

namespace SCRI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DriverFactory _driverFactory;
        private IDriver driver;

        public MainWindow(IDriverFactory driverFactory)
        {
            InitializeComponent();
            _driverFactory = driverFactory as DriverFactory;
            Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            //GeometryGraph graph = new GeometryGraph();
            driver = _driverFactory.CreateDriver();
            var session = driver.Session();
            var edges = session.ReadTransaction(tx => GetAllEdges(tx));
            var nodes = session.ReadTransaction(tx => GetAllNodes(tx));
            foreach (var edge in edges)
            {
                
                //var addedEdge = graph.AddEdge(edge.Key.Id.ToString(), edge.Value.Id.ToString());


                //addedEdge.LabelText = "edgeLabelText";
                //Node n1 = graph.FindNode(edge.Key.Id.ToString());
                //Node n2 = graph.FindNode(edge.Value.Id.ToString());
                //n1.LabelText = "nodeLabelText";

            }
            //var list = nodes.ConvertAll(
            //    new Converter<INode, string>(n => n.Properties.FirstOrDefault().Value.ToString())
            //    );
            //double w = 30;
            //double h = 20;
            //Node a = new Node(new Ellipse(w, h, new P()), "a");
            //Node b = new Node(CurveFactory.CreateRectangle(w, h, new P()), "b");
            //Node c = new Node(CurveFactory.CreateRectangle(w, h, new P()), "c");
            //Node d = new Node(CurveFactory.CreateRectangle(w, h, new P()), "d");

            //graph.Nodes.Add(a);
            //graph.Nodes.Add(b);
            //graph.Nodes.Add(c);
            //graph.Nodes.Add(d);
            //Edge edge1 = new Edge(a, b) { Length = 10 };
            //graph.Edges.Add(edge1);
            //graph.Edges.Add(new Edge(b, c) { Length = 3 });
            //graph.Edges.Add(new Edge(b, d) { Length = 4 });

            //graph.AddEdge("A", "B");
            //graph.AddEdge("B", "C");
            //graph.AddEdge("A", "C").Attr.Color = Microsoft.Msagl.Drawing.Color.Green;
            //graph.FindNode("A").Attr.FillColor = Microsoft.Msagl.Drawing.Color.Magenta;
            //graph.FindNode("B").Attr.FillColor = Microsoft.Msagl.Drawing.Color.MistyRose;
            //Microsoft.Msagl.Drawing.Node c = graph.FindNode("C");
            //c.Attr.FillColor = Microsoft.Msagl.Drawing.Color.PaleGreen;
            //c.Attr.Shape = Microsoft.Msagl.Drawing.Shape.Diamond;

            //graph.Attr.LayerDirection = LayerDirection.LR;


            //var settings = new Microsoft.Msagl.Layout.Layered.SugiyamaLayoutSettings();
            //var settings = new SugiyamaLayoutSettings
            //{
            //    Transformation = PlaneTransformation.Rotation(Math.PI / 2),
            //    EdgeRoutingSettings = { EdgeRoutingMode = EdgeRoutingMode.Spline }
            //};

            //GeometryGraphCreator.CreateLayoutSettings(graph);
            //graph.Attr.OptimizeLabelPositions = true;

            //graph.Save("c:\\tmp\\saved.msagl");
            //var settings = new Microsoft.Msagl.Layout.MDS.MdsLayoutSettings();
            //LayoutHelpers.CalculateLayout(graph, settings, null);

            //Graph drawGraph = new Graph();
            //drawGraph.GeometryGraph = graph;
            //graphControl.Graph = drawGraph;

            Random Rand = new Random();

            //Create data graph object
            var graph = new GraphExample();

            //Create and add vertices using some DataSource for ID's
            foreach(var node in nodes)
                graph.AddVertex(new DataVertex() { ID = node.Id, Text = node.Labels.FirstOrDefault() });

            var vlist = graph.Vertices.ToList();
            //Generate random edges for the vertices
            foreach (var edge in edges)
            {
                if (Rand.Next(0, 50) > 25) continue;
                var vertex1 = vlist.Find(x => x.ID == edge.Key.Id); 
                var vertex2 = vlist.Find(x => x.ID == edge.Value.Id);
                graph.AddEdge(new DataEdge(vertex1, vertex2)
                { Text = string.Format("{0} -> {1}", vertex1, vertex2) });
            }

            var LogicCore = new GXLogicCoreExample();
            //This property sets layout algorithm that will be used to calculate vertices positions
            //Different algorithms uses different values and some of them uses edge Weight property.
            LogicCore.DefaultLayoutAlgorithm = GraphX.Common.Enums.LayoutAlgorithmTypeEnum.KK;
            //Now we can set optional parameters using AlgorithmFactory
            //NOTE: default parameters can be automatically created each time you change Default algorithms
            LogicCore.DefaultLayoutAlgorithmParams =
                               LogicCore.AlgorithmFactory.CreateLayoutParameters(GraphX.Common.Enums.LayoutAlgorithmTypeEnum.KK);
            //Unfortunately to change algo parameters you need to specify params type which is different for every algorithm.
            ((KKLayoutParameters)LogicCore.DefaultLayoutAlgorithmParams).MaxIterations = 100;

            //This property sets vertex overlap removal algorithm.
            //Such algorithms help to arrange vertices in the layout so no one overlaps each other.
            LogicCore.DefaultOverlapRemovalAlgorithm = GraphX.Common.Enums.OverlapRemovalAlgorithmTypeEnum.FSA;
            //Setup optional params
            LogicCore.DefaultOverlapRemovalAlgorithmParams =
                              LogicCore.AlgorithmFactory.CreateOverlapRemovalParameters(GraphX.Common.Enums.OverlapRemovalAlgorithmTypeEnum.FSA);
            ((OverlapRemovalParameters)LogicCore.DefaultOverlapRemovalAlgorithmParams).HorizontalGap = 50;
            ((OverlapRemovalParameters)LogicCore.DefaultOverlapRemovalAlgorithmParams).VerticalGap = 50;

            //This property sets edge routing algorithm that is used to build route paths according to algorithm logic.
            //For ex., SimpleER algorithm will try to set edge paths around vertices so no edge will intersect any vertex.
            LogicCore.DefaultEdgeRoutingAlgorithm = GraphX.Common.Enums.EdgeRoutingAlgorithmTypeEnum.SimpleER;

            //This property sets async algorithms computation so methods like: Area.RelayoutGraph() and Area.GenerateGraph()
            //will run async with the UI thread. Completion of the specified methods can be catched by corresponding events:
            //Area.RelayoutFinished and Area.GenerateGraphFinished.
            LogicCore.AsyncAlgorithmCompute = false;

            //Finally assign logic core to GraphArea object
            gg_Area.LogicCore = LogicCore;

            //gg_zoomctrl.Zoom = 0.01; //disable zoom control auto fill animation by setting this value
            gg_Area.GenerateGraph(graph);
            //gg_zoomctrl.ZoomToFill();//manually update zoom control to fill the area
        }

        private static List<KeyValuePair<INode, INode>> GetAllEdges(ITransaction tx)
        {
            var res = tx.Run("MATCH (a)-[]->(b) Return a,b");
            var list = res.ToList();
            return list.ConvertAll(
            new Converter<IRecord, KeyValuePair<INode, INode>>(x => new KeyValuePair<INode, INode>(x.Values.Values.ElementAt(0).As<INode>(), x.Values.Values.ElementAt(1).As<INode>()))
            );
        }

        private static List<INode> GetAllNodes(ITransaction tx)
        {
            var res = tx.Run("MATCH (n) Return n");
            var list = res.ToList();
            return list.ConvertAll(
                new Converter<IRecord, INode>(x => x.Values.Values.First().As<INode>())
                );
        }
    }

    //Layout visual class
    public class GraphAreaExample : GraphArea<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>> { }

    //Graph data class
    public class GraphExample : BidirectionalGraph<DataVertex, DataEdge> { }

    //Logic core class
    public class GXLogicCoreExample : GXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>> { }

    //Vertex data object
    public class DataVertex : VertexBase
    {
        /// <summary>
        /// Some string property for example purposes
        /// </summary>
        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }

    //Edge data object
    public class DataEdge : EdgeBase<DataVertex>
    {
        public DataEdge(DataVertex source, DataVertex target, double weight = 1)
            : base(source, target, weight)
        {
        }

        public DataEdge()
            : base(null, null, 1)
        {
        }

        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}
