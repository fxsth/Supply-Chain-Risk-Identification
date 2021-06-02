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
        //private GraphDbConnection connection;
        public MainWindow(/*IDisposable con*/)
        {
            InitializeComponent();
            //if(con is GraphDbConnection)
            //    connection = con as GraphDbConnection;
            Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            Graph graph = new Graph();
            ////graph.AddEdge("Box", "House");
            ////graph.AddEdge("House", "InvHouse");
            ////graph.AddEdge("InvHouse", "Diamond");
            ////graph.AddEdge("Diamond", "Octagon");
            //graph.AddEdge("Octagon", "Hexagon");
            ////graph.AddEdge("Hexagon", "2 Circle");
            ////graph.AddEdge("2 Circle", "Box");

            ////graph.FindNode("Box").Attr.Shape = Shape.Box;
            ////graph.FindNode("House").Attr.Shape = Shape.House;
            ////graph.FindNode("InvHouse").Attr.Shape = Shape.InvHouse;
            ////graph.FindNode("Diamond").Attr.Shape = Shape.Diamond;
            //graph.FindNode("Octagon").Attr.Shape = Shape.Octagon;
            //graph.FindNode("Hexagon").Attr.Shape = Shape.Hexagon;
            ////graph.FindNode("2 Circle").Attr.Shape = Shape.DoubleCircle;
            ///
            //var session = connection.GetSession();
            //var nodes = session.ReadTransaction(tx => GetAllNodes(tx));
            //var list = nodes.ConvertAll(
            //    new Converter<INode, string>(n => n.Properties.FirstOrDefault().Value.ToString())
            //    );

            graph.AddEdge("A", "B");
            graph.AddEdge("B", "C");
            graph.AddEdge("A", "C").Attr.Color = Microsoft.Msagl.Drawing.Color.Green;
            graph.FindNode("A").Attr.FillColor = Microsoft.Msagl.Drawing.Color.Magenta;
            graph.FindNode("B").Attr.FillColor = Microsoft.Msagl.Drawing.Color.MistyRose;
            Microsoft.Msagl.Drawing.Node c = graph.FindNode("C");
            c.Attr.FillColor = Microsoft.Msagl.Drawing.Color.PaleGreen;
            c.Attr.Shape = Microsoft.Msagl.Drawing.Shape.Diamond;

            graph.Attr.LayerDirection = LayerDirection.LR;

            graphControl.Graph = graph;
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
}
