using OpenCvSharp;
using OpenCvSharp.Features2D;

namespace AlbionImageParser;

public class RegionDetection
{
    public static void SiftMatches(Mat input, Mat template, string outPath)
    {
        var scene = new Mat();
        var scale = 1920.0 / input.Height;
        Cv2.Resize(input, scene, new Size(input.Width * scale, input.Height * scale));
        
        if (scene.Empty() || template.Empty())
        {
            Console.WriteLine("Could not load images.");
            return;
        }

        var sift = SIFT.Create();
        var bf = new BFMatcher(NormTypes.L2, crossCheck: false);

        using var descTemplate = new Mat();
        using var descScene = new Mat();

        sift.DetectAndCompute(template, null, out var kpTemplate, descTemplate);
        sift.DetectAndCompute(scene, null, out var kpScene, descScene);

        if (descTemplate.Empty() || descScene.Empty())
        {
            Console.WriteLine("No descriptors found.");
            return;
        }

        // KNN matching
        var knnMatches = bf.KnnMatch(descTemplate, descScene, k: 2);

        const double threshold = .5;
        
        // Lowe’s ratio test
        var goodMatches = new List<DMatch>();
        foreach (var m in knnMatches)
        {
            if (m.Length == 2 && m[0].Distance < threshold * m[1].Distance)
                goodMatches.Add(m[0]);
        }

        Console.WriteLine($"Good matches: {goodMatches.Count}");

        if (goodMatches.Count >= 4)
        {
            // Use Point2f (float) for homography
            var srcPts = goodMatches.Select(m => kpTemplate[m.QueryIdx].Pt).ToArray();
            var dstPts = goodMatches.Select(m => kpScene[m.TrainIdx].Pt).ToArray();
            
            Mat homography = Cv2.FindHomography(InputArray.Create(srcPts), InputArray.Create(dstPts),
                                                HomographyMethods.Ransac, 5.0);

            if (!homography.Empty())
            {
                Point2f[] templateCorners =
                {
                    new(0, 0),
                    new(template.Cols, 0),
                    new(template.Cols, template.Rows),
                    new(0, template.Rows)
                };

                Point2f[] sceneCorners = Cv2.PerspectiveTransform(templateCorners, homography);

                // Draw box around detected area
                for (int i = 0; i < 4; i++)
                {
                    var pt1 = sceneCorners[i];
                    var pt2 = sceneCorners[(i + 1) % 4];
                    Cv2.Line(scene, (int)pt1.X, (int)pt1.Y, (int)pt2.X, (int)pt2.Y, Scalar.White, 3);
                }
                    
                Cv2.ImWrite(outPath, scene);
            }
            else
            {
                Console.WriteLine("Homography could not be computed.");
            }
        }
        else
        {
            Console.WriteLine("Not enough good matches found.");
        }
    }
}