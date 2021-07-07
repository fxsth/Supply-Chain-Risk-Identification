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

        public Dictionary<LayoutAlgorithmTypeEnum, string> LayoutAlgorithmEnumsWithCaptions { get; } =
        new Dictionary<LayoutAlgorithmTypeEnum, string>()
        {
            {LayoutAlgorithmTypeEnum.BoundedFR, "BoundedFR"},
            {LayoutAlgorithmTypeEnum.Circular, "Circular"},
            {LayoutAlgorithmTypeEnum.CompoundFDP, "CompoundFDP"},
            {LayoutAlgorithmTypeEnum.KK, "KK"},
            //{ExampleEnum.None, "Hidden in UI"},
        };

        private LayoutAlgorithmTypeEnum selectedLayoutAlgorithm = LayoutAlgorithmTypeEnum.KK;
        public LayoutAlgorithmTypeEnum LayoutEnumProperty
        {
            get { return selectedLayoutAlgorithm; }
            set { gg_Area.LogicCore.DefaultLayoutAlgorithm = value; gg_Area.RelayoutGraph(); }
        }

        public MainWindow(IDriverFactory driverFactory)
        {
            InitializeComponent();
            _driverFactory = driverFactory as DriverFactory;
            Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            driver = _driverFactory.CreateDriver();
            var session = driver.Session();
            var edges = session.ReadTransaction(tx => GraphDbConnection.GetAllEdges(tx));
            var nodes = session.ReadTransaction(tx => GraphDbConnection.GetAllNodes(tx));

            //Create data graph object
            var graph = session.ReadTransaction(tx => GraphDbConnection.GetCompleteGraph(tx));

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
            ((OverlapRemovalParameters)LogicCore.DefaultOverlapRemovalAlgorithmParams).HorizontalGap = 300;
            ((OverlapRemovalParameters)LogicCore.DefaultOverlapRemovalAlgorithmParams).VerticalGap = 100;

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

            gg_Area.SetVerticesDrag(true);
            //gg_Area.ShowAllEdgesLabels(false);
        }

        private void LayoutAlgorithmComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            gg_Area.LogicCore.DefaultLayoutAlgorithm = e.AddedItems[0].As<LayoutAlgorithmTypeEnum>();
            gg_Area.RelayoutGraph();
        }
    }

    //Layout visual class
    public class GraphAreaExample : GraphArea<Models.Supplier, Models.SupplierRelationship, BidirectionalGraph<Models.Supplier, Models.SupplierRelationship>> { }

    //Logic core class
    public class GXLogicCoreExample : GXLogicCore<Models.Supplier, Models.SupplierRelationship, BidirectionalGraph<Models.Supplier, Models.SupplierRelationship>> { }
}
