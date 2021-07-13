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
using Neo4j.Driver;
using SCRI.Database;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Miscellaneous;
using Microsoft.Msagl.Core.Geometry.Curves;
using SCRI.Utils;
using SCRI.Models;

namespace SCRI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DriverFactory _driverFactory;
        private IDriver driver;
        private GraphViewer _graphViewer;
        private GraphViewerSettings graphViewerSettings = new GraphViewerSettings();
        private SupplyNetwork graphData;
        private Dictionary<int, Dictionary<string, string>> NodePropertiesAndValues;

        private EdgeRoutingSettings selectedEdgeRoutingSettings;
        public LayoutAlgorithm selectedLayoutAlgorithm;



        public MainWindow(IDriverFactory driverFactory)
        {
            InitializeComponent();
            _driverFactory = driverFactory as DriverFactory;
            Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _graphViewer = new GraphViewer();
            _graphViewer.RunLayoutAsync = true;
            _graphViewer.BindToPanel(ViewGraphPanel);
            selectedEdgeRoutingSettings = new EdgeRoutingSettings();

            _graphViewer.LayoutStarted += OnLayoutStarted;
            _graphViewer.LayoutComplete += OnLayoutComplete;
            _graphViewer.ObjectUnderMouseCursorChanged += OnObjectUnderMouseCursorChanged;

            InitialGraphVisualization();
            //ViewGraphPanel.LayoutUpdated += updateGraph;
        }

        private void InitialGraphVisualization()
        {
            driver = _driverFactory.CreateDriver();
            Graph initialGraph = new Graph();
            using (var session = driver.Session())
            {
                GraphDatabaseCombobox.Text = session.ReadTransaction(tx => CypherTransactions.GetDefaultDatabase(tx));
                // Get graph from db
                graphData = session.ReadTransaction(tx => CypherTransactions.GetCompleteGraph(tx));
                // Check plugins
                var procedures = session.ReadTransaction(tx => CypherTransactions.GetAvailableProcedures(tx));
                bool apoc = Neo4jUtils.isAPOCEnabled(procedures);
                bool gds = Neo4jUtils.isGraphDataScienceLibraryEnabled(procedures);
                // Execute Centrality Algorithms and write results to graph properties
                session.WriteTransaction<object>(tx => CypherTransactions.WriteCentralityMeasuresToProperty(tx));
                // retrieve schema
                var dbSchema = session.ReadTransaction(tx => CypherTransactions.GetDatabaseSchema(tx));
                initialGraph = graphViewerSettings.GetGraph(graphData, dbSchema);
                NodePropertiesAndValues = Neo4jUtils.GetGraphPropertiesAndValues(graphData);
            }

            // default layout: MDS
            selectedLayoutAlgorithm = LayoutAlgorithm.MDS;
            LayoutAlgorithmComboBox.Text = selectedLayoutAlgorithm.ToString();
            var initialLayoutSettings = new Microsoft.Msagl.Layout.MDS.MdsLayoutSettings() { };
            selectedEdgeRoutingSettings.EdgeRoutingMode = Microsoft.Msagl.Core.Routing.EdgeRoutingMode.Spline;
            initialLayoutSettings.PackingAspectRatio = 7.8;
            initialLayoutSettings.PackingMethod = Microsoft.Msagl.Core.Layout.PackingMethod.Compact;
            initialGraph.LayoutAlgorithmSettings = initialLayoutSettings;

            initialGraph.Attr.LayerDirection = LayerDirection.LR;
            initialGraph.Attr.BackgroundColor = Microsoft.Msagl.Drawing.Color.Transparent;
            initialGraph.Attr.Color = Microsoft.Msagl.Drawing.Color.Transparent;

            _graphViewer.Graph = initialGraph;

            NodeSizeDependenceComboBox.ItemsSource = GeneralUtils.enumToStringList(typeof(GraphViewerSettings.NodeSizeDependsOn));
        }

        private void UpdateGraphLayout(Graph graph)
        {
            // recalculate graph layout
            LayoutHelpers.CalculateLayout(graph.GeometryGraph, graph.LayoutAlgorithmSettings, null);
            // and update visuals
            _graphViewer.Graph = graph;
        }

        private void OnLayoutComplete(object sender, EventArgs e)
        {
            CurrentStatusLabel.Content = "Graph Layout Complete";
        }
        private void OnLayoutStarted(object sender, EventArgs e)
        {
            CurrentStatusLabel.Content = "Calculating Graph Layout...";
        }

        private void OnObjectUnderMouseCursorChanged(object sender, EventArgs e)
        {
            var viewerObject = e.As<ObjectUnderMouseCursorChangedEventArgs>().NewObject ;
            if(viewerObject != null && viewerObject.DrawingObject is Microsoft.Msagl.Drawing.Node)
            {
                var id = Convert.ToInt32(viewerObject.DrawingObject.As<Microsoft.Msagl.Drawing.Node>().Id);
                NodePropertiesItemsControl.ItemsSource = NodePropertiesAndValues[id];
            }
        }

        private async void LayoutAlgorithmComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_graphViewer == null || _graphViewer.Graph == null || e.AddedItems.Count<1)
                return;
            Graph graph = _graphViewer.Graph;
            selectedLayoutAlgorithm = e.AddedItems[0].As<LayoutAlgorithm>();
            switch (selectedLayoutAlgorithm)
            {
                case LayoutAlgorithm.FastIncremental:
                    graph.LayoutAlgorithmSettings = new Microsoft.Msagl.Layout.Incremental.FastIncrementalLayoutSettings()
                    { 
                        EdgeRoutingSettings = selectedEdgeRoutingSettings
                    };
                    break;
                case LayoutAlgorithm.LargeGraph:
                    //_layoutAlgorithmSettings = new Microsoft.Msagl.Layout.LargeGraphLayout.LgLayoutSettings()  { EdgeRoutingSettings = _edgeRoutingSettings};
                    break;
                case LayoutAlgorithm.Sugiyama:
                    graph.LayoutAlgorithmSettings = new Microsoft.Msagl.Layout.Layered.SugiyamaLayoutSettings()
                    {
                        EdgeRoutingSettings = selectedEdgeRoutingSettings
                    };
                    break;
                case LayoutAlgorithm.MDS:
                    graph.LayoutAlgorithmSettings = new Microsoft.Msagl.Layout.MDS.MdsLayoutSettings()
                    {
                        EdgeRoutingSettings = selectedEdgeRoutingSettings
                    };
                    break;
                case LayoutAlgorithm.Ranking:
                    graph.LayoutAlgorithmSettings = new Microsoft.Msagl.Prototype.Ranking.RankingLayoutSettings()
                    {
                        EdgeRoutingSettings = selectedEdgeRoutingSettings
                    };
                    break;
                default:
                    break;
            }

            _graphViewer.Graph.LayoutAlgorithmSettings.PackingAspectRatio = 7.8;
            _graphViewer.Graph.LayoutAlgorithmSettings.PackingMethod = PackingMethod.Compact;
            UpdateGraphLayout(graph);
        }

        private void GraphDatabaseCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            var selectedDatabase = e.AddedItems[0].ToString();
            using (var session = driver.Session(o => o.WithDatabase(selectedDatabase)))
            {
                session.WriteTransaction<object>(tx => CypherTransactions.WriteCentralityMeasuresToProperty(tx));
                graphData = session.ReadTransaction(tx => CypherTransactions.GetCompleteGraph(tx));
                var dbSchema = session.ReadTransaction(tx => CypherTransactions.GetDatabaseSchema(tx));
                var graph = graphViewerSettings.GetGraph(graphData, dbSchema);
                graph.LayoutAlgorithmSettings = _graphViewer.Graph.LayoutAlgorithmSettings;
                graph.Attr = _graphViewer.Graph.Attr;
                _graphViewer.Graph = graph;
                NodePropertiesAndValues = Neo4jUtils.GetGraphPropertiesAndValues(graphData);
                NodePropertiesItemsControl.ItemsSource = NodePropertiesAndValues.First().Value;
            }
        }

        private void GraphDatabaseCombobox_DropDownOpened(object sender, EventArgs e)
        {
            using (var session = driver.Session())
            {
                var databases = session.ReadTransaction(tx => CypherTransactions.GetDatabases(tx));
                GraphDatabaseCombobox.Items.Clear();
                foreach (var database in databases)
                    GraphDatabaseCombobox.Items.Add(database);
            }
        }

        private void NodeSizeDependenceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            Enum.TryParse(e.AddedItems[0].ToString(), out GraphViewerSettings.NodeSizeDependsOn nodeSizeDependence);
            graphViewerSettings.selectedNodeSizeDependence = nodeSizeDependence;
            _graphViewer.Graph = _graphViewer.Graph;
        }
    }

    public enum LayoutAlgorithm
    {
        FastIncremental,
        LargeGraph,
        Sugiyama,
        MDS,
        Ranking
    }

}
