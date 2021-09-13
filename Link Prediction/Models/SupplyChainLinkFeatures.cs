using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link_Prediction.Models
{
    public class SupplyChainLinkFeatures
    {
        [LoadColumn(0)]
        public int outsourcingAssociation { get; set; }
        [LoadColumn(1)]
        public int buyerAssociation { get; set; }
        [LoadColumn(2)]
        public int competitionAssociation { get; set; }
        [LoadColumn(3)]
        public int degree { get; set; }
        [LoadColumn(4)]
        [ColumnName("Label")]
        public bool exists { get; set; }
    }
}
