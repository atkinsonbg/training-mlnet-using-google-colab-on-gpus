using App.Ml;
using CommandLine;

namespace App
{
    class Program
    {
        public class Options
        {
            [Option('t', "train", Required = false, HelpText = "Train flag to start a training session.")]
            public bool Train { get; set; }

            [Option('f', "folder", Required = false, HelpText = "Folderpath to train on.")]
            public string? Folder { get; set; }

            [Option('f', "file", Required = false, HelpText = "Training file to train on.")]
            public string? TrainingFile { get; set; }
        }

        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       if (o.Train)
                       {
                           new Trainer().Train(o.Folder!, o.TrainingFile!);
                       }
                       else
                       {
                           Console.WriteLine("No valid argument was passed in.");
                       }
                   });
        }
    }
}