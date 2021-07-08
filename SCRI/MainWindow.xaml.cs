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
        private LayoutAlgorithmSettings _layoutAlgorithmSettings;
        private Graph _graph;
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
            _graphViewer.BindToPanel(ViewGraphPanel);
            _graph = new Graph();
            _edgeRoutingSettings = new EdgeRoutingSettings();

            LoadAndVisualizeGraph();
            //ViewGraphPanel.LayoutUpdated += updateGraph;
        }

        private void updateGraph(object sender, EventArgs e)
        {
            LoadAndVisualizeGraph();
        }

        private void LoadAndVisualizeGraph()
        {
            driver = _driverFactory.CreateDriver();
            var session = driver.Session();
            // Get graph from 
            var graph = session.ReadTransaction(tx => GraphDbConnection.GetCompleteGraph(tx));

            // default layout: MDS
            _layoutAlgorithmSettings = new Microsoft.Msagl.Layout.MDS.MdsLayoutSettings() { };
            _edgeRoutingSettings.EdgeRoutingMode = Microsoft.Msagl.Core.Routing.EdgeRoutingMode.StraightLine;
            _layoutAlgorithmSettings.PackingAspectRatio = 7.8;
            _layoutAlgorithmSettings.PackingMethod = Microsoft.Msagl.Core.Layout.PackingMethod.Compact;
            _graph.LayoutAlgorithmSettings = _layoutAlgorithmSettings;
            foreach (var edge in graph.Edges)
                _graph.AddEdge(edge.Source.ID.ToString(), edge.Target.ID.ToString());

            _graph.Attr.LayerDirection = LayerDirection.LR;
            _graph.Attr.BackgroundColor = Microsoft.Msagl.Drawing.Color.Transparent;
            _graph.Attr.Color = Microsoft.Msagl.Drawing.Color.Transparent;
            _graphViewer.Graph = _graph;
            updateMSAGLLayout();
        }

        private void updateMSAGLLayout()
        {
            LayoutHelpers.CalculateLayout(_graphViewer.Graph.GeometryGraph, _graphViewer.Graph.LayoutAlgorithmSettings, null);
        }

        private void LayoutAlgorithmComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _layoutAlgorithm = e.AddedItems[0].As<LayoutAlgorithm>();
            switch (_layoutAlgorithm)
            {
                case LayoutAlgorithm.FastIncremental:
                    _graphViewer.Graph.LayoutAlgorithmSettings = new Microsoft.Msagl.Layout.Incremental.FastIncrementalLayoutSettings() { EdgeRoutingSettings = _edgeRoutingSettings };
                    break;
                case LayoutAlgorithm.LargeGraph:
                    //_layoutAlgorithmSettings = new Microsoft.Msagl.Layout.LargeGraphLayout.LgLayoutSettings()  { EdgeRoutingSettings = _edgeRoutingSettings};
                    break;
                case LayoutAlgorithm.Sugiyama:
                    _graphViewer.Graph.LayoutAlgorithmSettings = new Microsoft.Msagl.Layout.Layered.SugiyamaLayoutSettings() { EdgeRoutingSettings = _edgeRoutingSettings };
                    break;
                case LayoutAlgorithm.MDS:
                    _graphViewer.Graph.LayoutAlgorithmSettings = new Microsoft.Msagl.Layout.MDS.MdsLayoutSettings() { EdgeRoutingSettings = _edgeRoutingSettings };
                    break;
                case LayoutAlgorithm.Ranking:
                    _graphViewer.Graph.LayoutAlgorithmSettings = new Microsoft.Msagl.Prototype.Ranking.RankingLayoutSettings() { EdgeRoutingSettings = _edgeRoutingSettings };
                    break;
                default:
                    break;
            }
            updateMSAGLLayout();
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
