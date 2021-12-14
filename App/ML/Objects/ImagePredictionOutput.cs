using System;
using Microsoft.ML.Data;

namespace App.ML.Objects
{
    class ImagePredictionOutput
    {
        public string? ImagePath { get; set; }

        public string? Label { get; set; }

        public string? PredictedLabel { get; set; }

        public VBuffer<Single> Score { get; set; }
    }
}