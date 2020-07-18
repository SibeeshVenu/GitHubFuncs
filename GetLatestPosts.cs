using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;
using GitHubFuncs.ExtensionMethods;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace GitHubFuncs
{
    public static class GetLatestPosts
    {
        [FunctionName("GetLatestPosts")]
        public static async Task<FileStreamResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest request,
            ExecutionContext context, ILogger log)
        {
            try
            {
                // var imgPath = Path.Combine(context.FunctionDirectory, "..\\images\\wordart.png");

                // using var imgStream = new FileStream(imgPath, FileMode.Open, FileAccess.ReadWrite);
                log.LogInformation("Returning stream now!");
                return new FileStreamResult(await WriteOnImage(GetTopPost()), "image/png");
            }
            catch (System.Exception ex)
            {
                log.LogError($"Something went wrong: {ex}");
                throw ex;
            }
        }

        private static async Task<Stream> WriteOnImage(SyndicationItem feedItem)
        {
            using var stream = new MemoryStream();
            using var img = new Image<Rgba32>(500, 200);
            var font = SystemFonts.CreateFont("Arial", 18);
            img.Mutate(ctx => ctx.ApplyScalingWaterMark(font, feedItem.Summary.Text, Color.Black, 5, true));
            await img.SaveAsync(stream, new PngEncoder());
            return stream;
        }

        public static SyndicationItem GetTopPost()
        {
            var reader = XmlReader.Create("https://sibeeshpassion.com/feed");
            var feed = SyndicationFeed.Load(reader);
            reader.Close();
            return feed.Items.FirstOrDefault();
        }
    }
}
