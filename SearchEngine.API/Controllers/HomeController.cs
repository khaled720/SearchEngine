﻿using System.Globalization;
using System.Linq.Expressions;
using System.Reflection.PortableExecutable;
using System.Xml;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PuppeteerSharp;
using SearchEngineWeb;
using SearchEngineWeb.Models;
using static System.Net.WebRequestMethods;

namespace SearchEngine.API.Controllers
{
    public class HomeController : Controller
    {
        
        [HttpGet]
        public IActionResult Translate()
        {
            var cul = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            var langs = new Dictionary<string, string>();
            foreach (var item in cul)
            {
                try
                {
                    langs.Add(item.TwoLetterISOLanguageName+"-"+item.TextInfo.IsRightToLeft, item.EnglishName.Split(" ")[0]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            langs.OrderBy(y => y.Value);
            //RegionInfo regionInfo = new(cul[13].LCID);

            var translateModel = new TranslateModel() { Languages = langs };
            return View(translateModel);
        }

        public async Task<IActionResult> All(string query)
        {
            var browserFetcher = new BrowserFetcher();

            await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            string[] args = { "--no-sandbox" };

            var browser = await Puppeteer.LaunchAsync(
                new LaunchOptions
                {
                    Headless = true ,
                    DefaultViewport = null,
                    Args = args,
                    Timeout = 0
                }
            );



            GoogleEngineResult googleEngineResult = new GoogleEngineResult();
            googleEngineResult.Query = query;
            var googleTask =
                     Task.Run(async () =>
                    {
                       

                        using (var page = await browser.NewPageAsync())
                        {
                            //    await page.SetExtraHttpHeadersAsync(headers);
                            await page.GoToAsync(APIs.GoogleEngine_Endpoint);
                            await page.WaitForSelectorAsync("#APjFqb");
                            await page.FocusAsync("#APjFqb");
                            await page.Keyboard.TypeAsync(query);
                            await page.Keyboard.PressAsync(PuppeteerSharp.Input.Key.Enter);
                            await page.WaitForNavigationAsync();

                            await page.SetJavaScriptEnabledAsync(true);
                            //         await page.EvaluateExpressionAsync("window.scrollBy(0, 2000)");



                            var content = await page.GetContentAsync();
                            HtmlDocument htmlDocument = new();
                            htmlDocument.LoadHtml(content);
                            var Results = htmlDocument.DocumentNode.ChildNodes
                                .Descendants("a")
                                     .Where(r => r.ParentNode.ParentNode.Attributes["class"] != null)
                                    .Where(y => y.ParentNode.ParentNode.Attributes["class"].Value == "yuRUbf")
                                .ToList();
                            var ListOfDesc = htmlDocument.DocumentNode.ChildNodes
                                .Descendants("span")
                                .Where(r => r.ParentNode.Attributes["class"] != null)
                                .Where(
                                    y =>
                                        y.ParentNode.Attributes["class"].Value
                                        == "VwiC3b yXK7lf MUxGbd yDYNvb lyLwlc lEBKkf"
                                )
                                .ToList();




                            var Results2ndPage = new List<HtmlNode>();
                            var ListOfDesc2ndPage = new List<HtmlNode>();

                            try
                            {
                                await page.ClickAsync("#pnnext");
                                await page.WaitForNavigationAsync();

                                Results2ndPage = htmlDocument.DocumentNode.ChildNodes
              .Descendants("a")
              .Where(r => r.ParentNode.ParentNode.Attributes["class"] != null)
              .Where(y => y.ParentNode.ParentNode.Attributes["class"].Value == "yuRUbf")
              .ToList();

                                ListOfDesc2ndPage = htmlDocument.DocumentNode.ChildNodes
                                    .Descendants("span")
                                    .Where(r => r.ParentNode.Attributes["class"] != null)
                                    .Where(
                                        y =>
                                            y.ParentNode.Attributes["class"].Value
                                            == "VwiC3b yXK7lf MUxGbd yDYNvb lyLwlc lEBKkf"
                                    )
                                    .ToList();

                                Results.AddRange(Results2ndPage);
                                ListOfDesc.AddRange(ListOfDesc);


                            }
                            catch (Exception)
                            {


                            }






                            var counter = 0;




                            foreach (var item in Results)
                            {

                                try
                                {
                                    googleEngineResult.genericGoogleResults.Add(
                                        new GenericGoogleResult()
                                        {
                                            Type = "Google",
                                            Header = item.ChildNodes[1].InnerText,
                                            Link = item.ChildNodes[2].InnerText
                                                .Replace("instagram.com", "")
                                                .Replace(" > ", "/")
                                                .Replace(" › ", "instagram.com/"),
                                            Description = ListOfDesc[counter].InnerText
                                        }
                                    );

                                }
                                catch (Exception)
                                {
                                    continue;
                                }
                                counter++;
                            }

                            await page.CloseAsync();
                        }
                        await browser.CloseAsync();
                    });


            /////////////////////////////////////////////////////////////////////////



            List<GenericGoogleResult> InstgramResultsList = new();

            var instagramTask = Task.Run(async () =>
            {
                try
                {
        using (var page = await browser.NewPageAsync())
                {
                    //    await page.SetExtraHttpHeadersAsync(headers);
                    await page.GoToAsync(APIs.GoogleEngine_Endpoint,timeout:0);
                    await page.WaitForSelectorAsync("#APjFqb");
                    await page.FocusAsync("#APjFqb");
                    await page.Keyboard.TypeAsync("site:Instagram.com " + query);
                    await page.Keyboard.PressAsync(PuppeteerSharp.Input.Key.Enter);
                    // await page.WaitForNavigationAsync();
                    await Task.Delay(2000);

                    await page.SetJavaScriptEnabledAsync(true);
                    //         await page.EvaluateExpressionAsync("window.scrollBy(0, 2000)");



                    var content = await page.GetContentAsync();
                    HtmlDocument htmlDocument = new();
                    htmlDocument.LoadHtml(content);
                    var Results = htmlDocument.DocumentNode.ChildNodes
                        .Descendants("a")
                           .Where(r => r.ParentNode.ParentNode.Attributes["class"] != null)
                            .Where(y => y.ParentNode.ParentNode.Attributes["class"].Value == "yuRUbf")
                        .ToList();
                    var ListOfDesc = htmlDocument.DocumentNode.ChildNodes
                        .Descendants("span")
                        .Where(r => r.ParentNode.Attributes["class"] != null)
                        .Where(
                            y =>
                                y.ParentNode.Attributes["class"].Value
                                == "VwiC3b yXK7lf MUxGbd yDYNvb lyLwlc lEBKkf"
                        )
                        .ToList();
                    var counter = 0;
                    foreach (var item in Results)
                    {
                        try
                        {
InstgramResultsList.Add(
                            new GenericGoogleResult()
                            {
                                Type = "Instagram",
                                Header = item.ChildNodes[1].InnerText,
                                Link = item.ChildNodes[2].InnerText
                                    .Replace("instagram.com", "")
                                    .Replace(" > ", "/")
                                    .Replace(" › ", "instagram.com/"),
                                Description = ListOfDesc[counter].InnerText
                            }
                        );
                        }
                        catch (Exception)
                        {

                            continue;
                        }
                        
                        counter++;
                    }

                    await page.CloseAsync();
                }
         
                }
                catch (Exception)
                {

                   
                }
           });

            ////////////////////////////////////////
            List<GenericGoogleResult> twitterResultsList = new();
            var twitterTask = Task.Run(async () =>
            {
                try
                {
 using (var page = await browser.NewPageAsync())
                {
                    await page.GoToAsync(APIs.GoogleEngine_Endpoint);
                    await page.WaitForSelectorAsync("#APjFqb");
                    await page.FocusAsync("#APjFqb");
                    await page.Keyboard.TypeAsync("site:twitter.com " + query);
                    await page.Keyboard.PressAsync(PuppeteerSharp.Input.Key.Enter);
                    await Task.Delay(2000);
                 ///await page.WaitForNavigationAsync();
                    var content1 = await page.GetContentAsync();
                    HtmlDocument htmlDocument1 = new();
                    htmlDocument1.LoadHtml(content1);
                    var Results1 = htmlDocument1.DocumentNode.ChildNodes
                        .Descendants("a")
                            .Where(r => r.ParentNode.ParentNode.Attributes["class"] != null)
                            .Where(y => y.ParentNode.ParentNode.Attributes["class"].Value == "yuRUbf")
                        .ToList();
                    var ListOfDesc = htmlDocument1.DocumentNode.ChildNodes
                        .Descendants("span")
                        .Where(r => r.ParentNode.Attributes["class"] != null)
                        .Where(
                            y =>
                                y.ParentNode.Attributes["class"].Value
                                == "VwiC3b yXK7lf MUxGbd yDYNvb lyLwlc lEBKkf"
                        )
                        .ToList();
                    var counter = 0;
                    foreach (var item in Results1)
                    {
                        try
                        {
       twitterResultsList.Add(
                            new GenericGoogleResult()
                            {
                                Type = "Twitter",
                                Header = item.ChildNodes[1].InnerText,
                                Link = item.ChildNodes[2].InnerText
                                    .Replace("twitter.com", "")
                                    .Replace(" > ", "/")
                                    .Replace(" › ", "twitter.com/"),
                                Description = ListOfDesc[counter].InnerText
                            }
                        );
                        }
                        catch (Exception)
                        {

                            continue;
                        }
                 
                        counter++;
                    }
                }
                }
                catch (Exception)
                {

                   
                }
               
            });
            //////////////////////////////////////////////

            List<GoogleReviewResult> googleReviews = new();

            //GOOGLE PLAY REVIEWS TASK
            var reviewsTask = Task.Run(async () =>
            {
                try { 
                      
                    using (var page = await browser.NewPageAsync())
                {
                    //     await page.SetExtraHttpHeadersAsync(headers);
                    await page.GoToAsync(APIs.AppsReviews_Endpoint + "search?q=" + query);
                    //await page.EvaluateExpressionAsync("window.scrollBy(0, window.innerHeight)")
                    //   await page.ClickAsync("#searchbox-searchbutton");
                    //  await Task.Delay(10000);
                    await page.WaitForXPathAsync(
                        "/html/body/c-wiz[2]/div/div/c-wiz/c-wiz/c-wiz/section/div/div/div"
                    );
                    var content = await page.GetContentAsync();
                    HtmlDocument htmlDocument = new();
                    htmlDocument.LoadHtml(content);
                    var resultsListDiv = htmlDocument.DocumentNode.SelectNodes(
                        "/html/body/c-wiz[2]/div/div/c-wiz/c-wiz/c-wiz/section/div/div/div"
                    );

                    foreach (var item in resultsListDiv.First().ChildNodes)
                    {
                        if (!String.IsNullOrEmpty(item.InnerText))
                        {
                            try
                            {
                                var spans = item.ChildNodes.Descendants("span").ToList();

                                var appName = spans[1].InnerText;
                                var Rate = spans[3].InnerText;
                                var Image = item.ChildNodes.Descendants("img").ToList()[
                                    1
                                ].Attributes["src"].Value;
                                var desc = spans[2].InnerText;
                                try
                                {
                                    var x = double.Parse(desc);
                                    var temp = desc;
                                    desc = Rate;
                                    Rate = temp;

                                    if (desc == "star")
                                    {
                                        desc = appName;
                                        appName = spans[0].InnerText;
                                    }
                                }
                                catch (Exception) { }

                                googleReviews.Add(
                                    new GoogleReviewResult
                                    {
                                        Description = desc,
                                        Title = appName,
                                        Rate = Rate,
                                        Type = GoogleReviewsType.Apps_Games.ToString(),
                                        ImageLink = Image
                                    }
                                );
                            }
                            catch (Exception e)
                            {
                                continue;
                            }
                        }
                    }

                    ////////////////////////////////////////////////////////////////////
                    /////places
                    await page.GoToAsync(APIs.PlacesReviews_Endpoint + query, timeout: 0);
                    await Task.Delay(2000);
                    await page.ClickAsync("#searchbox-searchbutton");

                    await Task.Delay(2000);
                    await page.EvaluateExpressionAsync(
                        "document.getElementsByClassName('TFQHme')[0].parentNode.scrollBy(0,1000)"
                    );
                    await Task.Delay(2000);

                    await page.WaitForXPathAsync(
                        "/html/body/div[3]/div[9]/div[9]/div/div/div[1]/div[2]/div/div[1]/div/div/div[2]/div[1]"
                    );
                    var content1 = await page.GetContentAsync();
                    HtmlDocument htmlDocument1 = new();
                    htmlDocument1.LoadHtml(content1);
                    var resultsListDiv1 = htmlDocument1.DocumentNode.SelectNodes(
                        "/html/body/div[3]/div[9]/div[9]/div/div/div[1]/div[2]/div/div[1]/div/div/div[2]/div[1]"
                    );

                    foreach (var item in resultsListDiv1.First().ChildNodes)
                    {
                        if (!String.IsNullOrEmpty(item.InnerText))
                        {
                            try
                            {
                                var spans = item.ChildNodes.Descendants("span").ToList();
                                var placeName = spans[1].InnerText;
                                var Rate = spans[6].InnerText;
                                var numberofReviews = spans[8].InnerText;
                                var desc = spans[10].InnerText + " - " + spans[14].InnerText;

                                googleReviews.Add(
                                    new GoogleReviewResult
                                    {
                                        Description = desc,
                                        Title = placeName,
                                        Rate = Rate,
                                        Type = GoogleReviewsType.Place.ToString(),
                                        ReviewsNumber = numberofReviews
                                    }
                                );
                            }
                            catch (Exception e)
                            {
                                continue;
                            }
                        }

                        await page.CloseAsync();
                    }
                }
                }
                catch (Exception) { }
            });
           


            //////////////////////////////////////
            var newsList = new List<NewsModel>();
           
            /// GOOGLE NEWS
            var newsTask = Task.Run(async () =>
            { try
            {
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
                                    newsModel.Description = property.InnerText;
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
                        newsList.Add(newsModel);
                    }

                    counter++;
                    if (counter == 30)
                        break;
                }
            }
            catch (Exception)
            {

               
            }
              
            });
            ////////////////////////////////////////////



            List<YoutubeResultModel> youtubeList = new();

            /// YOUTUBE
            var youtubeTask = Task.Run(async () =>
            {
                try
                {
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri(APIs.Youtube_Endpoint);
                    client.DefaultRequestHeaders.Clear();
                    var requestUri =
                        "search?q="
                        + query
                        + "&max_results=10&part=snippet&key="
                        + Keys.Youtube_API_KEY;
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
                                youtubeResult.VideoThumbImage =
                                    "https://img.youtube.com/vi/"
                                    + youtubeResult.VideoId
                                    + "/hqdefault.jpg";
                                youtubeResult.ResultType = YoutubeResultTypes.Video.ToString();

                                var videoRequestUri =
                                    APIs.Youtube_Endpoint
                                    + "videos?key="
                                    + Keys.Youtube_API_KEY
                                    + "&part=snippet,statistics&id="
                                    + item["id"]["videoId"];
                                var responseObj = client
                                    .GetAsync(videoRequestUri)
                                    .GetAwaiter()
                                    .GetResult();
                                var reslt = responseObj.Content
                                    .ReadAsStringAsync()
                                    .GetAwaiter()
                                    .GetResult();
                                var responseJson = JsonConvert.DeserializeObject<
                                    Dictionary<string, dynamic>
                                >(reslt);

                                youtubeResult.PublishedAt = item["snippet"]["publishedAt"];
                                youtubeResult.VideoDescription = item["snippet"]["description"];
                                youtubeResult.VideoTitle = item["snippet"]["title"];
                                youtubeResult.ChannelId = item["snippet"]["channelId"];
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
                }
                catch (Exception)
                {

                    
                }
             
            });

            ////////////////////////

            //Images
            var imagesResult = new GoogleEngineResult();
            imagesResult.Images = new List<string>();

            var imagesTask = Task.Run(async () =>
            {
                using (var page = await browser.NewPageAsync())
                {
                    try
                    {
                        await page.GoToAsync(
                                           APIs.GoogleEngine_Endpoint + "search?q=" + query + "&tbm=isch"
                                       );

                        await page.SetJavaScriptEnabledAsync(true);
                        var content = await page.GetContentAsync();
                        HtmlDocument htmlDocument = new();
                        htmlDocument.LoadHtml(content);

                        var Results = htmlDocument.DocumentNode.ChildNodes[1].ChildNodes[1]
                            //.Where(y => y.Id == "islrg")
                            //.First()
                            .Descendants("img")
                            .ToList();
                        imagesResult.Images = new();

                        var counter = 0;
                        foreach (var item in Results)
                        {
                            try
                            {
                                if (item.Attributes["class"].Value.Contains("rg_i"))
                                {
                                    imagesResult.Images.Add(item.Attributes["src"].Value.ToString());
                                }
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    if (item.Attributes["class"].Value.Contains("rg_i"))
                                    {
                                        googleEngineResult.Images.Add(
                                            item.Attributes["data-src"].Value.ToString()
                                        );
                                    }
                                }
                                catch (Exception) { }
                            }

                            counter++;
                        }

                        imagesResult.AboutDiv = "<h3>Welcome</h3>";

                        //       counter++;
                        //     }


                    }
                    catch (Exception)
                    {

                    
                    }
               




                    await page.CloseAsync();
                }
            });

            /////////////////////
            ///



            //Wikipedia
            /////////////////////////////////////////


            var wikipediaResults = new List<WikipediaResult>();

            var wikipediaTask = Task.Run(async () =>
            {
                try
                {
                    using (var page = await browser.NewPageAsync())
                    {
                        
                        // await page.SetExtraHttpHeadersAsync(headers);
                        
                        await page.GoToAsync(APIs.WikiPedia_Endpoint, timeout: 0);
                        // await page.EvaluateExpressionAsync("");
                        //      await page.WaitForNavigationAsync();
                        await page.SetViewportAsync(
                    new ViewPortOptions() { IsLandscape = true, IsMobile = false, Width = 2000, Height = 3000 }
                );
                        await page.TypeAsync(".cdx-text-input__input", query);
                        await Task.Delay(5000);
                        ///  var resultsList= await page.QuerySelectorAsync(".cdx-menu__listbox");


                        //await page.EvaluateExpressionAsync("window.scrollBy(0, window.innerHeight)")
                        //   await page.ClickAsync("#searchbox-searchbutton");
                        // await Task.Delay(10000);
                        //        await page.WaitForXPathAsync("/html/body/c-wiz[2]/div/div/c-wiz/c-wiz/c-wiz/section/div/div/div");
                        var content = await page.GetContentAsync();
                        HtmlDocument htmlDocument = new();
                        htmlDocument.LoadHtml(content);

                        var resultsListDiv = htmlDocument.DocumentNode
                            .Descendants()
                            .Where(y => y.Id == "cdx-typeahead-search-menu-0")
                            .FirstOrDefault();
                        if (resultsListDiv != null)
                        {
                            foreach (var item in resultsListDiv.ChildNodes.ToList())
                            {
                                if (!String.IsNullOrEmpty(item.InnerText) && item.Name == "li")
                                {
                                    try
                                    {
                                        var ImageLink = "";
                                        try
                                        {
                                            ImageLink = item.ChildNodes[0].ChildNodes[0].ChildNodes[
                                                1
                                            ].Attributes["style"].DeEntitizeValue
                                                .Replace("background-image", "")
                                                .Replace("url(\"", "")
                                                .Replace("\")", "")
                                                .Replace(": //", "")
                                                .Replace(";", "");
                                        }
                                        catch (Exception) { }
                                        var Desc = item.ChildNodes[0].ChildNodes[1].ChildNodes[
                                            3
                                        ].InnerText;
                                        var Title = item.ChildNodes[0].ChildNodes[1].ChildNodes[
                                            0
                                        ].InnerText;

                                        wikipediaResults.Add(
                                            new WikipediaResult()
                                            {
                                                Title = Title,
                                                Description = Desc,
                                                ThumbImg = ImageLink,
                                                ArticleLink =
                                                    "https://en.wikipedia.org/wiki/"
                                                    + Title.Trim().Replace(" ", "_")
                                            }
                                        );
                                    }
                                    catch (Exception e)
                                    {
                                        continue;
                                    }
                                }
                            }
                        }
                        await page.CloseAsync();
                    }
                }
                catch (Exception)
                { }
            });

            Task.WaitAll(
                googleTask,
                wikipediaTask,
                instagramTask,
                twitterTask,
                reviewsTask,
                newsTask,
                imagesTask,
                youtubeTask
            );

            List<SearchResult> searchResultList =
                new()
                {
                    new()
                    {
                        Result = googleEngineResult,
                        Source = SearchResultType.Google_Engine.ToString(),
                        SourceID = (int)SearchResultType.Google_Engine
                    },
                    new()
                    {
                        Result = imagesResult,
                        Source = SearchResultType.Images.ToString(),
                        SourceID = (int)SearchResultType.Images
                    },
                    new()
                    {
                        Result = newsList,
                        Source = SearchResultType.Google_News.ToString(),
                        SourceID = (int)SearchResultType.Google_News
                    },
                    new()
                    {
                        Result = googleReviews,
                        Source = SearchResultType.Google_Reviews.ToString(),
                        SourceID = (int)SearchResultType.Google_Reviews
                    },
                    new()
                    {
                        Result = youtubeList,
                        Source = SearchResultType.Youtube.ToString(),
                        SourceID = (int)SearchResultType.Youtube
                    },
                    new()
                    {
                        Result = wikipediaResults,
                        Source = SearchResultType.Wikipedia.ToString(),
                        SourceID = (int)SearchResultType.Wikipedia
                    },
                    new()
                    {
                        Result = twitterResultsList,
                        Source = SearchResultType.Twitter.ToString(),
                        SourceID = (int)SearchResultType.Twitter
                    },
                    new()
                    {
                        Result = InstgramResultsList,
                        Source = SearchResultType.Instagram.ToString(),
                        SourceID = (int)SearchResultType.Instagram
                    }
                };

            await browser.CloseAsync();
            return View(searchResultList);
        }

        public async Task<IActionResult> Youtube(string query)
        {
            //	var browserFetcher = new BrowserFetcher();

            //	await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            //	string[] args = { "--no-sandbox" };

            //	var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            //	{
            //		Headless = false,
            //		DefaultViewport = null,
            //		Args = args
            //	});

            List<YoutubeResultModel> youtubeList = new();

            /// YOUTUBE
            await Task.Run(async () =>
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(APIs.Youtube_Endpoint);
                client.DefaultRequestHeaders.Clear();
                var requestUri =
                    "search?q="
                    + query
                    + "&max_results=10&part=snippet&key="
                    + Keys.Youtube_API_KEY;
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
                            youtubeResult.VideoThumbImage =
                                "https://img.youtube.com/vi/"
                                + youtubeResult.VideoId
                                + "/hqdefault.jpg";
                            youtubeResult.ResultType = YoutubeResultTypes.Video.ToString();

                            var videoRequestUri =
                                APIs.Youtube_Endpoint
                                + "videos?key="
                                + Keys.Youtube_API_KEY
                                + "&part=snippet,statistics&id="
                                + item["id"]["videoId"];
                            var responseObj = client
                                .GetAsync(videoRequestUri)
                                .GetAwaiter()
                                .GetResult();
                            var reslt = responseObj.Content
                                .ReadAsStringAsync()
                                .GetAwaiter()
                                .GetResult();
                            var responseJson = JsonConvert.DeserializeObject<
                                Dictionary<string, dynamic>
                            >(reslt);

                            youtubeResult.PublishedAt = item["snippet"]["publishedAt"];
                            youtubeResult.VideoDescription = item["snippet"]["description"];
                            youtubeResult.VideoTitle = item["snippet"]["title"];
                            youtubeResult.ChannelId = item["snippet"]["channelId"];
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
            });

            return View(youtubeList);
        }

        public async Task<IActionResult> Images(string query)
        {
            var browserFetcher = new BrowserFetcher();

            await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            string[] args = { "--no-sandbox", "--lang=en-US,en" };

            var browser = await Puppeteer.LaunchAsync(
                new LaunchOptions
                {
                    Headless = true,
                    DefaultViewport = null,
                    Args = args
                }
            );

            GoogleEngineResult googleEngineResult = new();

            googleEngineResult.Query = query;
            try
            {
                //GOOGLE Search TASK General
                //var googleTask =
                await Task.Run(async () =>
                {
                    using (var page = await browser.NewPageAsync())
                    {
                        await page.GoToAsync(
                            APIs.GoogleEngine_Endpoint + "search?q=" + query + "&tbm=isch"
                        );

                        await page.SetJavaScriptEnabledAsync(true);
                        var content = await page.GetContentAsync();
                        HtmlDocument htmlDocument = new();
                        htmlDocument.LoadHtml(content);

                        var Results = htmlDocument.DocumentNode.ChildNodes[1].ChildNodes[1]
                            //.Where(y => y.Id == "islrg")
                            //.First()
                            .Descendants("img")
                            .ToList();
                        googleEngineResult.Images = new();

                        var counter = 0;
                        foreach (var item in Results)
                        {
                            try
                            {
                                if (item.Attributes["class"].Value.Contains("rg_i"))
                                {
                                    googleEngineResult.Images.Add(
                                        item.Attributes["src"].Value.ToString()
                                    );
                                }
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    if (item.Attributes["class"].Value.Contains("rg_i"))
                                    {
                                        googleEngineResult.Images.Add(
                                            item.Attributes["data-src"].Value.ToString()
                                        );
                                    }
                                }
                                catch (Exception) { }
                            }

                            counter++;
                        }

                        googleEngineResult.AboutDiv = "<h3>Welcome</h3>";

                        //       counter++;
                        //     }






                        await page.CloseAsync();
                    }
                });

                //////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////
            }
            catch (Exception e) { }

            var sr = new SearchResult()
            {
                Result = googleEngineResult,
                Source = SearchResultType.Google_Engine.ToString(),
                SourceID = (int)SearchResultType.Google_Engine
            };

            await browser.CloseAsync();
            return View(sr);
        }

        public IActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(string query, string source)
        {
            switch (source)
            {
                case "Youtube":
                    return RedirectToAction("Youtube", new { query = query });

                case "Wikipedia":
                    return RedirectToAction("Wikipedia", new { query = query });

                case "Instagram":
                    return RedirectToAction("Instagram", new { query = query });

                case "Twitter":
                    return RedirectToAction("Twitter", new { query = query });

                case "Google_Reviews":
                    return RedirectToAction("GoogleReviews", new { query = query });

                case "Google_News":
                    return RedirectToAction("GoogleNews", new { query = query });

                case "Images":
                    return RedirectToAction("Images", new { query = query });
                case "Everything":
                    return RedirectToAction("All", new { query = query });

                default:

                    break;
            }

            var browserFetcher = new BrowserFetcher();

            await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            string[] args = { "--no-sandbox", "--lang=en-US,en" };

            var browser = await Puppeteer.LaunchAsync(
                new LaunchOptions
                {
                    Headless = true,
                    DefaultViewport = null,
                    Args = args
                }
            );

            GoogleEngineResult googleEngineResult = new();

            googleEngineResult.Query = query;


            /////////////////////////////////////////////////
            //////////////////////////////////////////////////
            //GOOGLE Search TASK General
            //var googleTask =
            await Task.Run(async () =>
            {
                string[] args = { "--no-sandbox", "--lang=en-US,en" };

                var browser = await Puppeteer.LaunchAsync(
                    new LaunchOptions
                    {
                        Headless = true,
                        DefaultViewport = null,
                        Args = args
                    }
                );

                using (var page = await browser.NewPageAsync())
                {
                    //    await page.SetExtraHttpHeadersAsync(headers);
                    await page.GoToAsync(APIs.GoogleEngine_Endpoint);
                    await page.WaitForSelectorAsync("#APjFqb");
                    await page.FocusAsync("#APjFqb");
                    await page.Keyboard.TypeAsync( query);
                    await page.Keyboard.PressAsync(PuppeteerSharp.Input.Key.Enter);
                    await page.WaitForNavigationAsync();

                    await page.SetJavaScriptEnabledAsync(true);
                    //         await page.EvaluateExpressionAsync("window.scrollBy(0, 2000)");



                    var content = await page.GetContentAsync();
                    HtmlDocument htmlDocument = new();
                    htmlDocument.LoadHtml(content);
                    var Results = htmlDocument.DocumentNode.ChildNodes
                        .Descendants("a")
                             .Where(r => r.ParentNode.ParentNode.Attributes["class"] != null)
                            .Where(y => y.ParentNode.ParentNode.Attributes["class"].Value == "yuRUbf")
                        .ToList();
                    var ListOfDesc = htmlDocument.DocumentNode.ChildNodes
                        .Descendants("span")
                        .Where(r => r.ParentNode.Attributes["class"] != null)
                        .Where(
                            y =>
                                y.ParentNode.Attributes["class"].Value
                                == "VwiC3b yXK7lf MUxGbd yDYNvb lyLwlc lEBKkf"
                        )
                        .ToList();




                    var Results2ndPage=new List<HtmlNode>();
                    var ListOfDesc2ndPage=new List<HtmlNode>();

                    try
                    {
                        await page.ClickAsync("#pnnext");
                        await page.WaitForNavigationAsync();

                         Results2ndPage = htmlDocument.DocumentNode.ChildNodes
               .Descendants("a")
               .Where(r => r.ParentNode.ParentNode.Attributes["class"] != null)
               .Where(y => y.ParentNode.ParentNode.Attributes["class"].Value == "yuRUbf")
               .ToList();

                        ListOfDesc2ndPage = htmlDocument.DocumentNode.ChildNodes
                            .Descendants("span")
                            .Where(r => r.ParentNode.Attributes["class"] != null)
                            .Where(
                                y =>
                                    y.ParentNode.Attributes["class"].Value
                                    == "VwiC3b yXK7lf MUxGbd yDYNvb lyLwlc lEBKkf"
                            )
                            .ToList();

                        Results.AddRange(Results2ndPage);
                        ListOfDesc.AddRange(ListOfDesc);


                    }
                    catch (Exception)
                    {

                  
                    }






                    var counter = 0;




                    foreach (var item in Results)
                    {

                        try
                        {
                            googleEngineResult.genericGoogleResults.Add(
                                new GenericGoogleResult()
                                {
                                    Type = "Google",
                                    Header = item.ChildNodes[1].InnerText,
                                    Link = item.ChildNodes[2].InnerText
                                        .Replace("instagram.com", "")
                                        .Replace(" > ", "/")
                                        .Replace(" › ", "instagram.com/"),
                                    Description = ListOfDesc[counter].InnerText
                                }
                            );

                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        counter++;
                    }

                    await page.CloseAsync();
                }
                await browser.CloseAsync();
            });

            //////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////


            var sr = new SearchResult()
            {
                Result = googleEngineResult,
                Source = SearchResultType.Google_Engine.ToString(),
                SourceID = (int)SearchResultType.Google_Engine
            };

            await browser.CloseAsync();
            return View(sr);
        }

        public async Task<ActionResult> Wikipedia(string query)
        {
            var wikipediaResults = new List<WikipediaResult>();
            var browserFetcher = new BrowserFetcher();

            await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            string[] args = { "--no-sandbox" };

            var browser = await Puppeteer.LaunchAsync(
                new LaunchOptions
                {
                    Headless = true,
                    DefaultViewport = null,
                    Args = args
                }
            );

            //WIKIPEDIA
            await Task.Run(async () =>
            {
                using (var page = await browser.NewPageAsync())
                {
                    // await page.SetExtraHttpHeadersAsync(headers);
                    await page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.181 Safari/537.36")
                ;                     await page.SetJavaScriptEnabledAsync(true);
                    
                    await page.GoToAsync(APIs.WikiPedia_Endpoint,timeout:0);
                    await page.SetViewportAsync(
                               new ViewPortOptions() { IsLandscape = true, IsMobile = false, Width = 2000, Height = 3000 }
                           );

                    /*       await page.SetViewportAsync(
                               new ViewPortOptions() { IsLandscape = true, IsMobile = false }
                           );*/
                    await page.TypeAsync(".cdx-text-input__input", query);
                 
                 ///  var resultsList= await page.QuerySelectorAsync(".cdx-menu__listbox");


                    //await page.EvaluateExpressionAsync("window.scrollBy(0, window.innerHeight)")
                    //   await page.ClickAsync("#searchbox-searchbutton");
                     await Task.Delay(5000);
                    //        await page.WaitForXPathAsync("/html/body/c-wiz[2]/div/div/c-wiz/c-wiz/c-wiz/section/div/div/div");
                    var content = await page.GetContentAsync();
                    HtmlDocument htmlDocument = new();
                    htmlDocument.LoadHtml(content);

                    var resultsListDiv = htmlDocument.DocumentNode
                        .Descendants()
                        .Where(y => y.Id == "cdx-typeahead-search-menu-0")
                        .FirstOrDefault();
                    if (resultsListDiv != null)
                    {
                        foreach (var item in resultsListDiv.ChildNodes.ToList())
                        {
                            if (!String.IsNullOrEmpty(item.InnerText) && item.Name == "li")
                            {
                                try
                                {
                                    var ImageLink = "";
                                    try
                                    {
                                        ImageLink = item.ChildNodes[0].ChildNodes[0].ChildNodes[
                                            1
                                        ].Attributes["style"].DeEntitizeValue
                                            .Replace("background-image", "")
                                            .Replace("url(\"", "")
                                            .Replace("\")", "")
                                            .Replace(": //", "")
                                            .Replace(";", "");
                                    }
                                    catch (Exception) { }
                                    var Desc = item.ChildNodes[0].ChildNodes[1].ChildNodes[
                                        3
                                    ].InnerText;
                                    var Title = item.ChildNodes[0].ChildNodes[1].ChildNodes[
                                        0
                                    ].InnerText;

                                    wikipediaResults.Add(
                                        new WikipediaResult()
                                        {
                                            Title = Title,
                                            Description = Desc,
                                            ThumbImg = ImageLink,
                                            ArticleLink =
                                                "https://en.wikipedia.org/wiki/"
                                                + Title.Trim().Replace(" ", "_")
                                        }
                                    );
                                }
                                catch (Exception e)
                                {
                                    continue;
                                }
                            }
                        }
                    }
                    await page.CloseAsync();
                }
            });

            await browser.CloseAsync();

            return View(wikipediaResults);
        }

        public async Task<ActionResult> Instagram(string query)
        {
            List<GenericGoogleResult> InstgramResultsList = new();
            // INSTAGRAM
            await Task.Run(async () =>
            {
                string[] args = { "--no-sandbox", "--lang=en-US,en" };

                var browser = await Puppeteer.LaunchAsync(
                    new LaunchOptions
                    {
                        Headless = true,
                        DefaultViewport = null,
                        Args = args
                    }
                );

                using (var page = await browser.NewPageAsync())
                {
                    //    await page.SetExtraHttpHeadersAsync(headers);
                    await page.GoToAsync(APIs.GoogleEngine_Endpoint);
                    await page.WaitForSelectorAsync("#APjFqb");
                    await page.FocusAsync("#APjFqb");
                    await page.Keyboard.TypeAsync("site:Instagram.com " + query);
                    await page.Keyboard.PressAsync(PuppeteerSharp.Input.Key.Enter);
                    await page.WaitForNavigationAsync();

                    await page.SetJavaScriptEnabledAsync(true);
                    //         await page.EvaluateExpressionAsync("window.scrollBy(0, 2000)");



                    var content = await page.GetContentAsync();
                    HtmlDocument htmlDocument = new();
                    htmlDocument.LoadHtml(content);
                    var Results = htmlDocument.DocumentNode.ChildNodes
                        .Descendants("a")
                             .Where(r => r.ParentNode.ParentNode.Attributes["class"] != null)
                            .Where(y => y.ParentNode.ParentNode.Attributes["class"].Value == "yuRUbf")
                        .ToList();
                    var ListOfDesc = htmlDocument.DocumentNode.ChildNodes
                        .Descendants("span")
                        .Where(r => r.ParentNode.Attributes["class"] != null)
                        .Where(
                            y =>
                                y.ParentNode.Attributes["class"].Value
                                == "VwiC3b yXK7lf MUxGbd yDYNvb lyLwlc lEBKkf"
                        )
                        .ToList();
                    var counter = 0;
                    foreach (var item in Results)
                    {

                        try
                        {
                            InstgramResultsList.Add(
                                new GenericGoogleResult()
                                {
                                    Type = "Instagram",
                                    Header = item.ChildNodes[1].InnerText,
                                    Link = item.ChildNodes[2].InnerText
                                        .Replace("instagram.com", "")
                                        .Replace(" > ", "/")
                                        .Replace(" › ", "instagram.com/"),
                                    Description = ListOfDesc[counter].InnerText
                                }
                            );

                        }
                        catch (Exception) {
                            continue;
                        }
                        counter++;
                    }

                    await page.CloseAsync();
                }
                await browser.CloseAsync();
            });
            
            return View(InstgramResultsList);
        }

        public async Task<ActionResult> Twitter(string query)
        {
            List<GenericGoogleResult> TwitterResultsList = new();
            await Task.Run(async () =>
            {
                string[] args = { "--no-sandbox", "--lang=en-US,en" };

                var browser = await Puppeteer.LaunchAsync(
                    new LaunchOptions
                    {
                        Headless = true,
                        DefaultViewport = null,
                        Args = args
                    }
                );
                using (var page = await browser.NewPageAsync())
                {
                    await page.GoToAsync(APIs.GoogleEngine_Endpoint);
                    await page.WaitForSelectorAsync("#APjFqb");
                    await page.FocusAsync("#APjFqb");
                    await page.Keyboard.TypeAsync("site:twitter.com " + query);
                    await page.Keyboard.PressAsync(PuppeteerSharp.Input.Key.Enter);
                    await page.WaitForNavigationAsync();
                    var content1 = await page.GetContentAsync();
                    HtmlDocument htmlDocument1 = new();
                    htmlDocument1.LoadHtml(content1);
                    var Results1 = htmlDocument1.DocumentNode.ChildNodes
                        .Descendants("a")
                .Where(r => r.ParentNode.ParentNode.Attributes["class"] != null)
                            .Where(y => y.ParentNode.ParentNode.Attributes["class"].Value == "yuRUbf")
                        .ToList();
                    var ListOfDesc = htmlDocument1.DocumentNode.ChildNodes
                        .Descendants("span")
                        .Where(r => r.ParentNode.Attributes["class"] != null)
                        .Where(
                            y =>
                                y.ParentNode.Attributes["class"].Value
                                == "VwiC3b yXK7lf MUxGbd yDYNvb lyLwlc lEBKkf"
                        )
                        .ToList();
                    var counter = 0;
                    foreach (var item in Results1)
                    {
                        try
                        {
   TwitterResultsList.Add(
                            new GenericGoogleResult()
                            {
                                Type = "Twitter",
                                Header = item.ChildNodes[1].InnerText,
                                Link = item.ChildNodes[2].InnerText
                                    .Replace("twitter.com", "")
                                    .Replace(" > ", "/")
                                    .Replace(" › ", "twitter.com/"),
                                Description = ListOfDesc[counter].InnerText
                            }
                        );
                        }
                        catch (Exception)
                        {

                            continue;
                        }
                     
                        counter++;
                    }
                }
            });

            return View(TwitterResultsList);
        }

        public async Task<ActionResult> GoogleReviews(string query)
        {
            List<GoogleReviewResult> googleReviews = new();
            string[] args = { "--no-sandbox", "--lang=en-US,en" };
            var browser = await Puppeteer.LaunchAsync(
                new LaunchOptions
                {
                    Headless = true,
                    DefaultViewport = null,
                    Args = args
                }
            );
            //GOOGLE PLAY REVIEWS TASK
            await Task.Run(async () =>
            {
                using (var page = await browser.NewPageAsync())
                {
                    //     await page.SetExtraHttpHeadersAsync(headers);
                    await page.GoToAsync(APIs.AppsReviews_Endpoint + "search?q=" + query);
                    //await page.EvaluateExpressionAsync("window.scrollBy(0, window.innerHeight)")
                    //   await page.ClickAsync("#searchbox-searchbutton");
                    //  await Task.Delay(10000);
                    await page.WaitForXPathAsync
                    (
                        "/html/body/c-wiz[2]/div/div/c-wiz/c-wiz/c-wiz/section/div/div/div"
                    );
                    var content = await page.GetContentAsync();
                    HtmlDocument htmlDocument = new();
                    htmlDocument.LoadHtml(content);
                    var resultsListDiv = htmlDocument.DocumentNode.SelectNodes(
                        "/html/body/c-wiz[2]/div/div/c-wiz/c-wiz/c-wiz/section/div/div/div"
                    );

                    foreach (var item in resultsListDiv.First().ChildNodes)
                    {
                        if (!String.IsNullOrEmpty(item.InnerText))
                        {
                            try
                            {
                                var spans = item.ChildNodes.Descendants("span").ToList();

                                var appName = spans[1].InnerText;
                                var Rate = spans[3].InnerText;
                                var Image = item.ChildNodes.Descendants("img").ToList()[
                                    1
                                ].Attributes["src"].Value;
                                var desc = spans[2].InnerText;
                                try
                                {
                                    var x = double.Parse(desc);
                                    var temp = desc;
                                    desc = Rate;
                                    Rate = temp;

                                    if (desc == "star")
                                    {
                                        desc = appName;
                                        appName = spans[0].InnerText;
                                    }
                                }
                                catch (Exception) { }

                                googleReviews.Add(
                                    new GoogleReviewResult
                                    {
                                        Description = desc,
                                        Title = appName,
                                        Rate = Rate,
                                        Type = GoogleReviewsType.Apps_Games.ToString(),
                                        ImageLink = Image
                                    }
                                );
                            }
                            catch (Exception e)
                            {
                                continue;
                            }
                        }
                    }

                    ////////////////////////////////////////////////////////////////////
                    /////places
                    await page.GoToAsync(APIs.PlacesReviews_Endpoint + query);
                    await page.ClickAsync("#searchbox-searchbutton");
                    await Task.Delay(3000);
                    await page.EvaluateExpressionAsync(
                        "document.getElementsByClassName('TFQHme')[0].parentNode.scrollBy(0,1000)"
                    );
                    await Task.Delay(2000);

                    await page.WaitForXPathAsync(
                        "/html/body/div[3]/div[9]/div[9]/div/div/div[1]/div[2]/div/div[1]/div/div/div[2]/div[1]"
                    );
                    var content1 = await page.GetContentAsync();
                    HtmlDocument htmlDocument1 = new();
                    htmlDocument1.LoadHtml(content1);
                    var resultsListDiv1 = htmlDocument1.DocumentNode.SelectNodes(
                        "/html/body/div[3]/div[9]/div[9]/div/div/div[1]/div[2]/div/div[1]/div/div/div[2]/div[1]"
                    );

                    foreach (var item in resultsListDiv1.First().ChildNodes)
                    {
                        if (!String.IsNullOrEmpty(item.InnerText))
                        {
                            try
                            {
                                var spans = item.ChildNodes.Descendants("span").ToList();
                                var placeName = spans[1].InnerText;
                                var Rate = spans[6].InnerText;
                                var numberofReviews = spans[8].InnerText;
                                var desc = spans[10].InnerText + " - " + spans[14].InnerText;

                                googleReviews.Add(
                                    new GoogleReviewResult
                                    {
                                        Description = desc,
                                        Title = placeName,
                                        Rate = Rate,
                                        Type = GoogleReviewsType.Place.ToString(),
                                        ReviewsNumber = numberofReviews
                                    }
                                );
                            }
                            catch (Exception e)
                            {
                                continue;
                            }
                        }

                        await page.CloseAsync();
                    }
                }
            });

            await browser.CloseAsync();
            return View(googleReviews);
        }

        public async Task<ActionResult> GoogleNews(string query)
        {
            var newsList = new List<NewsModel>();

            /// GOOGLE NEWS
            await Task.Run(async () =>
            {
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
                                    newsModel.Description = property.InnerText;
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
                        newsList.Add(newsModel);
                    }

                    counter++;
                    if (counter == 30)
                        break;
                }
            });

            return View(newsList);
        }


        public async Task<ActionResult> BingEngine(string query) 
        {
            var browserFetcher = new BrowserFetcher();

            await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            string[] args = { "--no-sandbox" };

            var browser = await Puppeteer.LaunchAsync(
                new LaunchOptions
                {
                    Headless = true,
                    DefaultViewport = null,
                    Args = args,
                    Timeout = 0
                }
            );

            var googleTask =
                     Task.Run(async () =>
                     {


                         using (var page = await browser.NewPageAsync())
                         {
                             //    await page.SetExtraHttpHeadersAsync(headers);
                             await page.GoToAsync(APIs.BingEngine_Endpoint+"search?q="+query);
                       //      await page.WaitForSelectorAsync("#APjFqb");
                         //    await page.FocusAsync("#APjFqb");
                           //  await page.Keyboard.TypeAsync(query);
                           //  await page.Keyboard.PressAsync(PuppeteerSharp.Input.Key.Enter);
                           //  await page.WaitForNavigationAsync();

                             await page.SetJavaScriptEnabledAsync(true);
                             //         await page.EvaluateExpressionAsync("window.scrollBy(0, 2000)");



                             var content = await page.GetContentAsync();
                             HtmlDocument htmlDocument = new();
                             htmlDocument.LoadHtml(content);
                             var Results = htmlDocument.DocumentNode.ChildNodes.Descendants("ol");

                             var ListOfDesc = htmlDocument.DocumentNode.ChildNodes
                                 .Descendants("span")
                                 .Where(r => r.ParentNode.Attributes["class"] != null)
                                 .Where(
                                     y =>
                                         y.ParentNode.Attributes["class"].Value
                                         == "VwiC3b yXK7lf MUxGbd yDYNvb lyLwlc lEBKkf"
                                 )
                                 .ToList();




                             var Results2ndPage = new List<HtmlNode>();
                             var ListOfDesc2ndPage = new List<HtmlNode>();

                             try
                             {
                                 await page.ClickAsync("#pnnext");
                                 await page.WaitForNavigationAsync();

                                 Results2ndPage = htmlDocument.DocumentNode.ChildNodes
               .Descendants("a")
               .Where(r => r.ParentNode.ParentNode.Attributes["class"] != null)
               .Where(y => y.ParentNode.ParentNode.Attributes["class"].Value == "yuRUbf")
               .ToList();

                                 ListOfDesc2ndPage = htmlDocument.DocumentNode.ChildNodes
                                     .Descendants("span")
                                     .Where(r => r.ParentNode.Attributes["class"] != null)
                                     .Where(
                                         y =>
                                             y.ParentNode.Attributes["class"].Value
                                             == "VwiC3b yXK7lf MUxGbd yDYNvb lyLwlc lEBKkf"
                                     )
                                     .ToList();

                              //   Results.AddRange(Results2ndPage);
                                // ListOfDesc.AddRange(ListOfDesc);


                             }
                             catch (Exception)
                             {


                             }






                             var counter = 0;



                             
                             foreach (var item in Results)
                             {

                                 try
                                 {
/*                                     googleEngineResult.genericGoogleResults.Add(
                                         new GenericGoogleResult()
                                         {
                                             Type = "Google",
                                             Header = item.ChildNodes[1].InnerText,
                                             Link = item.ChildNodes[2].InnerText
                                                 .Replace("instagram.com", "")
                                                 .Replace(" > ", "/")
                                                 .Replace(" › ", "instagram.com/"),
                                             Description = ListOfDesc[counter].InnerText
                                         }
                                     );
                                     */
                                 }
                                 catch (Exception)
                                 {
                                     continue;
                                 }
                                 counter++;
                             }

                             await page.CloseAsync();
                         }
                         await browser.CloseAsync();
                     });



            return Ok();
        }


    }
}
