using Link_Prediction.Models;
using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MachineLearning
{
    public class LinkPredictor
    {
        private const string MODEL_FILENAME = "model.zip";
        private MLContext _mlContext = new MLContext();
        private IDataView _trainingDataView;
        private DataViewSchema _modelSchema;
        private ITransformer _model;
        private ExperimentResult<BinaryClassificationMetrics> _experimentResult;

        public void SetData(IEnumerable<SupplyChainLinkFeatures> inputData)
        {
            _trainingDataView = _mlContext.Data.LoadFromEnumerable<SupplyChainLinkFeatures>(inputData);
        }

        public void SetData(IDataView dataView)
        {
            _trainingDataView = dataView;
        }
        public void TrainModel(uint experimentTimeSeconds, IProgress<RunDetail<BinaryClassificationMetrics>> progressHandler)
        {
            _experimentResult = _mlContext.Auto()
                .CreateBinaryClassificationExperiment(experimentTimeSeconds)
                .Execute(_trainingDataView, progressHandler: progressHandler);
        }
        public ITransformer GetBestModel() => _experimentResult.BestRun.Model;

        public void SaveModel(ITransformer model)
        {
            _mlContext.Model.Save(model, _trainingDataView.Schema, MODEL_FILENAME);
        }

        public ITransformer LoadModel()
        {
            _model = _mlContext.Model.Load(MODEL_FILENAME, out _modelSchema);
            return _model;
        }

        public DataViewSchema GetModelSchema() => _modelSchema;

        public bool PredictLinkExistence(SupplyChainLinkFeatures features)
        {
            PredictionEngine<SupplyChainLinkFeatures, PredictedSupplyChainLink> predictionEngine = _mlContext.Model.CreatePredictionEngine<SupplyChainLinkFeatures, PredictedSupplyChainLink>(_model);
            PredictedSupplyChainLink predictedScore = predictionEngine.Predict(features);
            return predictedScore.PredictedLinkExistence;
        }
    }
}
