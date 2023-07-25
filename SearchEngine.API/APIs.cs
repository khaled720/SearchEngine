namespace SearchEngine.API
{
    public class APIs
    {

        // q type 
        public static string Youtube_Endpoint { get;  } = "https://www.googleapis.com/youtube/v3/";

        //q
        public static string GoogleNews_Endpoint { get; } = "https://www.news.google.com/rss/search";

        //max_results query
        public static string Twitter_Endpoint { get; } = "https://api.twitter.com/2/";

        public static string PlacesReviews_Endpoint { get; } = "https://www.google.com/maps/place/";
        public static string AppsReviews_Endpoint { get; } = "https://play.google.com/store/";

        public static string WikiPedia_Endpoint { get; } = "https://en.wikipedia.org/wiki/Main_Page";

        public static string  GoogleEngine_Endpoint { get; } = "https://www.google.com/";

        public static string  GoogleTranslate_Endpoint { get; } = " https://translate.google.com";

       


    }
}
