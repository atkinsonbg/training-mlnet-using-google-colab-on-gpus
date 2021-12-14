using App.Common;
using Microsoft.ML;

namespace App.ML.Base
{
    public class BaseML
    {
        protected static string ModelPath =>
            Path.Combine(AppContext.BaseDirectory, Constants.MODEL_FILENAME);

        protected readonly MLContext mlContext;

        protected BaseML()
        {
            int randomSeed = DateTime.Now.Millisecond;
            Console.WriteLine($"Using the following random seed: {randomSeed}");
            mlContext = new MLContext(randomSeed);
        }
    }
}