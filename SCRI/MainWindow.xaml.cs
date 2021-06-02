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
using Microsoft.Msagl.Drawing;
using Neo4j.Driver;
using SCRI.Database;
using Shape = Microsoft.Msagl.Drawing.Shape;

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

            Graph graph = new Graph();
            driver = _driverFactory.CreateDriver();
            var session = driver.Session();
            var edges = session.ReadTransaction(tx => GetAllEdges(tx));
            foreach (var edge in edges)
            {
                graph.AddEdge(edge.Key.Id.ToString(), edge.Value.Id.ToString());
            }
            //var list = nodes.ConvertAll(
            //    new Converter<INode, string>(n => n.Properties.FirstOrDefault().Value.ToString())
            //    );

            //graph.AddEdge("A", "B");
            //graph.AddEdge("B", "C");
            //graph.AddEdge("A", "C").Attr.Color = Microsoft.Msagl.Drawing.Color.Green;
            //graph.FindNode("A").Attr.FillColor = Microsoft.Msagl.Drawing.Color.Magenta;
            //graph.FindNode("B").Attr.FillColor = Microsoft.Msagl.Drawing.Color.MistyRose;
            //Microsoft.Msagl.Drawing.Node c = graph.FindNode("C");
            //c.Attr.FillColor = Microsoft.Msagl.Drawing.Color.PaleGreen;
            //c.Attr.Shape = Microsoft.Msagl.Drawing.Shape.Diamond;

            graph.Attr.LayerDirection = LayerDirection.LR;
            graphControl.Graph = graph;
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
            var res = tx.Run("MATCH (a)-[]->(b) Return a,b");
            var list = res.ToList();
            return list.ConvertAll(
                new Converter<IRecord, INode>(x => x.Values.Values.First().As<INode>())
                );
        }
    }
}
