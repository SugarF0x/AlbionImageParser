namespace AlbionImageParser;

[TestClass]
public sealed class Test1
{
    public static IEnumerable<object[]> Cases =>
    [
        ["15:54:00"],
        ["15:49:00"],
        ["13:49:00"],
    ];
    
    [DataTestMethod]
    [DynamicData(nameof(Cases))]
    public void TimeoutTest(string expectedTimeout)
    {
        var assetsPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
        var iconPath = Path.GetFullPath(Path.Combine(assetsPath, @"portal-pass-icon-fhd-2.png"));
        var imagesPath = Path.GetFullPath(Path.Combine(assetsPath, @"samples"));
        var images = Directory.GetFiles(imagesPath);

        var index = Cases.TakeWhile(e => (string)e[0] != expectedTimeout).Count();
        var data = RegionDetection.Parse(images[index], iconPath);
        Assert.AreEqual(expectedTimeout, data.Timeout, $"At {index} index");
    }
}