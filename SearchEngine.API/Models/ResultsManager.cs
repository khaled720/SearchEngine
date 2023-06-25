using System.Xml;
using Newtonsoft.Json;
using SearchEngine.API.Controllers;

namespace SearchEngine.API.Models
{
    public class ResultsManager
    {











        public static  List<YoutubeResultModel> GetYoutubeResultAsync(string query) {

            Console.WriteLine("YOUTUBE WAS CALLED");

            List<YoutubeResultModel> youtubeList = new();

         
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(APIs.Youtube_Endpoint);
                client.DefaultRequestHeaders.Clear();
                var requestUri = "search?q=" + query + "&max_results=10&part=snippet&key=" + Keys.Youtube_API_KEY;
                var res = client.GetAsync(requestUri).GetAwaiter().GetResult();
                var result = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var re = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(result);
                var finalresult = re["items"];

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
                            youtubeResult.PublishedAt = responseJson["items"][0]["snippet"]["publishedAt"];
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


            Console.WriteLine("YOUTUBE WAS ENDED");
            return youtubeList;
        }



        public static List<NewsModel> GetGoogleNewsResults(string query) 
        {

            Console.WriteLine("GOOGLE NEWS WAS CALLED");
            List<NewsModel> NewsItems = new();

            HttpClient googleNewsClient = new HttpClient();

                googleNewsClient.BaseAddress = new Uri(APIs.GoogleNews_Endpoint);
                var requestUri1 = "?q=" + query;
                var result1 = googleNewsClient.GetAsync(requestUri1).GetAwaiter().GetResult();
                var res1 = result1.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(res1);

                var channel = xmlDocument.ChildNodes[1].ChildNodes[0];

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

            Console.WriteLine("GOOGLE NEWS WAS ENDED");
            return NewsItems;
        }


    }
}
