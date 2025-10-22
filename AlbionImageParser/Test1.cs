namespace AlbionImageParser;

[TestClass]
public sealed class Test1
{
    public static IEnumerable<object[]> Cases =>
    [
        ["19:23:00"],
        ["13:51:00"],
        ["19:22:00"],
        ["00:06:50"],
        ["08:17:00"],
        ["07:18:00"],
        ["13:48:00"],
        ["03:22:00"],
        ["14:54:00"],
        ["11:41:00"],
        ["16:41:00"],
        ["01:19:00"],
        ["17:03:00"],
        ["01:18:00"],
        ["00:31:12"],
        ["01:34:00"],
        ["04:51:00"],
        ["17:01:00"],
        ["01:29:00"],
        ["07:20:00"],
        ["13:11:00"],
        ["16:40:00"],
        ["09:21:00"],
        ["16:39:00"],
        ["03:08:00"],
        ["07:43:00"],
        ["10:42:00"],
        ["09:19:00"],
        ["15:41:00"],
        ["15:04:00"],
        ["00:36:05"],
        ["14:01:00"],
        ["13:59:00"],
        ["05:22:00"],
        ["06:29:00"],
        ["04:09:00"],
        ["06:12:00"],
        ["09:51:00"],
        ["09:37:00"],
        ["09:50:00"],
        ["10:40:00"],
        ["05:25:00"],
        ["17:20:00"],
        ["09:35:00"],
        ["08:26:00"],
        ["05:05:00"],
        ["17:49:00"],
        ["04:15:00"],
        ["04:14:00"],
        ["13:42:00"],
        ["05:49:00"],
        ["00:28:56"]
    ];
    
    [DataTestMethod]
    [DynamicData(nameof(Cases))]
    public void TimeoutTest(string expectedTimeout)
    {
        var assetsPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
        var iconPath = Path.GetFullPath(Path.Combine(assetsPath, @"portal-pass-icon-fhd.png"));
        var imagesPath = Path.GetFullPath(Path.Combine(assetsPath, @"samples"));
        var images = Directory.GetFiles(imagesPath);

        var index = Cases.TakeWhile(e => (string)e[0] != expectedTimeout).Count();
        var data = RegionDetection.Parse(images[index], iconPath);
        Assert.AreEqual(expectedTimeout, data.Timeout, $"At {index} index");
    }
}