namespace AlbionImageParser;

[TestClass]
public sealed class Test1
{
    public static IEnumerable<object[]> Cases =>
    [
        ["15:54:00"],
        ["15:49:00"],
        ["13:49:00"],
        ["00:25:45"],
        ["07:59:00"],
        ["06:45:00"],
        ["00:23:07"],
        ["06:04:00"],
        ["06:43:00"],
        ["06:41:00"],
        ["09:08:00"],
        ["10:32:00"],
        ["12:26:00"],
        ["07:56:00"],
        ["11:51:00"],
        ["03:52:00"],
        ["05:28:00"],
        ["11:50:00"],
        ["06:30:00"],
        ["09:17:00"],
        ["09:53:00"],
        ["06:28:00"],
        ["13:01:00"],
        ["13:10:00"],
        ["07:41:00"],
        ["07:39:00"],
        ["02:04:00"],
        ["03:16:00"],
        ["17:40:00"],
        ["00:24:24"],
        ["06:58:00"],
        ["17:39:00"],
        ["06:56:00"],
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