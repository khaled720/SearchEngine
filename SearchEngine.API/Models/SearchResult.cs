namespace SearchEngine.API.Models
{
    public class SearchResult
    {

      

        public string Source { get; set; }

    public int SourceID { get; internal set; }
        public dynamic  Result { get; set; }
    
    }


    public enum SearchResultType {
      
     Google_Engine
    ,Youtube,
    Google_News,
 //   Tiktok,
    Instagram,
    Twitter,
     Google_Reviews,
     Wikipedia,
     Images
 //    Instagram_Twitter
		
   
	}
    public enum GoogleReviewsType
    {

      Apps_Games,
        Place

    }

   

    public class WikipediaResult 
    {

        public string Title { get; set; }
        public string Description { get; set; }

        public string ArticleLink { get; set; }
        public string ThumbImg { get; internal set; }
    }


    public class GenericGoogleResult
    {


        public string Header { get; set; }
        public string Link { get; set; }
        public string Type { get; set; }
        public string Description { get; internal set; }
        //public string Name { get; set; }
        //public string Posts { get; set; }

        //public string Followers { get; set; }

        //public string Following { get; set; }
        ////place app 
        //public List<string> RecentImages { get; set; }
        //public string Username { get; internal set; }
    }



}
