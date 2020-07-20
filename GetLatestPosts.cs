using GitHubFuncs.ExtensionMethods;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;

namespace GitHubFuncs
{
    public static class GetLatestPosts
    {
        [FunctionName("GetLatestPosts")]
        public static FileStreamResult Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest request, ILogger log)
        {
            try
            {
                var baseString = WriteOnImage(GetLatestFeeds());
                // Had to do this, as it was throwing error "The input is not a valid Base-64 string as it contains a non-base 64 character"
                string convert = baseString.Replace("data:image/png;base64,", String.Empty);
                var bytes = Convert.FromBase64String(convert);
                var result = new FileStreamResult(new MemoryStream(bytes), Configuration.ContentType);
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

        private static string WriteOnImage(IEnumerable<SyndicationItem> feedItems)
        {
            var titles = string.Join(", ", feedItems.Select(s => s.Title.Text).ToList());
            using var img = new Image<Rgba32>(Configuration.ImageWidth, Configuration.ImageHeight);
            var font = SystemFonts.CreateFont(Configuration.Font, Configuration.FontSize);
            img.Mutate(ctx => ctx.ApplyScalingWaterMark(font, titles, Color.Black, 5, true));
            return img.ToBase64String(PngFormat.Instance);
        }

        public static IEnumerable<SyndicationItem> GetLatestFeeds()
        {
            var reader = XmlReader.Create(Configuration.BlogLink);
            var feed = SyndicationFeed.Load(reader);
            reader.Close();
            return feed.Items.Take(5);
        }
    }
}
