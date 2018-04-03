using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace YM.Json.Tests
{
    [TestClass]
    public class ParsingTests
    {
        [TestMethod]
        public void object_different_type_properties()
        {
            string json = File.ReadAllText(@"TestFiles\object_different_type_properties.json");
            var jv = json.ToJson();
            Assert.IsTrue(jv.Type == JsonType.Object);

            var jo = jv.Get<JsonObject>();
            Assert.IsTrue(jo.Properties().Length == 6);

            jo = JsonObject.Parse(json);
            json = jo.ToString();

            Assert.IsTrue(jo.Property<int>("id") == 1);
            Assert.IsTrue(jo.Property<string>("name") == "YM");
            Assert.IsTrue(jo.Property<bool>("success"));

            var grades = jo.Property<JsonArray>("grades");
            TestArray(grades);

            var @params = jo.Property<JsonObject>("params");
            Assert.IsTrue(@params.Property<string>("query") == "test");
            Assert.IsTrue(@params.Property<long>("size") == 100);
        }

        [TestMethod]
        public void array_different_type_items()
        {
            string json = File.ReadAllText(@"TestFiles\array_different_type_items.json");
            var jv = json.ToJson();
            Assert.IsTrue(jv.Type == JsonType.Array);

            var ja = jv.Get<JsonArray>();
            Assert.IsTrue(ja.Length == 5);

            ja = JsonArray.Parse(json);
            TestArray(ja);
        }

        [TestMethod]
        public void date_formats()
        {
            string json = File.ReadAllText(@"TestFiles\array_different_type_items.json");
            var jv = json.ToJson();
            Assert.IsTrue(jv.Type == JsonType.Array);

            var ja = jv.Get<JsonArray>();
            Assert.IsTrue(ja.Length == 5);

            ja = JsonArray.Parse(json);
            TestArray(ja);
        }

        private void TestArray(JsonArray ja)
        {
            var grade = ja.Get<JsonObject>(0);
            Assert.IsTrue(grade.Property<string>("subject") == "Math");
            Assert.IsTrue(grade.Property<float>("grade") == 5f);
            grade = ja.Get<JsonObject>(1);
            Assert.IsTrue(grade.Property<string>("subject") == "English");
            Assert.IsTrue(grade.Property<double>("grade") == 3.74);
            int number = ja.Get<int>(2);
            Assert.IsTrue(number == 5);
            double e = ja.Get<double>(3);
            Assert.IsTrue(e == 3140000000000);
            string xyz = ja.Get<string>(4);
            Assert.IsTrue(xyz == "xyz");
        }
    }
}
