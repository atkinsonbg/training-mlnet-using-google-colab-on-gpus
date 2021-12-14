namespace App.ML.Objects
{
    class ImagePredictionInput
    {
        public byte[]? Image { get; set; }

        public UInt32 LabelAsKey { get; set; }

        public string? ImagePath { get; set; }

        public string? Label { get; set; }
    }
}