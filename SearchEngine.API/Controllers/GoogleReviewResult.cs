namespace SearchEngine.API.Controllers
{
	public class GoogleReviewResult
	{
		public string Title { get; set; }
		public string Rate { get; set; }

		public string Description { get; set; }

		public string ReviewsNumber { get; set; }
		//place app 
		public string Type { get; set; }
		public string ImageLink { get; internal set; }
	}
}