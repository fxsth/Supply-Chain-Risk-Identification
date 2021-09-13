using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link_Prediction.Models
{
    public class PredictedSupplyChainLink
    {
        [ColumnName("Score")]
        public bool PredictedLinkExistence { get; set; }
    }
}
