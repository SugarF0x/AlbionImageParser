namespace AlbionImageParser;

[TestClass]
public sealed class Test1
{
    [TestMethod]
    public void SampleTest()
    {
        var assetsPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
        var iconPath = Path.GetFullPath(Path.Combine(assetsPath, @"portal-pass-icon-fhd.png"));
        var imagesPath = Path.GetFullPath(Path.Combine(assetsPath, @"samples"));
        var images = Directory.GetFiles(imagesPath);
        
        RegionDetection.Parse(images[13], iconPath);
        RegionDetection.Parse(images[14], iconPath);
        
        // foreach (var image in images)
        // {
        //     RegionDetection.Parse(image, iconPath);
        // }
    }
}