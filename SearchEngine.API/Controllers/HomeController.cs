using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PuppeteerSharp;
using SearchEngine.API.Models;

namespace SearchEngine.API.Controllers
{
	public class HomeController : Controller
	{

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
			await Task.Run(async () => {
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


			});

			return View(youtubeList);
		}





		public IActionResult Search()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Search(string query)
		{



			var browserFetcher = new BrowserFetcher();

			await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
			string[] args = { "--no-sandbox" };

			var browser = await Puppeteer.LaunchAsync(new LaunchOptions
			{
				Headless = false,
				DefaultViewport = null,
				Args = args
			});
			List<WikipediaResult> wikipediaResults = new();
			List<GoogleReviews> googleReviews = new();
			List<GenericGoogleResult> InstgramResultsList = new();
			GoogleEngineResult googleEngineResult = new();
			List<YoutubeResultModel> youtubeList = new();
			List<NewsModel> NewsItems = new();
			googleEngineResult.Query = query;
			try
			{


				Dictionary<string, string> headers = new Dictionary<string, string>();
				headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.131 Safari/537.36");
				headers.Add("accept", "text/html,application/xhtml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3");
				////////////////////////////////////////////////////////
				/////////////////////////////////////////////////
				//////////////////////////////////////////////////
				//GOOGLE Search TASK General
				var googleTask = Task.Run(async () =>
				{




					using (var page = await browser.NewPageAsync())
					{

						await page.SetExtraHttpHeadersAsync(headers);
						await page.GoToAsync(APIs.GoogleEngine_Endpoint + "search?q=" + query);
						//       await page.WaitForSelectorAsync("#APjFqb");
						//      await page.FocusAsync("#APjFqb");
						//      await page.Keyboard.TypeAsync(query);
						//     await page.Keyboard.PressAsync(PuppeteerSharp.Input.Key.Enter);
						//     await page.WaitForNavigationAsync();

						await page.SetJavaScriptEnabledAsync(true);
						//         await page.EvaluateExpressionAsync("window.scrollBy(0, 2000)");



						var content = await page.GetContentAsync();
						HtmlDocument htmlDocument = new();
						htmlDocument.LoadHtml(content);

						var filters = htmlDocument.DocumentNode.SelectSingleNode("//*[@id=\"cnt\"]/div[5]/div/div/div[1]/div[1]/div");

						var filtersNames = new List<string>();

						var Results = htmlDocument.DocumentNode.ChildNodes.Descendants("a").Where(r => r.ParentNode.Attributes["class"] != null)
						.Where(y => y.ParentNode.Attributes["class"].Value == "yuRUbf").ToList();
						foreach (var result in filters.ChildNodes)
						{
							try
							{
								if (result.Name == "a")
								{
									var link = result.Attributes["href"].Value;
									googleEngineResult.Filters.Add(result.InnerText, link);

								}
								else if (result.Name == "div")
								{

									var link = result.Descendants("a").ToList()[0].Attributes["href"].Value;
									googleEngineResult.Filters.Add(result.InnerText, link);
								}
							}
							catch (Exception)
							{


							}


						}

						var ListOfDesc = htmlDocument.DocumentNode.ChildNodes.Descendants("span")
						.Where(r => r.ParentNode.Attributes["class"] != null).Where(y => y.ParentNode
						.Attributes["class"].Value == "VwiC3b yXK7lf MUxGbd yDYNvb lyLwlc lEBKkf").ToList();

						//   var wikiBox = htmlDocument.DocumentNode.SelectSingleNode("//*[@id=\"rhs\"]/div/div/div[2]");

						var counter = 0;
						foreach (var item in Results)
						{
							googleEngineResult.genericGoogleResults.Add(new GenericGoogleResult()
							{
								Type = "Google",
								Header = item.ChildNodes[1].InnerText,
								Link = item.ChildNodes[2].InnerText.Replace("instagram.com", "").Replace(" > ", "/").Replace(" › ", "instagram.com/"),
								Description = ListOfDesc[counter].InnerText
							});
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

				/*
								////////////////////////////////////////

								// GOOGLE PLACES REVIEWS TASK
								var googlePlaceTask= Task.Run(async () => {

								using (var page = await browser.NewPageAsync())
								{

									await page.SetExtraHttpHeadersAsync(headers);
									await page.GoToAsync(APIs.PlacesReviews_Endpoint + query);
									//await page.EvaluateExpressionAsync("window.scrollBy(0, window.innerHeight)")
									await page.ClickAsync("#searchbox-searchbutton");
									await Task.Delay(10000);
									await page.WaitForXPathAsync("/html/body/div[3]/div[9]/div[9]/div/div/div[1]/div[2]/div/div[1]/div/div/div[2]/div[1]");
									var content = await page.GetContentAsync();
									HtmlDocument htmlDocument = new();
									htmlDocument.LoadHtml(content);
									var resultsListDiv = htmlDocument.DocumentNode.SelectNodes("/html/body/div[3]/div[9]/div[9]/div/div/div[1]/div[2]/div/div[1]/div/div/div[2]/div[1]");

									foreach (var item in resultsListDiv.First().ChildNodes)
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

												googleReviews.Add(new GoogleReviews { Description = desc, Title = placeName, Rate = Rate, Type = GoogleReviewsType.Place.ToString(), ReviewsNumber = numberofReviews });

											}
											catch (Exception e)
											{
												continue;

											}

										}


									}
									await page.CloseAsync();


								}


							});

								//GOOGLE PLAY REVIEWS TASK
								var googleplayTask =   Task.Run(async () => {

							 using (var page = await browser.NewPageAsync())
							 {

								 await page.SetExtraHttpHeadersAsync(headers);
								 await page.GoToAsync(APIs.AppsReviews_Endpoint + "search?q=" + query);
								 //await page.EvaluateExpressionAsync("window.scrollBy(0, window.innerHeight)")
								 //   await page.ClickAsync("#searchbox-searchbutton");
								 await Task.Delay(10000);
								 await page.WaitForXPathAsync("/html/body/c-wiz[2]/div/div/c-wiz/c-wiz/c-wiz/section/div/div/div");
								 var content = await page.GetContentAsync();
								 HtmlDocument htmlDocument = new();
								 htmlDocument.LoadHtml(content);
								 var resultsListDiv = htmlDocument.DocumentNode.SelectNodes("/html/body/c-wiz[2]/div/div/c-wiz/c-wiz/c-wiz/section/div/div/div");

								 foreach (var item in resultsListDiv.First().ChildNodes)
								 {
									 if (!String.IsNullOrEmpty(item.InnerText))
									 {
										 try
										 {
											 var spans = item.ChildNodes.Descendants("span").ToList();




											 var appName = spans[1].InnerText;
											 var Rate = spans[3].InnerText;
											 var Image = item.ChildNodes.Descendants("img").ToList()[1].Attributes["src"].Value;
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
											 catch (Exception)
											 {

											 }


											 googleReviews.Add(new GoogleReviews { Description = desc, Title = appName, Rate = Rate, Type = GoogleReviewsType.Apps_Games.ToString(), ImageLink = Image });

										 }
										 catch (Exception e)
										 {
											 continue;

										 }

									 }


								 }
								 await page.CloseAsync();


							 }


						 });

								// INSTAGRAM 
								var instagramTask = Task.Run(async () => {




							 using (var page = await browser.NewPageAsync())
							 {

								 await page.SetExtraHttpHeadersAsync(headers);
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
								 var Results = htmlDocument.DocumentNode.ChildNodes.Descendants("a").Where(r => r.ParentNode.Attributes["class"] != null).Where(y => y.ParentNode.Attributes["class"].Value == "yuRUbf").ToList();
										var ListOfDesc = htmlDocument.DocumentNode.ChildNodes.Descendants("span").Where(r => r.ParentNode.Attributes["class"] != null).Where(y => y.ParentNode.Attributes["class"].Value == "VwiC3b yXK7lf MUxGbd yDYNvb lyLwlc lEBKkf").ToList();
										var counter = 0;
										foreach (var item in Results)
								 {
											InstgramResultsList.Add(new GenericGoogleResult()
											{
												Type = "Instagram",
												Header = item.ChildNodes[1].InnerText,
												Link = item.ChildNodes[2].InnerText.Replace("instagram.com", "").Replace(" > ", "/").Replace(" › ", "instagram.com/"),
												Description = ListOfDesc[counter].InnerText
											}) ;
											counter++;
								 }






								 await page.CloseAsync();


							 }



						 });
							  var twitterTask=  Task.Run(async() =>
							  {


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
								  var Results1 = htmlDocument1.DocumentNode.ChildNodes.Descendants("a").Where(r => r.ParentNode.Attributes["class"] != null).Where(y => y.ParentNode.Attributes["class"].Value == "yuRUbf").ToList();
								  var ListOfDesc = htmlDocument1.DocumentNode.ChildNodes.Descendants("span").Where(r => r.ParentNode.Attributes["class"] != null).Where(y => y.ParentNode.Attributes["class"].Value == "VwiC3b yXK7lf MUxGbd yDYNvb lyLwlc lEBKkf").ToList();
									  var counter = 0;
									  foreach (var item in Results1)
								  {
										  InstgramResultsList.Add(new GenericGoogleResult()
										  {
											  Type = "Twitter",
											  Header = item.ChildNodes[1].InnerText,
											  Link = item.ChildNodes[2].InnerText.Replace("twitter.com", "").Replace(" > ", "/").Replace(" › ", "twitter.com/"),
											  Description = ListOfDesc[counter].InnerText

										  }) ;
										  counter++;
									  }

							  }

								});

								//WIKIPEDIA
								var wikipediaTask = Task.Run(async () => {

							   using (var page = await browser.NewPageAsync())
							   {

								   await page.SetExtraHttpHeadersAsync(headers);
								   await page.GoToAsync(APIs.WikiPedia_Endpoint);
								   await page.SetViewportAsync(new ViewPortOptions() { IsLandscape = true, IsMobile = false });
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

								   var resultsListDiv = htmlDocument.DocumentNode.Descendants()
										.Where(y => y.Id == "cdx-typeahead-search-menu-0").FirstOrDefault();
									 if (resultsListDiv!=null) {
										 foreach (var item in resultsListDiv.ChildNodes.ToList())
										 {
											 if (!String.IsNullOrEmpty(item.InnerText) && item.Name == "li")
											 {

												 try
												 {

													 var ImageLink = "";
													 try
													 {
														 ImageLink = item.ChildNodes[0].ChildNodes[0].ChildNodes[1].Attributes["style"].DeEntitizeValue.Replace("background-image", "").Replace("url(\"", "").Replace("\")", "").Replace(": //", "").Replace(";", "");

													 }
													 catch (Exception)
													 {


													 }
													 var Desc = item.ChildNodes[0].ChildNodes[1].ChildNodes[3].InnerText;
													 var Title = item.ChildNodes[0].ChildNodes[1].ChildNodes[0].InnerText;


													 wikipediaResults.Add(new WikipediaResult()
													 {
														 Title = Title,
														 Description = Desc,
														 ThumbImg = ImageLink,
														 ArticleLink = "https://en.wikipedia.org/wiki/" + Title.Trim().Replace(" ", "_")
													 });




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

								/// YOUTUBE
								var youtubeTask = Task.Run(async () => {
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


					 });

								/// GOOGLE NEWS
								var googleNewsTask = Task.Run(async () => {


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

							});
								*/


				Task.WaitAll(googleTask); // googlePlaceTask, googleplayTask, twitterTask,instagramTask, wikipediaTask, youtubeTask, googleNewsTask);

			}
			catch (Exception)
			{


			}








			List<SearchResult> searchResults = new();


			//   searchResults.Add(new SearchResult() { Result = NewsItems, Source = SearchResultType.Google_News.ToString(), SourceID = (int)SearchResultType.Google_News });

			//   searchResults.Add(new SearchResult() { Result = wikipediaResults, Source=SearchResultType.Wikipedia.ToString(), SourceID =(int) SearchResultType.Wikipedia });
			//     searchResults.Add(new SearchResult() { Result = InstgramResultsList, Source = SearchResultType.Instagram_Twitter.ToString(), SourceID = (int)SearchResultType.Instagram_Twitter });
			//      searchResults.Add(new SearchResult() { Result = youtubeList, Source = SearchResultType.Youtube.ToString(), SourceID = (int)SearchResultType.Youtube });
			//  searchResults.Add(new SearchResult() {Result=googleReviews,Source=SearchResultType.Google_Reviews.ToString() ,SourceID=(int)SearchResultType.Google_Reviews});
			searchResults.Add(new SearchResult() { Result = googleEngineResult, Source = SearchResultType.Google_Engine.ToString(), SourceID = (int)SearchResultType.Google_Engine });


			await browser.CloseAsync();
			return View(searchResults);
		}



















		// GET: HomeController
		//public ActionResult Search()
		//{
		//    return View();
		//}
		//[HttpPost]
		//public ActionResult Search(string query)
		//{


		//    ///

		//    HttpClient client = new HttpClient();
		//    client.BaseAddress = new Uri(APIs.Youtube_Endpoint);
		//    client.DefaultRequestHeaders.Clear();
		//    var requestUri = "search?q=" + query + "&max_results=10&part=snippet&key=" + Keys.Youtube_API_KEY;
		//    var res = client.GetAsync(requestUri).GetAwaiter().GetResult();
		//    var result = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
		//    var re = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(result);
		//    var finalresult = re["items"];
		//    List<YoutubeResultModel> youtubeList = new();
		//    foreach (var item in finalresult)
		//    {
		//        var youtubeResult = new YoutubeResultModel();

		//        string kind = item["id"]["kind"];

		//        switch (kind.ToLower().Replace("youtube#", ""))
		//        {
		//            case "channel":
		//                youtubeResult.ChannelId = item["id"]["channelId"];
		//                youtubeResult.ChannelTitle = item["snippet"]["title"];

		//                youtubeResult.PublishedAt = item["snippet"]["publishedAt"];
		//                youtubeResult.ChannelDescription = item["snippet"]["description"];

		//                youtubeResult.ResultType = YoutubeResultTypes.Channel.ToString();
		//                break;

		//            case "video":
		//                youtubeResult.VideoId = item["id"]["videoId"];
		//                youtubeResult.ResultType = YoutubeResultTypes.Video.ToString();
		//                var videoRequestUri =
		//                    APIs.Youtube_Endpoint
		//                    + "videos?key="
		//                    + Keys.Youtube_API_KEY
		//                    + "&part=snippet,statistics&id="
		//                    + item["id"]["videoId"];
		//                var responseObj = client.GetAsync(videoRequestUri).GetAwaiter().GetResult();
		//                var reslt = responseObj.Content
		//                    .ReadAsStringAsync()
		//                    .GetAwaiter()
		//                    .GetResult();
		//                var responseJson = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(reslt);
		//                youtubeResult.PublishedAt = responseJson["items"][0]["snippet"]["publishedAt"];
		//                youtubeResult.VideoDescription = responseJson["items"][0]["snippet"][
		//                    "description"
		//                ];
		//                youtubeResult.VideoTitle = responseJson["items"][0]["snippet"]["title"];
		//                youtubeResult.ChannelId = responseJson["items"][0]["snippet"]["channelId"];
		//                youtubeResult.ChannelTitle = responseJson["items"][0]["snippet"][
		//                    "channelTitle"
		//                ];

		//                youtubeResult.VideoViews = responseJson["items"][0]["statistics"][
		//                    "viewCount"
		//                ];
		//                youtubeResult.VideoLikes = responseJson["items"][0]["statistics"][
		//                    "likeCount"
		//                ];
		//                youtubeResult.VideoComments = responseJson["items"][0]["statistics"][
		//                    "commentCount"
		//                ];

		//                break;

		//            case "playlist":
		//                youtubeResult.PlaylistId = item["id"]["playlistId"];

		//                youtubeResult.PlaylistTitle = item["snippet"]["title"];
		//                youtubeResult.ChannelId = item["snippet"]["channelId"];

		//                youtubeResult.PublishedAt = item["snippet"]["publishedAt"];
		//                youtubeResult.PlaylistDesceription = item["snippet"]["description"];
		//                youtubeResult.ResultType = YoutubeResultTypes.Playlist.ToString();
		//                break;

		//            default:
		//                break;
		//        }

		//        youtubeList.Add(youtubeResult);
		//    }

		//    // google news
		//    HttpClient googleNewsClient = new HttpClient();

		//    googleNewsClient.BaseAddress = new Uri(APIs.GoogleNews_Endpoint);
		//    var requestUri1 = "?q=" + query;
		//    var result1 = googleNewsClient.GetAsync(requestUri1).GetAwaiter().GetResult();
		//    var res1 = result1.Content.ReadAsStringAsync().GetAwaiter().GetResult();

		//    XmlDocument xmlDocument = new XmlDocument();
		//    xmlDocument.LoadXml(res1);

		//    var channel = xmlDocument.ChildNodes[1].ChildNodes[0];
		//    List<NewsModel> NewsItems = new();
		//    int counter = 0;
		//    foreach (XmlNode item in channel.ChildNodes)
		//    {
		//        if (item.Name == "item")
		//        {
		//            NewsModel newsModel = new NewsModel();

		//            foreach (XmlNode property in item.ChildNodes)
		//            {
		//                switch (property.Name)
		//                {
		//                    case "title":
		//                        newsModel.Title = property.InnerText;

		//                        break;

		//                    case "description":
		//                        //               newsModel.Description = property.InnerText;
		//                        break;

		//                    case "link":
		//                        newsModel.DescriptionArticleLink = property.InnerText;
		//                        break;

		//                    case "source":
		//                        newsModel.Source = property.InnerText;
		//                        newsModel.SourceLink = property.Attributes[0].Value;

		//                        break;

		//                    case "guid":
		//                        newsModel.Guid = property.InnerText;
		//                        break;
		//                    case "pubDate":

		//                        newsModel.PubDate = property.InnerText;
		//                        break;

		//                    default:
		//                        break;
		//                }
		//            }
		//            NewsItems.Add(newsModel);
		//        }

		//        counter++;
		//        if (counter == 11)
		//            break;
		//    }

		//    //////Twitter
		//    //HttpClient Twitterclient = new HttpClient();
		//    //Twitterclient.BaseAddress = new Uri(APIs.Twitter_Endpoint);
		//    //Twitterclient.DefaultRequestHeaders.Clear();
		//    //Twitterclient.DefaultRequestHeaders.Add(
		//    //    "Authorization",
		//    //    "Bearer " + Keys.Twitter_BearerToken
		//    //);
		//    //var qq = query.Replace("@", "from:");
		//    //var requestUri3 =
		//    //    "tweets/search/recent?query="
		//    //    + qq
		//    //    + "&max_results=10&tweet.fields=created_at&user.fields=description&expansions=author_id";
		//    //var res3 = Twitterclient.GetAsync(requestUri3).GetAwaiter().GetResult();
		//    //var result3 = res3.Content.ReadAsStringAsync().GetAwaiter().GetResult();
		//    //////   var result3 = await res.Content.ReadAsStringAsync();
		//    //var respons = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(result3);
		//    //List<TwitteModel> TwittesList = new();
		//    //foreach (var item in respons["data"])
		//    //{
		//    //    var Twitte = new TwitteModel();
		//    //    Twitte.Text = item["text"];
		//    //    Twitte.AuthorId = item["author_id"];
		//    //    Twitte.CreatedAt = item["created_at"];
		//    //    Twitte.Id = item["id"];

		//    //    foreach (var user in respons["includes"]["users"])
		//    //    {
		//    //        if (user["id"] == item["author_id"])
		//    //        {
		//    //            Twitte.TwitteUser = new();
		//    //            Twitte.TwitteUser.Id = user["id"];
		//    //            Twitte.TwitteUser.Description = user["description"];
		//    //            Twitte.TwitteUser.Name = user["name"];
		//    //            Twitte.TwitteUser.Username = user["username"];
		//    //        }
		//    //    }

		//    //    TwittesList.Add(Twitte);
		//    //}

		//    SearchResult searchResult = new SearchResult();
		//    searchResult.Source = SearchResultType.Google_News.ToString();
		//    searchResult.SourceID = (int)SearchResultType.Google_News;
		//    searchResult.Result = NewsItems;
		//    SearchResult searchResult2 = new SearchResult();
		//    searchResult2.Source = SearchResultType.Youtube.ToString();
		//    searchResult2.SourceID = (int)SearchResultType.Youtube;
		//    searchResult2.Result = youtubeList;

		//    //SearchResult searchResult3 = new SearchResult();
		//    //searchResult3.Source = SearchResultType.Twitter.ToString();
		//    //searchResult3.SourceID = (int)SearchResultType.Twitter;
		//    //searchResult3.Result = TwittesList;

		//    List<SearchResult> searchList = new();
		//    searchList.Add(searchResult);
		//    searchList.Add(searchResult2);
		//    //searchList.Add(searchResult3);


		//    ///


		//    //   HttpClient client1 = new();

		//    //var resss=   client1.GetAsync("https://www.instagram.com/apple/").GetAwaiter().GetResult();
		//    //   var con = resss.Content.ReadAsStringAsync().GetAwaiter().GetResult();

		//    //   HtmlDocument htmlDocument = new HtmlDocument();

		//    //       htmlDocument.LoadHtml(con);




		//    return View(searchList);
		//}



	}
}
