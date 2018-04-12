using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using YM.Elasticsearch;
using YM.Elasticsearch.Client;
using YM.Elasticsearch.Client.Search;
using YM.Json;

namespace AP.Search.Tests
{
    class Temp
    {
        public async Task Test()
        {
            var http = new HttpClient();
            //string json = await http.GetStringAsync("http://proteus-prd-uno-esdata.associatedpress.com:9200/appl/doc/_search?q=(type:text AND NOT headline:AP* AND NOT headline:BC\\-* AND NOT headline:DIS*)&sort=arrivaldatetime:desc&_source=headline,arrivaldatetime&size=10000");

            string json = File.ReadAllText(@"C:\Users\Yuri Metelkin\Desktop\scroll.json");
            //var headlines = JsonObject.Parse(json)
            //    .Property<JsonObject>("hits")
            //    .Property<JsonArray>("hits")
            //    .Select(e => e.Get<JsonObject>().Property<JsonObject>("_source").Property<string>("headline"))
            //    .Distinct()
            //    .ToArray();

            var suggestions = new Dictionary<string, SuggestData>();

            var es = new ElasticsearchClient("http://proteus-prd-uno-esdata.associatedpress.com:9200");

            var request = new SearchRequest("appl-breaking", "doc", JsonObject.Parse(json));
            var response = await es.ScrollAsync(new ScrollRequest(request));
            while (!response.IsEmpty && suggestions.Count < 10000)
            {
                var docs = new List<ElasticsearchDocument>();

                var sources = response
                    .Hits
                    .Hits
                    .Select(hit => hit.Source)
                    .ToArray();

                foreach (var source in sources)
                {
                    GetClassifiers(source, suggestions);
                    GetHeadlines(source, suggestions);
                }

                response = await es.ScrollAsync(new ScrollRequest(response.ScrollId));
            }

            es = new ElasticsearchClient();

            int from = 0;
            while (from < suggestions.Count)
            {
                var docs = new List<ElasticsearchDocument>();

                foreach (var kvp in suggestions.Skip(from).Take(1000))
                {
                    var doc = new JsonObject()
                        .Add("text", kvp.Value.Text)
                        .Add("suggest", new JsonObject()
                            .Add("input", kvp.Value.Inputs)
                            .Add("weight", kvp.Value.Occurances));

                    docs.Add(new ElasticsearchDocument(kvp.Key, doc, "suggest"));
                }

                await es.BulkIndexDocumentsAsync(docs);

                from += 1000;
            }
        }

        private void GetClassifiers(JsonObject jo, Dictionary<string, SuggestData> suggestions)
        {
            //people
            var ja = jo.Property<JsonArray>("persons");

            if (ja != null)
            {
                foreach (var item in ja.Select(e => e.Get<JsonObject>()))
                {
                    string name = item.Property<string>("name");
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        AddSuggestion(name, suggestions);
                    }

                    var teams = item.Property<JsonArray>("teams");
                    if (teams != null)
                    {
                        foreach (var team in teams.Select(e => e.Get<JsonObject>()))
                        {
                            name = item.Property<string>("name");
                            if (!string.IsNullOrWhiteSpace(name))
                            {
                                AddSuggestion(name, suggestions);
                            }
                        }
                    }
                }
            }

            //organizations
            ja = jo.Property<JsonArray>("organizations");

            if (ja != null)
            {
                foreach (var item in ja.Select(e => e.Get<JsonObject>()))
                {
                    string name = item.Property<string>("name");
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        AddSuggestion(name, suggestions);
                    }
                }
            }

            //places
            ja = jo.Property<JsonArray>("places");

            if (ja != null)
            {
                foreach (var item in ja.Select(e => e.Get<JsonObject>()))
                {
                    string name = item.Property<string>("name");
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        var locationtype = item.Property<JsonObject>("locationtype");
                        if (locationtype != null)
                        {
                            string type = locationtype.Property<string>("name");
                            if (type == "City" || type == "Nation" || type == "State" || type == "Province" || type == "Disputed territory")
                            {
                                AddSuggestion(name, suggestions);
                            }
                        }
                    }
                }
            }
        }

        private void GetHeadlines(JsonObject jo, Dictionary<string, SuggestData> suggestions)
        {
            string original = jo.Property<string>("headline");
            if (!string.IsNullOrWhiteSpace(original))
            {
                foreach (var sentence in original.Replace("`", "").Replace("'", "").Split('.').Where(e => !string.IsNullOrWhiteSpace(e)))
                {
                    foreach (var sub in sentence.Split(';').Where(e => !string.IsNullOrWhiteSpace(e)))
                    {
                        AddSuggestion(sub, suggestions, false);
                    }
                }
            }
        }

        private void AddSuggestion(string text, Dictionary<string, SuggestData> suggestions, bool addSplits = true)
        {
            var suggest = new SuggestData(text);
            if (suggestions.ContainsKey(suggest.Id))
            {
                suggest = suggestions[suggest.Id];
            }

            suggest.AddInput(text, true);

            if (addSplits)
            {
                var tokens = text.Split(' ').Where(e => !string.IsNullOrWhiteSpace(e)).ToArray();
                if (tokens.Length > 1)
                {
                    for (int i = 1; i < tokens.Length; i++)
                    {
                        suggest.AddInput(tokens[i], false);
                    }
                }
            }

            suggestions[suggest.Id] = suggest;
        }
    }

    class SuggestData
    {
        public string Id { get; private set; }
        public string Text { get; private set; }
        public string[] Inputs { get; private set; }
        public int Occurances { get; private set; }

        public SuggestData(string text)
        {
            using (MD5 md5 = MD5.Create())
            {
                var sb = new StringBuilder();
                var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(text));
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append(bytes[i].ToString("x2"));
                }
                Id = sb.ToString();
            }

            Text = text;
        }

        public SuggestData AddInput(string input, bool addOccurance = false)
        {
            if (Inputs == null)
            {
                Inputs = new string[] { input };
            }
            else if (!Inputs.Contains(input))
            {
                int size = Inputs.Length;
                var @new = new string[size + 1];
                Array.Copy(Inputs, @new, size);
                @new[size] = input;
                Inputs = @new;
            }

            if (addOccurance)
            {
                AddOccurance();
            }

            return this;
        }

        public SuggestData AddOccurance()
        {
            Occurances += 1;
            return this;
        }
    }
}
