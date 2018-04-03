using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using YM.Json;

namespace YM.Json.Tests
{
    [TestClass]
    public class JsonTemplateTests
    {
        [TestMethod]
        public void test_templates()
        {
            var files = Directory.EnumerateFiles("TestFiles").Where(e => e.Contains("templates_"));
            foreach(string file in files)
            {
                string json = File.ReadAllText(file);
                var jo = JsonObject.Parse(json);

                foreach(var jv in jo.Property<JsonArray>("tests"))
                {
                    var test = jv.Get<JsonObject>();
                    var template = test.Property<JsonObject>("template");
                    var parameters = test.Property<JsonObject>("params").ToDictionary();
                    var output = test.Property<JsonObject>("output");

                    var merge = template.SetParameters(parameters);
                    Assert.IsTrue(merge.ToString() == output.ToString());
                }
            }
        }
    }
}
