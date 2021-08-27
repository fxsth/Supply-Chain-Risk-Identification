using QuikGraph;
using SCRI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCRI.Services
{
    /// <summary>
    /// Stores the graphs from the database
    /// </summary>
    public class GraphStore : IGraphStore
    {
        private Dictionary<string, SupplyNetwork> _graphDictionary = new Dictionary<string, SupplyNetwork>();
        private Dictionary<string, DbSchema> _schemaDictionary = new Dictionary<string, DbSchema>();

        public GraphStore()
        {
        }

        public string defaultGraph { get; set; }
        public IEnumerable<string> availableGraphs => _graphDictionary.Select(x=>x.Key);

        public void AnnounceAvailableGraphs(IEnumerable<string> graphNames)
        {
            foreach(var graph in graphNames)
            {
                _graphDictionary[graph] = null;
            }
        }

        public SupplyNetwork GetGraph(string graphName)
        {
            if (_graphDictionary.ContainsKey(graphName))
                return _graphDictionary[graphName];
            else
                return null;
        }

        public void StoreGraph(string graphName, SupplyNetwork graph)
        {
            _graphDictionary[graphName] = graph;
        }

        public DbSchema GetDbSchema(string graphName)
        {
            if (_schemaDictionary.ContainsKey(graphName))
                return _schemaDictionary[graphName];
            else
                return null;
        }

        public void StoreSchema(string graphName, DbSchema schema)
        {
            _schemaDictionary[graphName] = schema;
        }
    }
}
