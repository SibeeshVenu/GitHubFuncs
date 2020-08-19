using GitHubFuncs.ExtensionMethods;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;

namespace GitHubFuncs
{
    public class GetLatestPosts
    {
        private static ILogger _logger;
        private const string _fileName = "latestpost.png";
        private const string _blobContainerName = "github";
        [FunctionName("GetLatestPosts")]
        public async Task Run([TimerTrigger("0 0 */1 * * *")] TimerInfo myTimer, ILogger log)
        {
            try
            {
                _logger = log;
                await UplaodImageToStorage();
            }
            catch (System.Exception ex)
            {
                log.LogError($"Something went wrong: {ex}");
                throw ex;
            }
        }

        private static async Task<bool> UplaodImageToStorage()
        {
            try
            {
                var baseString = WriteOnImage(GetLatestFeeds());
                string convert = baseString.Replace("data:image/png;base64,", String.Empty);
                var bytes = Convert.FromBase64String(convert);
                var stream = new MemoryStream(bytes);
                if (CloudStorageAccount.TryParse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), out CloudStorageAccount cloudStorageAccount))
                {
                    var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                    var cloudBlobContainer = cloudBlobClient.GetContainerReference(_blobContainerName);
                    var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(_fileName);
                    cloudBlockBlob.Properties.ContentType = "image/png";
                    await cloudBlockBlob.UploadFromStreamAsync(stream);
                    _logger.LogInformation("Uploaded new image");
                }
                else
                {
                    _logger.LogError("Error in connection");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return false;
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
