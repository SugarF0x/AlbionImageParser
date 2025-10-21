namespace AlbionImageParser;

[TestClass]
public sealed class Test1
{
    [TestMethod]
    public void TemplateMatch()
    {
        var outputPath = Path.GetFullPath("../../../../.output");
        
        var assetsPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
        var iconPath = Path.GetFullPath(Path.Combine(assetsPath, @"portal-pass-icon-fhd.png"));
        var imagesPath = Path.GetFullPath(Path.Combine(assetsPath, @"samples"));
        var images = Directory.GetFiles(imagesPath);
        
        foreach (var image in images)
        {
            var newPath = Path.Combine(outputPath, Path.GetFileName(image));
            File.Delete(newPath);
            RegionDetection.TemplateMatch(image, iconPath, newPath);
        }
    }
}