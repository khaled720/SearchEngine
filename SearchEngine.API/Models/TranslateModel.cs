namespace SearchEngineWeb.Models
{
    public class TranslateModel
    {

        public string FirstLang  { get; set; }  =  string.Empty;
        public string FirstText  { get; set; }  =  string.Empty;
        public string SecondLang { get; set; } = string.Empty;
        public string SecondText { get; set; } = string.Empty;


        public Dictionary<string, string> Languages { get; set; }

    }
}
