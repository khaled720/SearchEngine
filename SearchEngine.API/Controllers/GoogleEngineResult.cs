using SearchEngine.API.Models;

namespace SearchEngine.API.Controllers
{
    public class GoogleEngineResult
    {

        public List<string> Images { get; set; }


       //key is name 
       //value is link to that filter
        public Dictionary<string, string> Filters { get; set; } =new Dictionary<string, string>();


        public string Description { get; set; }

        //
        Dictionary<string, string> Informations { get; set; }


        public string AboutDiv { get; set; }

        public List<GenericGoogleResult> genericGoogleResults { get; set; } = new List<GenericGoogleResult>();

        public string Query { get; set; } 

    }
}