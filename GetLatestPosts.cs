using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Xml;
using System.ServiceModel.Syndication;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GitHubFuncs
{
    public static class GetLatestPosts
    {
        [FunctionName("GetLatestPosts")]
        public static string Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest request, ILogger log)
        {
            try
            {
                var posts = JsonConvert.SerializeObject(GetLatestFivePosts());
                if (posts == null) return null;
                return posts;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }            
        }

        public static IEnumerable<FeedItem> GetLatestFivePosts()
        {
            var reader = XmlReader.Create("https://sibeeshpassion.com/feed");
            var feed = SyndicationFeed.Load(reader);
            reader.Close();
            return (from itm in feed.Items select new FeedItem { Title = itm.Title.Text, Link = itm.Id }).ToList().Take(5);
        }

        public class FeedItem
        {
            public string Title { get; set; }
            public string Link { get; set; }
        }
    }
}
