using System.Collections.Generic;
using System.Linq;
using YM.Json;

namespace YM.Elasticsearch.Client.Indices
{
    public class AliasRequest : RestRequest
    {
        private readonly List<AliasAction> _actions = new List<AliasAction>();

        public AliasAction[] Actions
        {
            get
            {
                return _actions.ToArray();
            }
        }

        public bool IsEmpty
        {
            get
            {
                return _actions.Count == 0;
            }
        }

        public AliasRequest Add(string index, string alias)
        {
            _actions.Add(new AliasAction("add", index, alias));
            return this;
        }

        public AliasRequest Remove(string index, string alias)
        {
            _actions.Add(new AliasAction("remove", index, alias));
            return this;
        }

        public override JsonObject ToJson()
        {
            return new JsonObject()
                .Add("actions", new JsonArray(_actions.Select(e => e.ToJson())));
        }

        public override string GetUrl(string clusterUrl)
        {
            return clusterUrl + "/_aliases";
        }

        public override string GetBody()
        {
            return ToJson().ToString();
        }
    }
}
