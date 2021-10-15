using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MachineLearning.Models;

namespace MachineLearning
{
    public class LinkPredictor
    {
        private const string FolderName = "Machine Learning Results";
        private const string ModelFilename = "classification-model.zip";
        private const string ResultsFilename = "training-results.txt";
        private string ModelFilePath;
        private string ResultsFilePath;
        private readonly MLContext _mlContext;
        private IDataView _trainingDataView;
        private DataViewSchema _modelSchema;
        private ITransformer _model;
        private ExperimentResult<BinaryClassificationMetrics> _experimentResult;

        public LinkPredictor()
        {
            _mlContext = new MLContext();
            Directory.CreateDirectory(FolderName);
            ModelFilePath = Path.Combine(FolderName, ModelFilename);
            ResultsFilePath = Path.Combine(FolderName, ResultsFilename);
        }

        public void SetData(IEnumerable<SupplyChainLinkFeatures> inputData)
        {
            _trainingDataView = _mlContext.Data.LoadFromEnumerable(inputData);
        }

        public void SetData(IDataView dataView)
        {
            _trainingDataView = dataView;
        }

        public void TrainModel(uint experimentTimeSeconds,
            IProgress<RunDetail<BinaryClassificationMetrics>> progressHandler)
        {
            _experimentResult = _mlContext.Auto()
                .CreateBinaryClassificationExperiment(experimentTimeSeconds)
                .Execute(_trainingDataView, progressHandler: progressHandler);
        }

        public ITransformer GetBestModel() => _experimentResult.BestRun.Model;

        public async Task SaveTrainingResults()
        {
            List<string> lines = new List<string>()
            {
                "--------- Training Results ----------",
                $"Data set with {_trainingDataView.GetRowCount()} entries"
            };
            
            foreach (var runDetail in _experimentResult.RunDetails)
            {
                lines.Add("----------------------------------------------");
                lines.Add($"Model trained with: {runDetail.TrainerName}");
                lines.Add($"Training runtime in seconds: {runDetail.RuntimeInSeconds}");
                lines.Add($"Accuracy: {runDetail.ValidationMetrics.Accuracy}");
                lines.Add($"F1Score: {runDetail.ValidationMetrics.F1Score}");
                lines.Add($"AreaUnderRocCurve: {runDetail.ValidationMetrics.AreaUnderRocCurve}");
                lines.Add($"PositiveRecall: {runDetail.ValidationMetrics.PositiveRecall}");
                lines.Add($"NegativeRecall: {runDetail.ValidationMetrics.NegativeRecall}");
                lines.Add($"PositivePrecision: {runDetail.ValidationMetrics.PositivePrecision}");
                lines.Add($"NegativePrecision: {runDetail.ValidationMetrics.NegativePrecision}");
                lines.Add(runDetail.ValidationMetrics.ConfusionMatrix.GetFormattedConfusionTable());
            }

            lines.Add("----------------------------------------------");
            lines.Add("----------------------------------------------");
            lines.Add($"Best Run is: {_experimentResult.BestRun.TrainerName}");

            await File.WriteAllLinesAsync(ResultsFilePath, lines);
        }
        public void SaveModel(ITransformer model)
        {
            _mlContext.Model.Save(model, _trainingDataView.Schema, ModelFilePath);
        }

        public ITransformer LoadModel()
        {
            _model = _mlContext.Model.Load(ModelFilePath, out _modelSchema);
            return _model;
        }

        public DataViewSchema GetModelSchema() => _modelSchema;

        public bool PredictLinkExistence(SupplyChainLinkFeatures features)
        {
            PredictionEngine<SupplyChainLinkFeatures, PredictedSupplyChainLink> predictionEngine =
                _mlContext.Model.CreatePredictionEngine<SupplyChainLinkFeatures, PredictedSupplyChainLink>(_model);
            PredictedSupplyChainLink predictedScore = predictionEngine.Predict(features);
            return predictedScore.PredictedLinkExistence;
        }

        public Dictionary<(int, int), bool> PredictLinkExistence(
            Dictionary<(int, int), SupplyChainLinkFeatures> featuresList)
        {
            PredictionEngine<SupplyChainLinkFeatures, PredictedSupplyChainLink> predictionEngine =
                _mlContext.Model.CreatePredictionEngine<SupplyChainLinkFeatures, PredictedSupplyChainLink>(_model);
            Dictionary<(int, int), bool> predictedExistingLinks = new Dictionary<(int, int), bool>();
            foreach (var features in featuresList)
            {
                PredictedSupplyChainLink predictedScore = predictionEngine.Predict(features.Value);
                predictedExistingLinks[features.Key] = predictedScore.PredictedLinkExistence;
            }

            return predictedExistingLinks;
        }
    }
}