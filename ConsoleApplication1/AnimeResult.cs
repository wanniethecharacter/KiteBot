using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace KiteBot
{
    public enum RequestHttpMethod
    {
        Get,
        Post
    }

    public static class SearchHelper
    {
        private static DateTime lastRefreshed = DateTime.MinValue;
        private static string token { get; set; } = "";

        public static async Task<Stream> GetResponseStreamAsync(string url,
            IEnumerable<KeyValuePair<string, string>> headers = null, RequestHttpMethod method = RequestHttpMethod.Get)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));
            var httpClient = new HttpClient();
            switch (method)
            {
                case RequestHttpMethod.Get:
                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }
                    return await httpClient.GetStreamAsync(url);
                case RequestHttpMethod.Post:
                    FormUrlEncodedContent formContent = null;
                    if (headers != null)
                    {
                        formContent = new FormUrlEncodedContent(headers);
                    }
                    var message = await httpClient.PostAsync(url, formContent);
                    return await message.Content.ReadAsStreamAsync();
                default:
                    throw new NotImplementedException("That type of request is unsupported.");
            }
        }

        public static async Task<string> GetResponseStringAsync(string url,
            IEnumerable<KeyValuePair<string, string>> headers = null,
            RequestHttpMethod method = RequestHttpMethod.Get)
        {

            using (var streamReader = new StreamReader(await GetResponseStreamAsync(url, headers, method)))
            {
                return await streamReader.ReadToEndAsync();
            }
        }

        public static async Task<AnimeResult> GetAnimeData(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentNullException(nameof(query));

            await RefreshAnilistToken();

            //var link = "http://anilist.co/api/anime/search/" + Uri.EscapeUriString(query);
            var smallContent = "";
            var cl = new RestSharp.RestClient("http://anilist.co/api");
            var rq = new RestSharp.RestRequest("/anime/search/" + Uri.EscapeUriString(query));
            rq.AddParameter("access_token", token);
            smallContent = cl.Execute(rq).Content;
            var smallObj = JArray.Parse(smallContent)[0];

            rq = new RestSharp.RestRequest("/anime/" + smallObj["id"]);
            rq.AddParameter("access_token", token);
            var content = cl.Execute(rq).Content;

            return await Task.Run(() => JsonConvert.DeserializeObject<AnimeResult>(content));
        }

        public static async Task<MangaResult> GetMangaData(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentNullException(nameof(query));

            await RefreshAnilistToken();

            //var link = "http://anilist.co/api/anime/search/" + Uri.EscapeUriString(query);
            var smallContent = "";
            var cl = new RestSharp.RestClient("http://anilist.co/api");
            var rq = new RestSharp.RestRequest("/manga/search/" + Uri.EscapeUriString(query));
            rq.AddParameter("access_token", token);
            smallContent = cl.Execute(rq).Content;
            var smallObj = JArray.Parse(smallContent)[0];

            rq = new RestSharp.RestRequest("/manga/" + smallObj["id"]);
            rq.AddParameter("access_token", token);
            var content = cl.Execute(rq).Content;

            return await Task.Run(() => JsonConvert.DeserializeObject<MangaResult>(content));
        }

        private static async Task RefreshAnilistToken()
        {
            if (DateTime.Now - lastRefreshed > TimeSpan.FromMinutes(29))
                lastRefreshed = DateTime.Now;
            else
            {
                return;
            }
            var headers = new Dictionary<string, string> {
                {"grant_type", "client_credentials"},
                {"client_id", "kwoth-w0ki9"},
                {"client_secret", "Qd6j4FIAi1ZK6Pc7N7V4Z"},
            };
            var content =
                await GetResponseStringAsync("http://anilist.co/api/auth/access_token", headers, RequestHttpMethod.Post);

            token = JObject.Parse(content)["access_token"].ToString();
        }
    }

    public class AnimeResult
    {
        public int id;
        public string airing_status;
        public string title_english;
        public int total_episodes;
        public string description;
        public string image_url_lge;

        public override string ToString() =>
            "`Title:` **" + title_english +
            "**\n`Status:` " + airing_status +
            "\n`Episodes:` " + total_episodes +
            "\n`Link:` http://anilist.co/anime/" + id +
            "\n`Synopsis:` " + description.Substring(0, description.Length > 500 ? 500 : description.Length) + "..." +
            "\n`img:` " + image_url_lge;
    }

    public class MangaResult
    {
        public int id;
        public string publishing_status;
        public string image_url_lge;
        public string title_english;
        public int total_chapters;
        public int total_volumes;
        public string description;

        public override string ToString() =>
            "`Title:` **" + title_english +
            "**\n`Status:` " + publishing_status +
            "\n`Chapters:` " + total_chapters +
            "\n`Volumes:` " + total_volumes +
            "\n`Link:` http://anilist.co/manga/" + id +
            "\n`Synopsis:` " + description.Substring(0, description.Length > 500 ? 500 : description.Length) + "..." +
            "\n`img:` " + image_url_lge;
    }
}
