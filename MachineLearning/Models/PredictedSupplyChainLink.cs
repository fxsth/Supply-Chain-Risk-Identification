using Microsoft.ML.Data;

namespace MachineLearning.Models
{
    public class PredictedSupplyChainLink
    {
        [ColumnName("PredictedLabel")] public bool PredictedLinkExistence { get; set; }

        public float Probability { get; set; }

        public float Score { get; set; }
    }
}