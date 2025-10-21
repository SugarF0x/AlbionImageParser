using OpenCvSharp;
using Tesseract;

namespace AlbionImageParser;

[TestClass]
public sealed class Test1
{

    [TestMethod]
    public void Region()
    {
        var outputPath = Path.GetFullPath("../../../../.output");
        
        
        var assetsPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
        var iconPath = Path.GetFullPath(Path.Combine(assetsPath, @"icon.png"));
        var imagesPath = Path.GetFullPath(Path.Combine(assetsPath, @"samples"));
        var images = Directory.GetFiles(imagesPath);
        
        // foreach (var image in images)
        // {
        //     var newFileName = Path.Combine(outputPath, Path.GetFileName(image));
        //     Console.WriteLine(newFileName);
        // }

        foreach (var image in images)
        {
            var newPath = Path.Combine(outputPath, Path.GetFileName(image));
            Console.WriteLine(newPath);
            File.Delete(newPath);
            RegionDetection.SiftMatches(Cv2.ImRead(image, ImreadModes.Grayscale),
                Cv2.ImRead(iconPath, ImreadModes.Grayscale), newPath);
        }
        
        // RegionDetection.SiftMany(Cv2.ImRead(images[0], ImreadModes.Grayscale), Cv2.ImRead(iconPath, ImreadModes.Grayscale));
    }
    
    [TestMethod]
    public void TestMethod1()
    {
        var assetsPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
        var tessdataPath = Path.GetFullPath(Path.Combine(assetsPath, "tessdata"));
        var iconPath = Path.GetFullPath(Path.Combine(assetsPath, @"icon.png"));
        var imagePath = Path.GetFullPath(Path.Combine(assetsPath, @"samples\1.png"));
        
        var parser = new ScreenshotParser(tessdataPath, iconPath);
        var result = parser.Parse(imagePath);

        if (result != null)
            Console.WriteLine(result);
        else
            Console.WriteLine("No portal box detected.");
    }

    [TestMethod]
    public void T2()
    {
        var assetsPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
        var tessdataPath = Path.GetFullPath(Path.Combine(assetsPath, "tessdata"));

        using (var engine = new TesseractEngine(tessdataPath, "eng", EngineMode.Default))
        {
            
        }
    }
}