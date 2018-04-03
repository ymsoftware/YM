using MathNet.Numerics.Statistics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace YM.Json.Tests.ConsoleApp
{
    class Program
    {
        static readonly string _dir = @"C:\tmp\appl";

        static void Main(string[] args)
        {
            var docs = LoadDocuments();
            CheckParseCorrectness(docs);
            //CheckToStringCorrectness(docs);

            for (int i = 0; i < 10; i++)
            {
                CheckPerformance(docs);
            }

            Console.Read();
        }

        static void CheckParseCorrectness(string[] docs)
        {
            Console.WriteLine("\nChecking parsing correctness...");

            int count = 0;
            int errors = 0;

            foreach (string file in Directory.GetFiles(_dir, "*.json", SearchOption.AllDirectories))
            {
                string doc = File.ReadAllText(file);

                var jo1 = JsonObject.Parse(doc);
                var jo2 = JObject.Parse(doc);

                count++;
                if (count % 100 == 0)
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write("Checked {0} documents", count);
                }

                if (!Compare(jo1, jo2))
                {
                    errors++;
                    break;
                }
            }

            Console.WriteLine();

            if (errors > 0)
            {
                Console.WriteLine("Failed correctness test:  {0} errors :-(", errors);
            }
            else
            {
                Console.WriteLine("All {0} documents parsed correctly!", count);
            }
        }

        static void CheckToStringCorrectness(string[] docs)
        {
            Console.WriteLine("\nChecking ToString correctness...");

            int count = 0;
            int errors = 0;

            foreach (string file in Directory.GetFiles(_dir, "*.json", SearchOption.AllDirectories))
            {
                string doc = File.ReadAllText(file);

                var jo1 = JsonObject.Parse(doc);
                string json1 = jo1.ToString();

                var jo2 = JObject.Parse(doc);
                string json2 = jo2.ToString();

                count++;
                if (count % 100 == 0)
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write("Checked {0} documents", count);
                }

                var test2 = JObject.Parse(json1);
                if (test2.ToString() != json2)
                {
                    errors++;
                }
                else
                {
                    var test1 = JsonObject.Parse(json2);
                    if (test1.ToString() != json1)
                    {
                        errors++;
                    }
                }
            }

            if (errors > 0)
            {
                Console.WriteLine("Failed ToString correctness test:  {0} errors :-(", errors);
            }
            else
            {
                Console.WriteLine("All {0} documents passed ToString correctness test!", count);
            }
        }

        static void CheckPerformance(string[] docs)
        {
            Console.WriteLine("\nChecking performance...");

            var ticks1 = new List<long>();
            var ticks2 = new List<long>();

            foreach (string file in Directory.GetFiles(_dir, "*.json", SearchOption.AllDirectories))
            {
                string doc = File.ReadAllText(file);

                try
                {
                    var t1 = Stopwatch.StartNew();
                    var jo1 = JsonObject.Parse(doc);
                    t1.Stop();
                    ticks1.Add(t1.ElapsedTicks);
                }
                catch { }

                try
                {
                    var t2 = Stopwatch.StartNew();
                    var jo2 = JObject.Parse(doc);
                    t2.Stop();
                    ticks2.Add(t2.ElapsedTicks);
                }
                catch { }
            }

            long min1 = ticks1.Min();
            long max1 = ticks1.Max();
            long avg1 = (long)ticks1.Average();
            long med1 = (long)ticks1.Select(e => (double)e).Median();
            Console.WriteLine("YM.Json:\t{0}\t{1}\t{2}\t{3}", min1, max1, avg1, med1);

            long min2 = ticks2.Min();
            long max2 = ticks2.Max();
            long avg2 = (long)ticks2.Average();
            long med2 = (long)ticks2.Select(e => (double)e).Median();
            Console.WriteLine("Json.Net:\t{0}\t{1}\t{2}\t{3}", min2, max2, avg2, med2);
        }

        static string[] LoadDocuments()
        {
            Console.WriteLine("Loading Json documents from {0}...", _dir);

            var docs = Directory
                .GetFiles(_dir, "*.json", SearchOption.TopDirectoryOnly)
                .Select(e => File.ReadAllText(e))
                .ToArray();

            Console.WriteLine("Loaded {0} documents", docs.Length);

            return docs;
        }

        static bool Compare(JsonObject jo1, JObject jo2)
        {
            var ps1 = jo1.Properties();
            var ps2 = jo2.Properties();

            if (ps1.Length != ps2.Count())
            {
                return false;
            }

            foreach (var p1 in ps1)
            {
                if (p1.Value.Type == JsonType.Object)
                {
                    var p2 = ps2.First(e => e.Name == p1.Name);
                    if (!Compare(p1.Value.Get<JsonObject>(), p2.Value as JObject))
                    {
                        return false;
                    }
                }
                else if (p1.Value.Type == JsonType.Array)
                {
                    var ja1 = p1.Value.Get<JsonArray>().ToArray();

                    var p2 = ps2.First(e => e.Name == p1.Name);
                    var ja2 = (p2.Value as JArray).ToArray();

                    for (int i = 0; i < ja1.Length; i++)
                    {
                        var item1 = ja1[i];
                        if (item1.Type == JsonType.Object)
                        {
                            var item2 = ja2[i];

                            if (!Compare(item1.Get<JsonObject>(), item2 as JObject))
                            {
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    var p2 = ps2.FirstOrDefault(e => e.Name == p1.Name);
                    if (p2 == null)
                    {
                        return false;
                    }

                    if (p2.Value.Type != JTokenType.Date && (p1.ToString() != p2.ToString()) && (p1.Value.Get().ToString() != p2.Value.ToString()))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}