using OpenCvSharp;
using OpenCvSharp.Features2D;

namespace AlbionImageParser;

public static class RegionDetection
{
    public static void TemplateMatch(string sampleSrc, string templateSrc, string outPath)
    {
        using var rawSample = Cv2.ImRead(sampleSrc);
        var scale = 1920.0 / rawSample.Width;

        using var sample = new Mat();
        Cv2.Resize(rawSample, sample, new Size(rawSample.Width * scale, rawSample.Height * scale));
        using var template = Cv2.ImRead(templateSrc);
        using var result = new Mat();

        Cv2.MatchTemplate(sample, template, result, TemplateMatchModes.CCoeffNormed);
        Cv2.MinMaxLoc(result, out _, out _, out _, out var maxLoc);
        
        // source
        Cv2.Rectangle(sample, new Point(350, 40), new Point(660, 77), Scalar.White);
        // target
        Cv2.Rectangle(sample, new Point(maxLoc.X - 208, maxLoc.Y - 35), new Point(maxLoc.X + 35, maxLoc.Y - 8), Scalar.White);
        // timeout
        Cv2.Rectangle(sample, new Point(maxLoc.X + 3, maxLoc.Y + 24), new Point(maxLoc.X + 83, maxLoc.Y + 44), Scalar.White);
        
        Cv2.ImWrite(outPath, sample);
    }
}