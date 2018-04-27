using System.Collections.Generic;
using System.Linq;
using YM.Json;

namespace AP.Search
{
    public static class FieldAliasesRepository
    {
        private static readonly IDictionary<string, string[]> _appl = new Dictionary<string, string[]>()
        {
            {"byline", new string[] { "bylines.by", "editor.name", "photographer.name", "producer.name", "captionwriter.name" } },
            {"editorialtype", new string[] { "editorialtypes" } },
            {"producer", new string[] { "producer.name" } },
            {"captionwriter", new string[] { "captionwriter.name" } },
            {"photographer", new string[] { "photographer.name" } },
            {"editor", new string[] { "editor.name" } },
            {"category", new string[] { "categories.code", "categories.name" } },
            {"creditline", new string[] { "creditline" } },
            {"editorialpriority", new string[] { "editorialpriority" } },
            {"format", new string[] { "filings.format" } },
            {"fixture", new string[] { "fixture.name" } },
            {"company", new string[] { "companies.name", "companies.symbols.ticker" } },
            {"industry", new string[] { "companies.industries.name", "companies.industries.code" } },
            {"industryname", new string[] { "companies.industries.name", "companies.industries.code" } },
            {"instrument", new string[] { "companies.symbols.instrument" } },
            {"itemcontenttype", new string[] { "itemcontenttype.name" } },
            {"contenttype", new string[] { "itemcontenttype.name" } },
            {"partytype", new string[] { "persons.types" } },
            {"persontype", new string[] { "persons.types" } },
            {"ticker", new string[] { "companies.symbols.ticker" } },
            {"city", new string[] { "datelinelocation.city" } },
            {"state", new string[] { "datelinelocation.countryareacode", "datelinelocation.countryareaname" } },
            {"country", new string[] { "datelinelocation.countrycode, datelinelocation.countryname" } },
            {"location", new string[] { "dateline", "datelinelocation.city", "datelinelocation.countryareacode", "datelinelocation.countryareaname", "datelinelocation.countrycode", "datelinelocation.countryname", "places.name" } },
            {"place", new string[] { "dateline", "datelinelocation.city", "datelinelocation.countryareacode", "datelinelocation.countryareaname", "datelinelocation.countrycode", "datelinelocation.countryname", "places.name" } },
            {"geography", new string[] { "places.name", "places.code" } },
            {"geo", new string[] { "places.name", "places.code" } },
            {"productid", new string[] { "filings.products" } },
            {"product", new string[] { "filings.products" } },
            {"slug", new string[] { "filings.slugline" } },
            {"slugline", new string[] { "filings.slugline" } },
            {"transref", new string[] { "filings.transmissionreference" } },
            {"headline", new string[] { "headline" } },
            {"itemid", new string[] { "itemid" } },
            {"keyword", new string[] { "keywordlines" } },
            {"keywords", new string[] { "keywordlines" } },
            {"organization", new string[] { "organizations.name", "organizations.code" } },
            {"person", new string[] { "persons.name", "persons.code" } },
            {"recordid", new string[] { "recordid" } },
            {"source", new string[] { "sources.name", "sources.code" } },
            {"subject", new string[] { "subjects.name", "subjects.code" } },
            {"suppcat", new string[] { "suppcategories.name", "suppcategories.code" } },
            {"supcat", new string[] { "suppcategories.name", "suppcategories.code" } },
            {"title", new string[] { "title" } },
            {"selector", new string[] { "filings.selector" } },
            {"language", new string[] { "language" } },
            {"friendlykey", new string[] { "friendlykey" } },
            {"creationdate", new string[] { "firstcreated" } },
            {"firstcreated", new string[] { "firstcreated" } },
            {"arrivaldate", new string[] { "arrivaldatetime" } },
            {"date", new string[] { "arrivaldatetime" } },
            {"mediatype", new string[] { "type" } },
            {"event", new string[] { "events.name", "subjects.name" } },
            {"sourcetype", new string[] { "sources.type" } },
            {"provider", new string[] { "provider.name" } },
            {"partner", new string[] { "provider.name" } },
            {"storyid", new string[] { "editorialid", "filings.foreignkeys.storyid" } },
            {"videoid", new string[] { "editorialid", "filings.foreignkeys.storyid" } },
            {"storynumber", new string[] { "editorialid", "filings.foreignkeys.storyid" } },
            {"altids.itemid", new string[] { "itemid" } },
            {"urgency", new string[] { "priority" } },
            {"version", new string[] { "recordsequencenumber" } },
            {"altids.imageid", new string[] { "friendlykey" } },
            {"imageid", new string[] { "friendlykey" } },
            {"altids.videoid", new string[] { "editorialid", "filings.foreignkeys.storyid" } },
            {"altids.transref", new string[] { "filings.transmissionreference" } },
            {"versioncreated", new string[] { "arrivaldatetime" } },
            {"embargoed", new string[] { "releasedatetime" } },
            {"ednote", new string[] { "specialinstructions" } },
            {"description_summary_nitf", new string[] { "summary" } },
            {"description_summary", new string[] { "summary" } },
            {"located", new string[] { "dateline", "locationline" } },
            {"description_creditline", new string[] { "creditline" } },
            {"description_caption", new string[] { "caption.nitf" } },
            {"description_shotlist_nitf", new string[] { "shotlist.nitf" } },
            {"description_script_nitf", new string[] { "script.nitf" } },
            {"description_editornotes", new string[] { "publishableeditornotes.nitf" } },
            {"profile", new string[] { "itemcontenttype" } },
            {"subject.name", new string[] { "subjects.name", "categories.name", "suppcategories.name" } },
            {"subject.code", new string[] { "subjects.code", "categories.code", "suppcategories.code" } },
            {"person.type", new string[] { "persons.type" } },
            {"person.team", new string[] { "persons.team" } },
            {"person.associatedstate", new string[] { "persons.associatedstate" } },
            {"person.associatedevent", new string[] { "persons.associatedevent" } },
            {"person.extid", new string[] { "persons.extid" } },
            {"organisation", new string[] { "organizations", "organizations.name", "companies.name" } },
            {"organisation.name", new string[] { "organizations.name", "companies.name" } },
            {"place.name", new string[] { "places.name" } },
            {"body_nitf", new string[] { "main.nitf" } },
            {"renditions.backgroundcolour", new string[] { "renditions.backgroundcolor" } },
            {"renditions.colourspace", new string[] { "renditions.colorspace" } },
            {"renditions.videocodec", new string[] { "renditions.videocoder" } },
            {"renditions.producedaspectratio", new string[] { "renditions.aspectratio" } },
            {"event.code", new string[] { "events.code" } },
            {"event.extid", new string[] { "events.extid", "events.externaleventids.code" } },
            {"event.extidsource", new string[] { "events.extidsource", "events.externaleventids.creator" } }
        };

        public static IDictionary<string, string[]> Appl
        {
            get
            {
                return _appl;
            }
        }

        public static IDictionary<string, string[]> GetAliases(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            switch (key.ToLower())
            {
                case "appl": return Appl;
            }

            return null;
        }

        public static IDictionary<string, string[]> GetAliases(JsonObject aliases)
        {
            if (aliases == null || aliases.IsEmpty)
            {
                return null;
            }

            var map = new Dictionary<string, string[]>();

            foreach (var jp in aliases.Properties())
            {
                string key = jp.Name;

                if (jp.Value.Type == JsonType.String)
                {
                    map.Add(key, jp.Value.Get<string>().Split(','));
                }
                else if (jp.Value.Type == JsonType.Array)
                {
                    map.Add(key, jp.Value.Get<JsonArray>().Select(e => e.Get<string>()).ToArray());
                }
            }

            return map.Count == 0 ? null : map;
        }
    }
}