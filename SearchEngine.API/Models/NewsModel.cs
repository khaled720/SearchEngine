namespace SearchEngineWeb.Models
{
    public class NewsModel
    {
        public string Title { get; set; }

        public string Guid { get; set; }

        public string PubDate { get; set; }

        public string Description { get; set; }

        public string DescriptionArticleLink { get; set; }

        public string Source { get; set; }
        public string SourceLink { get; set; }
    }
}