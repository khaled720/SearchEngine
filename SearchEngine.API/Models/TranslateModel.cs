namespace SearchEngineWeb.Models
{
    public class TranslateModel
    {

        public string FirstLang { get; set; }
        public string FirstText { get; set; }
        public string SecondLang { get; set; }
        public string SecondText { get; set; }


        public Dictionary<string, string> Languages { get; set; }

    }
}
