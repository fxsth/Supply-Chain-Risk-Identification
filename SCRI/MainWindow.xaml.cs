using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Neo4j.Driver;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Miscellaneous;
using SCRI.Utils;
using SCRI.Services;
using System.Threading.Tasks;
using MachineLearning;
using MachineLearning.Models;

namespace SCRI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GraphViewer _graphViewer;
        private readonly GraphViewerSettings _graphViewerSettings;
        private readonly IGraphService _graphService;
        private Dictionary<int, Dictionary<string, string>> _nodePropertiesAndValues;

        private string _selectedDatabase;
        private EdgeRoutingSettings _selectedEdgeRoutingSettings;
        private LayoutAlgorithm _selectedLayoutAlgorithm;
        private string _selectedNodeLabelFilter;

        public MainWindow(IGraphService graphService)
        {
            InitializeComponent();
            _graphService = graphService;
            _graphViewerSettings = _graphService.CreateGraphViewerSettings();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _graphViewer = new GraphViewer();
            _graphViewer.RunLayoutAsync = true;
            _graphViewer.BindToPanel(ViewGraphPanel);
            _selectedEdgeRoutingSettings = new EdgeRoutingSettings();

            _graphViewer.LayoutStarted += OnLayoutStarted;
            _graphViewer.LayoutComplete += OnLayoutComplete;
            _graphViewer.ObjectUnderMouseCursorChanged += OnObjectUnderMouseCursorChanged;

            await InitialGraphVisualization();
        }

        private async Task InitialGraphVisualization()
        {
            await _graphService.Init();
            Graph initialGraph = _graphViewerSettings.GetDefaultMsaglGraph();
            _nodePropertiesAndValues =
                _graphService.GetGraphPropertiesAndValues(_graphService.GetDefaultGraphName());
            GraphDatabaseCombobox.ItemsSource = _graphService.GetAvailableGraphs();
            _selectedDatabase = _graphService.GetDefaultGraphName();
            GraphDatabaseCombobox.Text = _selectedDatabase;

            // default layout: MDS
            _selectedLayoutAlgorithm = LayoutAlgorithm.MDS;
            LayoutAlgorithmComboBox.Text = _selectedLayoutAlgorithm.ToString();
            var initialLayoutSettings = new Microsoft.Msagl.Layout.MDS.MdsLayoutSettings() { };
            _selectedEdgeRoutingSettings.EdgeRoutingMode = Microsoft.Msagl.Core.Routing.EdgeRoutingMode.Spline;
            initialLayoutSettings.PackingAspectRatio = 7.8;
            initialLayoutSettings.PackingMethod = Microsoft.Msagl.Core.Layout.PackingMethod.Compact;
            initialGraph.LayoutAlgorithmSettings = initialLayoutSettings;

            initialGraph.Attr.LayerDirection = LayerDirection.LR;
            initialGraph.Attr.BackgroundColor = Microsoft.Msagl.Drawing.Color.Transparent;
            initialGraph.Attr.Color = Microsoft.Msagl.Drawing.Color.Transparent;

            _graphViewer.Graph = initialGraph;
            
            NodeSizeDependenceComboBox.Text = _graphViewerSettings.SelectedNodeSizeDependence.ToString();

            var listLabels = _graphService.GetLabelsInGraphSchema(_selectedDatabase).ToList();
            _selectedNodeLabelFilter = "All Labels";
            listLabels.Add("All Labels");
            FilterNodeLabelsComboBox.ItemsSource = listLabels;
            FilterNodeLabelsComboBox.Text = _selectedNodeLabelFilter;
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
            var viewerObject = e.As<ObjectUnderMouseCursorChangedEventArgs>().NewObject;
            if (viewerObject != null && viewerObject.DrawingObject is Microsoft.Msagl.Drawing.Node)
            {
                var id = Convert.ToInt32(viewerObject.DrawingObject.As<Microsoft.Msagl.Drawing.Node>().Id);
                NodePropertiesItemsControl.ItemsSource = _nodePropertiesAndValues[id];
            }
            else
            {
                NodePropertiesItemsControl.ItemsSource = null;
            }
        }

        private void LayoutAlgorithmComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_graphViewer == null || _graphViewer.Graph == null || e.AddedItems.Count < 1)
                return;
            Graph graph = _graphViewer.Graph;
            _selectedLayoutAlgorithm = e.AddedItems[0].As<LayoutAlgorithm>();
            switch (_selectedLayoutAlgorithm)
            {
                case LayoutAlgorithm.FastIncremental:
                    graph.LayoutAlgorithmSettings =
                        new Microsoft.Msagl.Layout.Incremental.FastIncrementalLayoutSettings()
                        {
                            EdgeRoutingSettings = _selectedEdgeRoutingSettings
                        };
                    break;
                case LayoutAlgorithm.Sugiyama:
                    graph.LayoutAlgorithmSettings = new Microsoft.Msagl.Layout.Layered.SugiyamaLayoutSettings()
                    {
                        EdgeRoutingSettings = _selectedEdgeRoutingSettings
                    };
                    break;
                case LayoutAlgorithm.MDS:
                    graph.LayoutAlgorithmSettings = new Microsoft.Msagl.Layout.MDS.MdsLayoutSettings()
                    {
                        EdgeRoutingSettings = _selectedEdgeRoutingSettings
                    };
                    break;
                case LayoutAlgorithm.Ranking:
                    graph.LayoutAlgorithmSettings = new Microsoft.Msagl.Prototype.Ranking.RankingLayoutSettings()
                    {
                        EdgeRoutingSettings = _selectedEdgeRoutingSettings
                    };
                    break;
                default:
                    break;
            }

            _graphViewer.Graph.LayoutAlgorithmSettings.PackingAspectRatio = 7.8;
            _graphViewer.Graph.LayoutAlgorithmSettings.PackingMethod = PackingMethod.Compact;
            UpdateGraphLayout(graph);
        }

        private async void GraphDatabaseCombobox_SelectionChangedAsync(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems.Count == 0 || _graphViewer is null || _graphViewer.Graph is null)
                    return;
                _selectedDatabase = e.AddedItems[0].ToString();
                await _graphService.RetrieveGraphFromDatabase(_selectedDatabase);
                var listLabels = _graphService.GetLabelsInGraphSchema(_selectedDatabase).ToList();
                _selectedNodeLabelFilter = "All Labels";
                listLabels.Add("All Labels");
                FilterNodeLabelsComboBox.ItemsSource = listLabels;
                var graph = _graphViewerSettings.GetMsaglGraph(_selectedDatabase);
                graph.LayoutAlgorithmSettings = _graphViewer.Graph.LayoutAlgorithmSettings;
                graph.Attr = _graphViewer.Graph.Attr;
                _graphViewer.Graph = graph;
                _nodePropertiesAndValues = _graphService.GetGraphPropertiesAndValues(_selectedDatabase);
                NodePropertiesItemsControl.ItemsSource = _nodePropertiesAndValues.First().Value;
            }
            catch (Exception ex)
            {
                CurrentStatusLabel.Content = ex.Message;
            }
        }

        private void GraphDatabaseCombobox_DropDownOpened(object sender, EventArgs e)
        {
            GraphDatabaseCombobox.ItemsSource = _graphService.GetAvailableGraphs();
        }

        private void NodeSizeDependenceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e?.AddedItems.Count < 1)
                return;
            Enum.TryParse(e.AddedItems[0].ToString(), out NodeSizeDependsOn nodeSizeDependence);
            _graphViewerSettings.SelectedNodeSizeDependence = nodeSizeDependence;
            var graph = _graphViewerSettings.GetMsaglGraph(_selectedDatabase);
            graph.LayoutAlgorithmSettings = _graphViewer.Graph.LayoutAlgorithmSettings;
            graph.Attr = _graphViewer.Graph.Attr;
            _graphViewer.Graph = graph;
        }

        private async void FilterNodeLabelsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0 || e.AddedItems[0].ToString() == _selectedNodeLabelFilter)
                return;
            _selectedNodeLabelFilter = e.AddedItems[0].ToString();
            if (_selectedNodeLabelFilter == "All Labels")
            {
                await _graphService.RetrieveGraphFromDatabase(_selectedDatabase);
            }
            else
            {
                await _graphService.RetrieveGraphFromDatabase(_selectedDatabase,
                    new List<string> {_selectedNodeLabelFilter});
            }
            _nodePropertiesAndValues = _graphService.GetGraphPropertiesAndValues(_selectedDatabase);
            var graph = _graphViewerSettings.GetMsaglGraph(_selectedDatabase);
            graph.LayoutAlgorithmSettings = _graphViewer.Graph.LayoutAlgorithmSettings;
            graph.Attr = _graphViewer.Graph.Attr;
            _graphViewer.Graph = graph;
        }

        private async void LinkPredictionTrainButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CurrentStatusLabel.Content = $"Calculating Features and training Data Model...";
                LinkPredictionTrainButton.IsEnabled = false;
                await _graphService.TrainLinkPredictionOnGivenGraph(_selectedDatabase);
                CurrentStatusLabel.Content = $"Data Model trained and saved";

            }
            catch (Exception ex)
            {
                CurrentStatusLabel.Content = ex.Message;
            }
            LinkPredictionTrainButton.IsEnabled = true;
        }

        private async void LinkPredictionPredictButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CurrentStatusLabel.Content = $"Predicting Links";
                LinkPredictor linkPredictor = new LinkPredictor();
                linkPredictor.LoadModel();
                Dictionary<(int, int), PredictedSupplyChainLink> predictedExistingLinks = await _graphService.ExecuteLinkPredictionOnGivenGraph(_selectedDatabase);
                _graphViewer.Graph = _graphViewerSettings.AddPredictedLinksToGraph(_graphViewer.Graph,
                    predictedExistingLinks,
                    GraphViewerSettings.ShowPredictedLinks.OnlyAdditionalExisting);
            }
            catch (Exception ex)
            {
                CurrentStatusLabel.Content = ex.Message;
            }
        }
    }

    public enum LayoutAlgorithm
    {
        FastIncremental,
        Sugiyama,
        MDS,
        Ranking
    }
}