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
        private LayoutAlgorithm _layoutAlgorithm;
        private EdgeRoutingSettings _edgeRoutingSettings;
        private GraphViewer _graphViewer;



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
            _edgeRoutingSettings = new EdgeRoutingSettings();

            _graphViewer.LayoutStarted += OnLayoutStarted;
            _graphViewer.LayoutComplete += OnLayoutComplete;

            InitialGraphVisualization();
            //ViewGraphPanel.LayoutUpdated += updateGraph;
        }

        private void InitialGraphVisualization()
        {
            driver = _driverFactory.CreateDriver();
            var graphData = new Models.SupplyNetwork();
            var dbSchema = new DbSchema();
            var graphViewerSettings = new GraphViewerSettings();
            using (var session = driver.Session())
            {
                GraphDatabaseCombobox.SelectedItem = session.SessionConfig.Database;
                // Get graph from db
                graphData = session.ReadTransaction(tx => CypherTransactions.GetCompleteGraph(tx));
                // Check plugins
                var procedures = session.ReadTransaction(tx => CypherTransactions.GetAvailableProcedures(tx));
                bool apoc = Neo4jUtils.isAPOCEnabled(procedures);
                bool gds = Neo4jUtils.isGraphDataScienceLibraryEnabled(procedures);
                // retrieve schema
                dbSchema = session.ReadTransaction(tx => CypherTransactions.GetDatabaseSchema(tx));
                graphViewerSettings.AssignColorsToLabels(dbSchema.getUniqueNodeLabels());
            }

            // default layout: MDS
            _layoutAlgorithm = LayoutAlgorithm.MDS;
            var initialLayoutSettings = new Microsoft.Msagl.Layout.MDS.MdsLayoutSettings() { };
            var initialGraph = new Graph();
            _edgeRoutingSettings.EdgeRoutingMode = Microsoft.Msagl.Core.Routing.EdgeRoutingMode.StraightLine;
            initialLayoutSettings.PackingAspectRatio = 7.8;
            initialLayoutSettings.PackingMethod = Microsoft.Msagl.Core.Layout.PackingMethod.Compact;
            initialGraph.LayoutAlgorithmSettings = initialLayoutSettings;
            foreach (var edge in graphData.Edges)
            {
                var n1 = initialGraph.AddNode(edge.Source.ID.ToString());
                n1.Attr.FillColor = graphViewerSettings.GetLabelColor(edge.Source.Label.First());
                var n2 = initialGraph.AddNode(edge.Target.ID.ToString());
                n2.Attr.FillColor = graphViewerSettings.GetLabelColor(edge.Target.Label.First());
                var e = initialGraph.AddEdge(n1.Id, n2.Id);
            }

            initialGraph.Attr.LayerDirection = LayerDirection.LR;
            initialGraph.Attr.BackgroundColor = Microsoft.Msagl.Drawing.Color.Transparent;
            initialGraph.Attr.Color = Microsoft.Msagl.Drawing.Color.Transparent;

            _graphViewer.Graph = initialGraph;
        }

        private void UpdateGraphLayout()
        {
            // recalculate graph layout
            LayoutHelpers.CalculateLayout(_graphViewer.Graph.GeometryGraph, _graphViewer.Graph.LayoutAlgorithmSettings, null);
            // and update visuals
            _graphViewer.Graph = _graphViewer.Graph;
        }

        private void OnLayoutComplete(object sender, EventArgs e)
        {
            CurrentStatusLabel.Content = "Graph Layout Complete";
        }
        private void OnLayoutStarted(object sender, EventArgs e)
        {
            CurrentStatusLabel.Content = "Calculating Graph Layout...";
        }

        private async void LayoutAlgorithmComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _layoutAlgorithm = e.AddedItems[0].As<LayoutAlgorithm>();
            switch (_layoutAlgorithm)
            {
                case LayoutAlgorithm.FastIncremental:
                    _graphViewer.Graph.LayoutAlgorithmSettings = new Microsoft.Msagl.Layout.Incremental.FastIncrementalLayoutSettings()
                    { 
                        EdgeRoutingSettings = _edgeRoutingSettings
                    };
                    break;
                case LayoutAlgorithm.LargeGraph:
                    //_layoutAlgorithmSettings = new Microsoft.Msagl.Layout.LargeGraphLayout.LgLayoutSettings()  { EdgeRoutingSettings = _edgeRoutingSettings};
                    break;
                case LayoutAlgorithm.Sugiyama:
                    _graphViewer.Graph.LayoutAlgorithmSettings = new Microsoft.Msagl.Layout.Layered.SugiyamaLayoutSettings()
                    {
                        EdgeRoutingSettings = _edgeRoutingSettings
                    };
                    break;
                case LayoutAlgorithm.MDS:
                    _graphViewer.Graph.LayoutAlgorithmSettings = new Microsoft.Msagl.Layout.MDS.MdsLayoutSettings()
                    {
                        EdgeRoutingSettings = _edgeRoutingSettings
                    };
                    break;
                case LayoutAlgorithm.Ranking:
                    _graphViewer.Graph.LayoutAlgorithmSettings = new Microsoft.Msagl.Prototype.Ranking.RankingLayoutSettings()
                    {
                        EdgeRoutingSettings = _edgeRoutingSettings
                    };
                    break;
                default:
                    break;
            }

            _graphViewer.Graph.LayoutAlgorithmSettings.PackingAspectRatio = 7.8;
            _graphViewer.Graph.LayoutAlgorithmSettings.PackingMethod = PackingMethod.Compact;
            UpdateGraphLayout();
        }

        private void GraphDatabaseCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            var selectedDatabase = e.AddedItems[0].ToString();
            using (var session = driver.Session(o => o.WithDatabase(selectedDatabase)))
            {
                var graphData = session.ReadTransaction(tx => CypherTransactions.GetCompleteGraph(tx));
                var graph = new Graph() { LayoutAlgorithmSettings = _graphViewer.Graph.LayoutAlgorithmSettings, Attr = _graphViewer.Graph.Attr };
                foreach (var edge in graphData.Edges)
                    graph.AddEdge(edge.Source.ID.ToString(), edge.Target.ID.ToString());
                _graphViewer.Graph = graph;
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
