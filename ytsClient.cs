using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace YtsClient
{
    class Request
    {
        static readonly HttpClient hc = new HttpClient();
        
        // query_term - Used for movie search, matching on: Movie Title/IMDb Code, Actor Name/IMDb Code, Director Name/IMDb Code
        // limit - The limit of results per page that has been set
        // page - Used to see the next page of movies, eg limit=15 and page=2 will show you movies 15-30
        // sort_by - String (title, year, rating, peers, seeds, download_count, like_count, date_added) Sorts the results by choosen value
        public static async Task<JsonResponse> Make(string query_term, int page, int limit, string sort_by = "title")
        {
            HttpResponseMessage response = await hc.GetAsync($"https://yts.lt/api/v2/list_movies.json?query_term={query_term}&page={page}&limit={limit}&sort_by={sort_by}");
            // Throws an exception if the System.Net.Http.HttpResponseMessage.IsSuccessStatusCode property for the HTTP response is false.
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            // Converts the json string to a JsonResponse object
            return JsonConvert.DeserializeObject<JsonResponse>(responseBody);
        }

        public static async Task<JsonResponseMovie> FindMovieById(string id)
        {
            HttpResponseMessage response = await hc.GetAsync($"https://yts.lt/api/v2/movie_details.json?movie_id={id}");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<JsonResponseMovie>(responseBody);
        }
    }

    class JsonResponseMovie
    {
        public string status { get; set; }
        public string status_message { get; set; }
        public MovieData data { get; set; }
    }

    class MovieData
    {
        public Movie movie { get; set; }
    }

    class JsonResponse
    {
        public string status { get; set; }
        public string status_message { get; set; }
        public MovieList data { get; set; }
    }

    class MovieList
    {
        public int movie_count { get; set; }
        public int limit { get; set; }
        public int page_number { get; set; } 
        public List<Movie> movies { get; set; }
    }

    class Movie
    {
        public int id { get; set; }
        public string title_long { get; set; }
        public string description_full { get; set; }
        public string large_cover_image { get; set; }
        public List<Torrent> torrents { get; set; }
    }

    class Torrent {
        public string url { get; set; }
        public string quality { get; set; } 
        public string type { get; set; }
    }
}