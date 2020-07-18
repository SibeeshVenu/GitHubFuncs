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
        public static string Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest request, ILogger log)
        {
            try
            {
                var baseString = WriteOnImage(GetTopPost());
                log.LogInformation("Returning stream now!");
                return baseString; ;
            }
            catch (System.Exception ex)
            {
                log.LogError($"Something went wrong: {ex}");
                throw ex;
            }
        }

        private static string WriteOnImage(SyndicationItem feedItem)
        {
            using var stream = new MemoryStream();
            using var img = new Image<Rgba32>(500, 200);
            var font = SystemFonts.CreateFont("Arial", 18);
            img.Mutate(ctx => ctx.ApplyScalingWaterMark(font, feedItem.Summary.Text, Color.Black, 5, true));
            return img.ToBase64String(PngFormat.Instance);
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
