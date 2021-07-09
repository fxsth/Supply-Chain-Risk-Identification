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
            var session = driver.Session();
            // Get graph from 
            var graphData= session.ReadTransaction(tx => CypherTransactions.GetCompleteGraph(tx));
            var procedures= session.ReadTransaction(tx => CypherTransactions.GetAvailableProcedures(tx));

            // default layout: MDS
            var initialLayoutSettings = new Microsoft.Msagl.Layout.MDS.MdsLayoutSettings() { };
            var initialGraph = new Graph();
            _edgeRoutingSettings.EdgeRoutingMode = Microsoft.Msagl.Core.Routing.EdgeRoutingMode.StraightLine;
            initialLayoutSettings.PackingAspectRatio = 7.8;
            initialLayoutSettings.PackingMethod = Microsoft.Msagl.Core.Layout.PackingMethod.Compact;
            initialGraph.LayoutAlgorithmSettings = initialLayoutSettings;
            foreach (var edge in graphData.Edges)
                initialGraph.AddEdge(edge.Source.ID.ToString(), edge.Target.ID.ToString());

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
