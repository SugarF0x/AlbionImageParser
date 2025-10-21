using System.Text.RegularExpressions;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Features2D;
using Tesseract;
using Rect = OpenCvSharp.Rect;

namespace AlbionImageParser;

public static partial class RegionDetection
{
    public struct SampleRegionData(string source, string target, string timeout);
    public static SampleRegionData? Parse(string sampleSrc, string templateSrc)
    {
        var (sample, template) = PrepareSample(sampleSrc, templateSrc);
        using (sample)
        using (template)
        {
            var regions = TemplateMatch(sample, template);
        
            if (regions is not { } r) return null;
            return new SampleRegionData(
                ExtractText(sample, r.Source),
                ExtractText(sample, r.Target),
                ExtractTimeout(sample, r.Timeout)
            );            
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

    private readonly record struct MatchedSampleRegions(Rect Source, Rect Target, Rect Timeout);
    private static MatchedSampleRegions? TemplateMatch(Mat sample, Mat template)
    {
        using var result = new Mat();

        Cv2.MatchTemplate(sample, template, result, TemplateMatchModes.CCoeffNormed);
        Cv2.MinMaxLoc(result, out _, out var maxVal, out _, out var maxLoc);

        if (maxVal < .9) return null;
        return new MatchedSampleRegions(
            new Rect(350, 40, 660 - 350, 77 - 40),
            new Rect(maxLoc.X - 208, maxLoc.Y - 35, 208 + 35, 35 - 8),
            new Rect(maxLoc.X + 3, maxLoc.Y + 24, 83 - 3, 20)
        );
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("", "CA1416", Justification = "Not accessible on non windows machines anyway")]
    private static (string text, float confidence) OCR(Mat sample, Rect rect)
    {
        using var crop = CropMat(sample, rect);
        
        var tessDataPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "tessdata");
        using var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default);
        
        using var pix = PixConverter.ToPix(crop.ToBitmap());
        using var page = engine.Process(pix, PageSegMode.SingleLine);
        return (page.GetText().Trim(), page.GetMeanConfidence());
    }

    private static string ExtractText(Mat sample, Rect rect)
    {
        var (text, confidence) = OCR(sample, rect);
        Console.WriteLine($"[{text}] Confidence: {confidence}");
        return text;
    }

    private static string ExtractTimeout(Mat sample, Rect rect)
    {
        var (text, confidence) = OCR(sample, rect);
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
    
    [GeneratedRegex(@"(\d{1,2}\D+)")]
    private static partial Regex TimeoutSplitRegex();
    [GeneratedRegex(@"\d+")]
    private static partial Regex DigitGroupRegex();
    [GeneratedRegex(@"\D")]
    private static partial Regex UnitRegex();
}
