using Emgu.CV;
using Rce2;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.Versioning;
using Emgu.CV.Structure;

namespace OpenCvAgent.Infrastructure;

public class OpenCvService
{
    private readonly Rce2Service _rce2Service;

    public OpenCvService(Rce2Service rce2Service)
    {
        _rce2Service = rce2Service;
    }

    public void Init()
    {
        Task.Run(MatchTemplates);
    }

    private async Task MatchTemplates()
    {
        var templates = await LoadTemplates();
        //CvInvoke.Imshow("x", templates[0]);
        //CvInvoke.WaitKey();

        var matches = new Mat();
        while (true)
        {
            try
            {
                using var screen = GetScreen(0, 0, 1000, 500);
                //CvInvoke.Imshow("x", screen);
                //CvInvoke.WaitKey();

                foreach (var template in templates)
                {
                    CvInvoke.MatchTemplate(screen, template, matches, Emgu.CV.CvEnum.TemplateMatchingType.SqdiffNormed);
                    var (matchValue, matchRect) = ProcessMatches(matches, template.Size);

                    if (matchValue < 0.1)
                    {
                        //CvInvoke.Rectangle(screen, matchRect, new MCvScalar(255, 0, 0));
                        //CvInvoke.Imshow("x", screen);
                        //CvInvoke.WaitKey();

                        await _rce2Service.Send("match-found", matchValue);
                        await Task.Delay(5000);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                // ignore
            }

            await Task.Delay(333);
        }
    }

    private (double matchValue, Rectangle matchRect) ProcessMatches(Mat matches, Size templateSize)
    {
        var minVal = 0.0;
        var maxVal = 0.0;
        var minLoc = default(Point);
        var maxLoc = default(Point);
        CvInvoke.MinMaxLoc(matches, ref minVal, ref maxVal, ref minLoc, ref maxLoc);
        var rectangle = new Rectangle(minLoc, templateSize);

        return (minVal, rectangle);
    }

    private async Task<List<Mat>> LoadTemplates()
    {
        return new List<Mat>
        {
            CvInvoke.Imread("C:\\Users\\User\\Desktop\\bin.png"),
            CvInvoke.Imread("C:\\Users\\User\\Desktop\\bin.png"),
        };
    }

    [SupportedOSPlatform("windows")]
    private Mat GetScreen(int x, int y, int width, int height)
    {
        using var bitmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        using var graphics = Graphics.FromImage(bitmp);
        graphics.CopyFromScreen(x, y, 0, 0, bitmp.Size);

        return bitmp.ToMat();
    }
}
