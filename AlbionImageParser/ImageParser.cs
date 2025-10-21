using System.Drawing;

namespace AlbionImageParser;

using System;
using OpenCvSharp;
using Tesseract;

class ParsedInfo
{
    public string Source { get; set; }
    public string Target { get; set; }
    public TimeSpan TimeRemaining { get; set; }

    public override string ToString() =>
        $"Source: {Source}\nTarget: {Target}\nTime: {TimeRemaining}";
}

class ScreenshotParser
{
    private readonly string tessDataPath;
    private readonly string portalIconPath;
    private const double MatchThreshold = 0.7; // Confidence threshold

    public ScreenshotParser(string tessDataPath, string portalIconPath)
    {
        this.tessDataPath = tessDataPath;
        this.portalIconPath = portalIconPath;
    }

    public ParsedInfo Parse(string imagePath)
    {
        using var src = Cv2.ImRead(imagePath, ImreadModes.Color);
        if (src.Empty()) throw new Exception("Could not read screenshot.");

        var result = new ParsedInfo();

        // --- STEP 1: Extract top banner (source) ---
        var topRect = new OpenCvSharp.Rect(
            (int)(src.Width * 0.2),
            (int)(src.Height * 0.01),
            (int)(src.Width * 0.6),
            (int)(src.Height * 0.08)
        );
        var topCrop = new Mat(src, topRect);
        result.Source = ExtractText(topCrop, "eng", "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");

        // --- STEP 2: Find portal box via template matching ---
        using var portalIcon = Cv2.ImRead(portalIconPath, ImreadModes.Color);
        if (portalIcon.Empty()) throw new Exception("Missing portal icon template.");

        // Scale portal icon if necessary (for robustness)
        var (bestMatchPoint, bestScore) = FindBestTemplateMatch(src, portalIcon);

        if (bestScore < MatchThreshold)
        {
            Console.WriteLine("Portal box not found. Aborting.");
            return null;
        }

        // Estimate bounding box relative to match point
        int boxX = Math.Max(bestMatchPoint.X - (int)(portalIcon.Width * 0.5), 0);
        int boxY = Math.Max(bestMatchPoint.Y - (int)(portalIcon.Height * 0.5), 0);
        int boxW = Math.Min((int)(portalIcon.Width * 9.5), src.Width - boxX);
        int boxH = Math.Min((int)(portalIcon.Height * 3.2), src.Height - boxY);

        var portalRect = new OpenCvSharp.Rect(boxX, boxY, boxW, boxH);
        var portalCrop = new Mat(src, portalRect);

        // --- STEP 3: Preprocess the portal box for OCR ---
        Cv2.CvtColor(portalCrop, portalCrop, ColorConversionCodes.BGR2GRAY);
        Cv2.AdaptiveThreshold(portalCrop, portalCrop, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 21, 5);

        // --- STEP 4: Extract Target and Timer subregions ---
        var targetRect = new OpenCvSharp.Rect(0, 0, portalCrop.Width, (int)(portalCrop.Height * 0.3));
        var timerRect = new OpenCvSharp.Rect(
            (int)(portalCrop.Width * 0.55),
            (int)(portalCrop.Height * 0.55),
            (int)(portalCrop.Width * 0.4),
            (int)(portalCrop.Height * 0.4)
        );

        string target = ExtractText(new Mat(portalCrop, targetRect), "eng", "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-");
        string timeText = ExtractText(new Mat(portalCrop, timerRect), "eng", "0123456789 ");

        result.Target = target;
        result.TimeRemaining = ParseTime(timeText);

        return result;
    }

    private (Point Point, double Score) FindBestTemplateMatch(Mat source, Mat template)
    {
        // Try matching at multiple scales for robustness
        double bestScore = 0;
        Point bestPoint = default;

        foreach (var scale in new double[] { 0.75, 1.0, 1.25 })
        {
            int newW = (int)(template.Width * scale);
            int newH = (int)(template.Height * scale);
            if (newW < 20 || newH < 20) continue;

            using var resizedTemplate = template.Resize(new Size(newW, newH));
            using var result = new Mat();

            Cv2.MatchTemplate(source, resizedTemplate, result, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out Point maxLoc);

            if (maxVal > bestScore)
            {
                bestScore = maxVal;
                bestPoint = maxLoc;
            }
        }

        return (bestPoint, bestScore);
    }

    private string ExtractText(Mat image, string lang, string whitelist)
    {
        using var engine = new TesseractEngine(tessDataPath, lang, EngineMode.Default);
        engine.SetVariable("tessedit_char_whitelist", whitelist);
        engine.SetVariable("user_defined_dpi", "300");
        
        using var pix = PixConverter.ToPix(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image));
        using var page = engine.Process(pix, PageSegMode.SingleLine);
        return page.GetText().Trim();
    }

    private TimeSpan ParseTime(string text)
    {
        string cleaned = text.Replace("ч", "h").Replace("м", "m").Replace(" ", "");
        int hours = 0, minutes = 0;
        try
        {
            int hIdx = cleaned.IndexOf('h');
            int mIdx = cleaned.IndexOf('m');

            if (hIdx > 0) hours = int.Parse(cleaned[..hIdx]);
            if (mIdx > 0 && mIdx > hIdx) minutes = int.Parse(cleaned[(hIdx + 1)..mIdx]);
        }
        catch { }
        return new TimeSpan(hours, minutes, 0);
    }
}