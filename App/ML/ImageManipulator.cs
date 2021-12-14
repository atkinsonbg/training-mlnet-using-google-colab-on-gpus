using System.Drawing.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace App.Ml
{
    public class ImageManipulator
    {
        public void ResizeImage(string imageFolder)
        {
            // Get all the images to transform, order by name for logging purposes, ToList to avoid any possible lazy loading
            List<string> files = Directory.GetFiles(imageFolder).OrderBy(x => x).ToList();

            // Used for logging/display, totalFiles times 3 as we'll be doing 3 transforms
            var totalFiles = files.Count() * 3;
            var totalProcessed = 0;

            Console.WriteLine($"Starting conversion on {totalFiles} number of files...");

            // Parallelize the processing of each file for speed
            Parallel.ForEach(files, file =>
             {
                 try
                 {
                     // Get the FileInfo object to get to the name of the file
                     FileInfo fi = new FileInfo(file);
                     var filename = fi.Name;

                     // Convert the image to bytes to avoid any file locks while processsing the same image 3 times
                     byte[] imageArray = File.ReadAllBytes($"{imageFolder}/{filename}");

                     // Create a List of our objects we want to transform on
                     List<ImageDetails> images = new List<ImageDetails>();
                     images.Add(new ImageDetails { ImageBytes = imageArray, Filename = fi.Name, FilenameMutex = "_flip.jpg", Flip = FlipMode.Horizontal, Rotate = RotateMode.None });
                     images.Add(new ImageDetails { ImageBytes = imageArray, Filename = fi.Name, FilenameMutex = "_rotate_90.jpg", Flip = FlipMode.None, Rotate = RotateMode.Rotate90 });
                     images.Add(new ImageDetails { ImageBytes = imageArray, Filename = fi.Name, FilenameMutex = "_rotate_180.jpg", Flip = FlipMode.None, Rotate = RotateMode.Rotate180 });

                     // Parallelize the processing of the 3 transforms
                     Parallel.ForEach(images, image =>
                     {
                         var i = Image.Load<Rgba32>(image.ImageBytes);
                         i.Mutate(x => x.RotateFlip(image.Rotate, image.Flip));
                         i.Save($"{imageFolder}/{image.Filename!.Replace(".jpg", image.FilenameMutex)}");
                         totalProcessed++;
                         Console.WriteLine($"Finished {filename} - {totalProcessed} of {totalFiles} files...");
                     });
                 }
                 catch (Exception ex)
                 {
                     Console.WriteLine($"EXCEPTION: {ex.Message} : {ex.StackTrace}");
                 }
             });

            Console.WriteLine($"Image conversion was successful: {totalFiles == totalProcessed}");
            Console.WriteLine($"Number of files: {totalFiles}, Number of conversions: {totalProcessed}");
        }
    }

    public class ImageDetails
    {
        public byte[]? ImageBytes { get; set; }
        public string? Filename { get; set; }
        public string? FilenameMutex { get; set; }
        public FlipMode Flip { get; set; }
        public RotateMode Rotate { get; set; }
    }
}