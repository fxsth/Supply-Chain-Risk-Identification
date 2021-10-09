using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearning.Models
{
    public class SupplyChainLinkFeatures
    {
        [LoadColumn(0)]
        public Single OutsourcingAssociation { get; set; }
        [LoadColumn(1)]
        public Single BuyerAssociation { get; set; }
        [LoadColumn(2)]
        public Single CompetitionAssociation { get; set; }
        [LoadColumn(3)]
        public Single Degree { get; set; }
        [LoadColumn(4)]
        [ColumnName("Label")]
        public bool Exists { get; set; }
    }
}
