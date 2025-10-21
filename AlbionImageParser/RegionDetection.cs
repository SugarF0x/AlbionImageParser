using System.Text.RegularExpressions;
using OpenCvSharp;
using Tesseract;
using Rect = OpenCvSharp.Rect;
using Size = OpenCvSharp.Size;

namespace AlbionImageParser;

public static partial class RegionDetection
{
    public struct SampleRegionData(string source, string target, string timeout);
    public static SampleRegionData Parse(string sampleSrc, string templateSrc)
    {
        var (sample, template) = PrepareSample(sampleSrc, templateSrc);
        using (sample)
        using (template)
        {
            var (source, target, timeout) = TemplateMatch(sample, template);
            using (source)
            using (target)
            using (timeout)
            {
                if (IsTextRed(timeout)) throw new InvalidImage("Portal timeout is under 1 hour");
                
                return new SampleRegionData(
                    ExtractText(source),
                    ExtractText(target),
                    ExtractTimeout(timeout)
                );            
            }
        }
    }

    private static (Mat sample, Mat template) PrepareSample(string sampleSrc, string templateSrc)
    {
        using var rawSample = Cv2.ImRead(sampleSrc);
        var scale = 1920.0 / rawSample.Width;

        var sample = new Mat();
        Cv2.Resize(rawSample, sample, new Size(rawSample.Width * scale, rawSample.Height * scale));
        var template = Cv2.ImRead(templateSrc);

        return (sample, template);
    }
    
    private static (Mat Source, Mat Target, Mat Timeout) TemplateMatch(Mat sample, Mat template)
    {
        using var result = new Mat();

        Cv2.MatchTemplate(sample, template, result, TemplateMatchModes.CCoeffNormed);
        Cv2.MinMaxLoc(result, out _, out var maxVal, out _, out var maxLoc);

        if (maxVal < .9) throw new InvalidImage("Could not match template image - portal frame missing or obstructed");
        return (
            CropMat(sample, new Rect(350, 40, 310, 37)),
            CropMat(sample, new Rect(maxLoc.X - 208, maxLoc.Y - 35, 243, 27)),
            CropMat(sample, new Rect(maxLoc.X + 3, maxLoc.Y + 24, 80, 20))
        );
    }

    private static (string text, float confidence) OcrRead(Mat sample)
    {
        var tessDataPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "tessData");
        using var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default);
        
        using var pix = Pix.LoadFromMemory(sample.ToBytes());
        using var page = engine.Process(pix, PageSegMode.SingleLine);
        return (page.GetText().Trim(), page.GetMeanConfidence());
    }

    private static string ExtractText(Mat sample)
    {
        var (text, confidence) = OcrRead(sample);
        Console.WriteLine($"[{text}] Confidence: {confidence}");
        return text;
    }

    private static string ExtractTimeout(Mat sample)
    {
        var (text, confidence) = OcrRead(sample);
        var matches = TimeoutSplitRegex().Matches(text);

        var result = "";
        foreach (Match match in matches)
        {
            var item = match.Value;
            var number = int.Parse(DigitGroupRegex().Match(item).Value);
            var unit = UnitRegex().Match(item).Value[0];
            
            result += $"{number}:{unit} ";
        }

        Console.WriteLine($"[{result.TrimEnd()}] Confidence: {confidence}");
        return result.TrimEnd();
    } 
    
    private static Mat CropMat(Mat source, Rect rect) => new (source, rect);
    
    private static bool IsTextRed(Mat image, double redRatioThreshold = 0.1)
    {
        // Convert BGR to HSV (better for color detection)
        using var hsv = new Mat();
        Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);

        // Define two red hue ranges (red wraps around 0° and 180° in HSV)
        var lowerRed1 = new Scalar(0, 100, 100);
        var upperRed1 = new Scalar(10, 255, 255);

        var lowerRed2 = new Scalar(160, 100, 100);
        var upperRed2 = new Scalar(180, 255, 255);

        // Threshold for red regions
        using var mask1 = new Mat();
        using var mask2 = new Mat();
        Cv2.InRange(hsv, lowerRed1, upperRed1, mask1);
        Cv2.InRange(hsv, lowerRed2, upperRed2, mask2);

        // Combine both masks
        using var redMask = new Mat();
        Cv2.BitwiseOr(mask1, mask2, redMask);

        // Calculate ratio of red pixels to total pixels
        double redPixels = Cv2.CountNonZero(redMask);
        double totalPixels = image.Rows * image.Cols;
        var ratio = redPixels / totalPixels;

        Console.WriteLine($"Red ratio: {ratio}");
        return ratio > redRatioThreshold;
    }
    
    [GeneratedRegex(@"(\d{1,2}\D+)")]
    private static partial Regex TimeoutSplitRegex();
    [GeneratedRegex(@"\d+")]
    private static partial Regex DigitGroupRegex();
    [GeneratedRegex(@"\D")]
    private static partial Regex UnitRegex();
}

public class InvalidImage(string message) : Exception(message);
