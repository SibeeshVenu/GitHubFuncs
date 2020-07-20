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
using System.IO;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GitHubFuncs
{
    public static class GetLatestPosts
    {
        [FunctionName("GetLatestPosts")]
        public static FileStreamResult Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest request, ILogger log)
        {
            try
            {
                var baseString = WriteOnImage(GetTopPost());
                // Had to do this, as it was throwing error "The input is not a valid Base-64 string as it contains a non-base 64 character"
                string convert = baseString.Replace("data:image/png;base64,", String.Empty);
                var bytes = Convert.FromBase64String(convert);
                var result = new FileStreamResult(new MemoryStream(bytes), "image/png");
                log.LogInformation("Returning stream now!");
                request.HttpContext.Response.Headers.Add("Cache-Control", "s-maxage=1, stale-while-revalidate");
                return result; ;
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
            using var img = new Image<Rgba32>(850, 200);
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
