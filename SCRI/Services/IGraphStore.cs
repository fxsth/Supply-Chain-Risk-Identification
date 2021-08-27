using QuikGraph;
using SCRI.Models;
using System.Collections.Generic;

namespace SCRI.Services
{
    public interface IGraphStore
    {
        string defaultGraph { get; set; }
        SupplyNetwork GetGraph(string graphName);
        void StoreGraph(string graphName, SupplyNetwork graph);
        DbSchema GetDbSchema(string graphName);
        void StoreSchema(string graphName, DbSchema schema);
        IEnumerable<string> availableGraphs { get;}
        void AnnounceAvailableGraphs(IEnumerable<string> graphNames);
    }
}