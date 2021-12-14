using System.Globalization;
using App.ML.Base;
using App.ML.Objects;
using CsvHelper;
using Microsoft.ML;
using Microsoft.ML.Vision;
using static Microsoft.ML.DataOperationsCatalog;

namespace App.Ml
{
    public class Trainer : BaseML
    {
        public void Train(string folderPath, string trainingFilename)
        {
            IEnumerable<ImageData> images = LoadImagesFromCsv($"{folderPath}/{trainingFilename}");

            IDataView imageData = mlContext.Data.LoadFromEnumerable(images);
            IDataView shuffledData = mlContext.Data.ShuffleRows(imageData, shuffleSource: true);

            var previewshuffledData = shuffledData.Preview();

            var preprocessingPipeline = mlContext.Transforms.Conversion
                                                .MapValueToKey(
                                                    inputColumnName: "Label",
                                                    outputColumnName: "LabelAsKey")
                                                .Append(mlContext.Transforms.LoadRawImageBytes(
                                                    outputColumnName: "Image",
                                                    imageFolder: $"{folderPath}",
                                                    inputColumnName: "ImagePath"));

            IDataView preProcessedData = preprocessingPipeline
                    .Fit(shuffledData)
                    .Transform(shuffledData);

            TrainTestData trainSplit = mlContext.Data.TrainTestSplit(data: preProcessedData, testFraction: 0.2);
            TrainTestData validationTestSplit = mlContext.Data.TrainTestSplit(trainSplit.TestSet);

            IDataView trainSet = trainSplit.TrainSet;
            IDataView validationSet = validationTestSplit.TrainSet;
            IDataView testSet = validationTestSplit.TestSet;

            var classifierOptions = new ImageClassificationTrainer.Options()
            {
                FeatureColumnName = "Image",
                LabelColumnName = "LabelAsKey",
                ValidationSet = validationSet,
                MetricsCallback = (metrics) => Console.WriteLine(metrics),
                Arch = ImageClassificationTrainer.Architecture.ResnetV250,
                TestOnTrainSet = true,
                ReuseTrainSetBottleneckCachedValues = true,
                ReuseValidationSetBottleneckCachedValues = true,
                WorkspacePath = "."
            };

            var trainingPipeline = mlContext.MulticlassClassification.Trainers.ImageClassification(classifierOptions)
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            ITransformer trainedModel = trainingPipeline.Fit(trainSet);

            mlContext.Model.Save(trainedModel, trainSet.Schema, ModelPath);

            PredictionEngine<ImagePredictionInput, ImagePredictionOutput> predictionEngine = mlContext.Model.CreatePredictionEngine<ImagePredictionInput, ImagePredictionOutput>(trainedModel);
            IEnumerable<ImagePredictionInput> testImages = mlContext.Data.CreateEnumerable<ImagePredictionInput>(testSet, reuseRowObject: true).Take(20);

            foreach (var image in testImages)
            {
                ImagePredictionOutput prediction = predictionEngine.Predict(image);
                string imageName = Path.GetFileName(prediction.ImagePath!);
                Console.WriteLine($"Image: {imageName} | Actual Value: {prediction.Label} | Predicted Value: {prediction.PredictedLabel} | Score: {prediction.Score.GetValues().ToArray().Max()}");
            }
        }

        public static IEnumerable<ImageData> LoadImagesFromCsv(string csvPath)
        {
            var records = new List<ImageData>();
            using (var reader = new StreamReader(csvPath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    var record = new ImageData
                    {
                        ImagePath = csv.GetField("ImagePath"),
                        Label = csv.GetField("Label")
                    };
                    records.Add(record);
                }
            }

            return records.AsEnumerable();
        }
    }
}