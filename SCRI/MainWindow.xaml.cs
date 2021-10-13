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
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;

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
        public LayoutAlgorithm SelectedLayoutAlgorithm;
        public string SelectedNodeLabelFilter;

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
            //ViewGraphPanel.LayoutUpdated += updateGraph;
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
            SelectedLayoutAlgorithm = LayoutAlgorithm.MDS;
            LayoutAlgorithmComboBox.Text = SelectedLayoutAlgorithm.ToString();
            var initialLayoutSettings = new Microsoft.Msagl.Layout.MDS.MdsLayoutSettings() { };
            _selectedEdgeRoutingSettings.EdgeRoutingMode = Microsoft.Msagl.Core.Routing.EdgeRoutingMode.Spline;
            initialLayoutSettings.PackingAspectRatio = 7.8;
            initialLayoutSettings.PackingMethod = Microsoft.Msagl.Core.Layout.PackingMethod.Compact;
            initialGraph.LayoutAlgorithmSettings = initialLayoutSettings;

            initialGraph.Attr.LayerDirection = LayerDirection.LR;
            initialGraph.Attr.BackgroundColor = Microsoft.Msagl.Drawing.Color.Transparent;
            initialGraph.Attr.Color = Microsoft.Msagl.Drawing.Color.Transparent;

            _graphViewer.Graph = initialGraph;

            NodeSizeDependenceComboBox.ItemsSource =
                GeneralUtils.enumToStringList(typeof(GraphViewerSettings.NodeSizeDependsOn));
            NodeSizeDependenceComboBox.Text = _graphViewerSettings.selectedNodeSizeDependence.ToString();

            var listLabels = _graphService.GetLabelsInGraphSchema(_selectedDatabase).ToList();
            SelectedNodeLabelFilter = "All Labels";
            listLabels.Add("All Labels");
            FilterNodeLabelsComboBox.ItemsSource = listLabels;
            NodeSizeDependenceComboBox.Text = SelectedNodeLabelFilter;
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
            SelectedLayoutAlgorithm = e.AddedItems[0].As<LayoutAlgorithm>();
            switch (SelectedLayoutAlgorithm)
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
                SelectedNodeLabelFilter = "All Labels";
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
                // ignored
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
            Enum.TryParse(e.AddedItems[0].ToString(), out GraphViewerSettings.NodeSizeDependsOn nodeSizeDependence);
            _graphViewerSettings.selectedNodeSizeDependence = nodeSizeDependence;
            var graph = _graphViewerSettings.GetMsaglGraph(_selectedDatabase);
            graph.LayoutAlgorithmSettings = _graphViewer.Graph.LayoutAlgorithmSettings;
            graph.Attr = _graphViewer.Graph.Attr;
            _graphViewer.Graph = graph;
        }

        private async void FilterNodeLabelsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0 || e.AddedItems[0].ToString() == SelectedNodeLabelFilter)
                return;
            SelectedNodeLabelFilter = e.AddedItems[0].ToString();
            if (SelectedNodeLabelFilter == "All Labels")
            {
                await _graphService.RetrieveGraphFromDatabase(_selectedDatabase);
            }
            else
            {
                await _graphService.RetrieveGraphFromDatabase(_selectedDatabase,
                    new List<string> {SelectedNodeLabelFilter});
            }
            _nodePropertiesAndValues = _graphService.GetGraphPropertiesAndValues(_selectedDatabase);
            var graph = _graphViewerSettings.GetMsaglGraph(_selectedDatabase);
            graph.LayoutAlgorithmSettings = _graphViewer.Graph.LayoutAlgorithmSettings;
            graph.Attr = _graphViewer.Graph.Attr;
            _graphViewer.Graph = graph;
        }

        private async void LinkPredictionTrainButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentStatusLabel.Content = $"Calculating Features...";
            await _graphService.CalculateLinkFeatures(_selectedDatabase);
            Dictionary<(int, int), SupplyChainLinkFeatures> featuresList =
                _graphService.GetLinkFeatures(_selectedDatabase);

            LinkPredictor linkPredictor = new LinkPredictor();
            linkPredictor.SetData(featuresList.Values);
            var modelSchema = linkPredictor.GetModelSchema();
            string buttonText = LinkPredictionTrainButton.Content.ToString();
            CurrentStatusLabel.Content = $"Training Data Model...";
            linkPredictor.TrainModel(60, new Progress<RunDetail<BinaryClassificationMetrics>>());
            linkPredictor.SaveModel(linkPredictor.GetBestModel());
            LinkPredictionTrainButton.Content = buttonText;
            CurrentStatusLabel.Content = $"Data Model trained and saved";
        }

        private async void LinkPredictionPredictButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_graphService.ExistLinkFeatures(_selectedDatabase))
            {
                CurrentStatusLabel.Content = $"Calculating Link Features first...";
                await _graphService.CalculateLinkFeatures(_selectedDatabase);
            }

            var featuresList = _graphService.GetLinkFeatures(_selectedDatabase);

            CurrentStatusLabel.Content = $"Predicting Links";
            LinkPredictor linkPredictor = new LinkPredictor();
            linkPredictor.LoadModel();
            Dictionary<(int, int), bool> predictedExistingLinks = linkPredictor.PredictLinkExistence(featuresList);

            _graphViewer.Graph = _graphViewerSettings.AddPredictedLinksToGraph(_graphViewer.Graph,
                predictedExistingLinks,
                GraphViewerSettings.ShowPredictedLinks.OnlyAdditionalExisting);
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