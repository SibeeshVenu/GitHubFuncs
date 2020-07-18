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

namespace GitHubFuncs
{
    public static class GetLatestPosts
    {
        [FunctionName("GetLatestPosts")]
        public static string Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest request, ILogger log)
        {
            try
            {
                var postImage = WriteOnImage(GetTopPost());
                return postImage;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        private static string WriteOnImage(SyndicationItem feedItem)
        {
            using var img = Image.Load("images/canvas.jpg");
            var font = SystemFonts.CreateFont("Arial", 20);
            using var img2 = img.Clone(ctx => ctx.ApplyScalingWaterMark(font, feedItem.Summary.Text, Color.White, 5, true));
            return img2.ToBase64String(PngFormat.Instance);
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
