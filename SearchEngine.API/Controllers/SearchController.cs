using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SearchEngine.API.Models;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

namespace SearchEngine.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get(string query)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(APIs.Youtube_Endpoint);
            client.DefaultRequestHeaders.Clear();
            var requestUri = "search?q=" + query + "&part=snippet&key=" + Keys.Youtube_API_KEY;
            var res = client.GetAsync(requestUri).GetAwaiter().GetResult();
            var result = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var re = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(result);
            var finalresult = re["items"];
            List<YoutubeResultModel> youtubeList = new();
            foreach (var item in finalresult)
            {
                var youtubeResult = new YoutubeResultModel();

                string kind = item["id"]["kind"];

                switch (kind.ToLower().Replace("youtube#", ""))
                {
                    case "channel":
                        youtubeResult.ChannelId = item["id"]["channelId"];
                        youtubeResult.ChannelTitle = item["snippet"]["title"];

                        youtubeResult.PublishedAt = item["snippet"]["publishedAt"];
                        youtubeResult.ChannelDescription = item["snippet"]["description"];

                        youtubeResult.ResultType = YoutubeResultTypes.Channel.ToString();
                        break;

                    case "video":
                        youtubeResult.VideoId = item["id"]["videoId"];
                        youtubeResult.ResultType = YoutubeResultTypes.Video.ToString();
                        var videoRequestUri =
                            APIs.Youtube_Endpoint
                            + "videos?key="
                            + Keys.Youtube_API_KEY
                            + "&part=snippet,statistics&id="
                            + item["id"]["videoId"];
                        var responseObj = client.GetAsync(videoRequestUri).GetAwaiter().GetResult();
                        var reslt = responseObj.Content
                            .ReadAsStringAsync()
                            .GetAwaiter()
                            .GetResult();
                        var responseJson = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(reslt);
                        youtubeResult.PublishedAt = responseJson["items"][0]["snippet"][ "publishedAt"];
                        youtubeResult.VideoDescription = responseJson["items"][0]["snippet"][
                            "description"
                        ];
                        youtubeResult.VideoTitle = responseJson["items"][0]["snippet"]["title"];
                        youtubeResult.ChannelId = responseJson["items"][0]["snippet"]["channelId"];
                        youtubeResult.ChannelTitle = responseJson["items"][0]["snippet"][
                            "channelTitle"
                        ];

                        youtubeResult.VideoViews = responseJson["items"][0]["statistics"][
                            "viewCount"
                        ];
                        youtubeResult.VideoLikes = responseJson["items"][0]["statistics"][
                            "likeCount"
                        ];
                        youtubeResult.VideoComments = responseJson["items"][0]["statistics"][
                            "commentCount"
                        ];

                        break;

                    case "playlist":
                        youtubeResult.PlaylistId = item["id"]["playlistId"];

                        youtubeResult.PlaylistTitle = item["snippet"]["title"];
                        youtubeResult.ChannelId = item["snippet"]["channelId"];

                        youtubeResult.PublishedAt = item["snippet"]["publishedAt"];
                        youtubeResult.PlaylistDesceription = item["snippet"]["description"];
                        youtubeResult.ResultType = YoutubeResultTypes.Playlist.ToString();
                        break;

                    default:
                        break;
                }

                youtubeList.Add(youtubeResult);
            }

            // google news
            HttpClient googleNewsClient = new HttpClient();

            googleNewsClient.BaseAddress = new Uri(APIs.GoogleNews_Endpoint);
            var requestUri1 = "?q=" + query;
            var result1 = googleNewsClient.GetAsync(requestUri1).GetAwaiter().GetResult();
            var res1 = result1.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(res1);

            var channel = xmlDocument.ChildNodes[1].ChildNodes[0];
            List<NewsModel> NewsItems = new();
            int counter = 0;
            foreach (XmlNode item in channel.ChildNodes)
            {
                if (item.Name == "item")
                {
                    NewsModel newsModel = new NewsModel();

                    foreach (XmlNode property in item.ChildNodes)
                    {
                        switch (property.Name)
                        {
                            case "title":
                                newsModel.Title = property.InnerText;

                                break;

                            case "description":
                                //               newsModel.Description = property.InnerText;
                                break;

                            case "link":
                                newsModel.DescriptionArticleLink = property.InnerText;
                                break;

                            case "source":
                                newsModel.Source = property.InnerText;
                                newsModel.SourceLink = property.Attributes[0].Value;

                                break;

                            case "guid":
                                newsModel.Guid = property.InnerText;
                                break;
                            case "pubDate":

                                newsModel.PubDate = property.InnerText;
                                break;

                            default:
                                break;
                        }
                    }
                    NewsItems.Add(newsModel);
                }

                counter++;
                if (counter == 11)
                    break;
            }

            ////Twitter
            HttpClient Twitterclient = new HttpClient();
            Twitterclient.BaseAddress = new Uri(APIs.Twitter_Endpoint);
            Twitterclient.DefaultRequestHeaders.Clear();
            Twitterclient.DefaultRequestHeaders.Add(
                "Authorization",
                "Bearer " + Keys.Twitter_BearerToken
            );
            var qq = query.Replace("@", "from:");
            var requestUri3 =
                "tweets/search/recent?query="
                + qq
                + "&max_results=10&tweet.fields=created_at&user.fields=description&expansions=author_id";
            var res3 = Twitterclient.GetAsync(requestUri3).GetAwaiter().GetResult();
            var result3 = res3.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            ////   var result3 = await res.Content.ReadAsStringAsync();
            var respons = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(result3);
            List<TwitteModel> TwittesList = new();
            foreach (var item in respons["data"])
            {
                var Twitte = new TwitteModel();
                Twitte.Text = item["text"];
                Twitte.AuthorId = item["author_id"];
                Twitte.CreatedAt = item["created_at"];
                Twitte.Id = item["id"];

                foreach (var user in respons["includes"]["users"])
                {
                    if (user["id"] == item["author_id"])
                    {
                        Twitte.TwitteUser = new();
                        Twitte.TwitteUser.Id = user["id"];
                        Twitte.TwitteUser.Description = user["description"];
                        Twitte.TwitteUser.Name = user["name"];
                        Twitte.TwitteUser.Username = user["username"];
                    }
                }

                TwittesList.Add(Twitte);
            }

            SearchResult searchResult = new SearchResult();
            searchResult.Source = SearchResultType.Google_News.ToString();
            searchResult.SourceID = (int)SearchResultType.Google_News;
            searchResult.Result = NewsItems;
            SearchResult searchResult2 = new SearchResult();
            searchResult2.Source = SearchResultType.Youtube.ToString();
            searchResult2.SourceID = (int)SearchResultType.Youtube;
            searchResult2.Result = youtubeList;

            SearchResult searchResult3 = new SearchResult();
            searchResult3.Source = SearchResultType.Twitter.ToString();
            searchResult3.SourceID = (int)SearchResultType.Twitter;
            searchResult3.Result = TwittesList;

            List<SearchResult> searchList = new();
            searchList.Add(searchResult);
            searchList.Add(searchResult2);
            searchList.Add(searchResult3);

            return Ok(searchList);
        }
    }

  

    public class YoutubeResultModel
    {
        public string? VideoId { get; set; }

        public string? VideoTitle { get; set; }

        public string? VideoDescription { get; set; }
        public string? VideoViews { get; set; }
        public string? VideoLikes { get; set; }

		public string? VideoThumbImage { get; set; }

		public string? PlaylistId { get; set; }
        public string? PlaylistTitle { get; set; }
        public string? PlaylistDesceription { get; set; }

        public string? ChannelId { get; set; }

        public string? ChannelTitle { get; set; }

        public string? ChannelDescription { get; set; }

        public string? PublishedAt { get; set; }

        public string ResultType { get; set; }

        //in there
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? VideoComments { get; internal set; }
    }

    public enum YoutubeResultTypes
    {
        Video,
        Playlist,
        Channel
    }

    /*
    public class TwitterResultModel
    {
    
    
    
    
    }*/
    public class TwitteModel
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string CreatedAt { get; set; }
        public string AuthorId { get; set; }

        public TwitterUserModel TwitteUser { get; set; }
    }

    public class TwitterUserModel
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public string? Username { get; set; }
        public string? Description { get; set; }
    }
}
