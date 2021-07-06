using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.IO;

namespace NvDriverListUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            string geckoDriverPath = Environment.CurrentDirectory;

            List<JObject> GameReadyDriverList = new List<JObject>();
            List<JObject> StudioDriverList = new List<JObject>();


            var o = new FirefoxOptions();
            o.AddArgument("-headless");
            o.SetLoggingPreference(LogType.Browser, LogLevel.Info);

            using (var driver = new FirefoxDriver(geckoDriverPath, o))
            {
                driver.Navigate().GoToUrl("https://www.nvidia.com/Download/Find.aspx?lang=en-us");
                Console.WriteLine("[Selenium] Browser opened");
                var GPUType = driver.FindElementByCssSelector("select[name='selProductSeriesType']");
                var DCH = driver.FindElementByCssSelector("select[name='selDownloadTypeDch']");
                var GPUSeries = driver.FindElementByCssSelector("select[name='selProductSeries']");
                GPUType.SendKeys("G");


                for (int i = 1; i < 10; i++)
                {
                    GPUSeries.SendKeys("G");
                }
                DCH.SendKeys("S");

                var SearchButton = driver.FindElementByCssSelector("a[href='javascript: search();']");
                SearchButton.Click();
                Console.WriteLine("[List Updater] Started");
                int j = 1;
                foreach (var element in driver.FindElementsByCssSelector("tr[id='driverList']"))
                {
                    Console.WriteLine($"[List Updater] {j} of {driver.FindElementsByCssSelector("tr[id='driverList']").Count}");
                    if (element.Text.StartsWith("GeForce Game Ready Driver"))
                    {
                        string text = element.Text.Replace("GeForce Game Ready Driver WHQL ", "");

                        JObject DriverDetails = new JObject() { { "Version ", text.Split(" ")[0] }, { "Date", text.Replace($"{text.Split(" ")[0]} ", "")} };

                        GameReadyDriverList.Add(DriverDetails);
                    }
                    else
                    {
                        string text = element.Text.Replace("NVIDIA Studio Driver SD ", "");

                        JObject DriverDetails = new JObject() { { "Version ", text.Split(" ")[0] }, { "Date", text.Replace($"{text.Split(" ")[0]} ", "") } };

                        StudioDriverList.Add(DriverDetails);

                    }
                    j++;
                }
                Console.WriteLine("[List Updater] Done");
                driver.Close();

                JObject dr = new JObject()
                {
                    {"Game Ready", new JArray(GameReadyDriverList.ToArray()) },
                    {"Studio", new JArray(StudioDriverList.ToArray()) }
                };

                File.WriteAllText(Environment.CurrentDirectory + @"\DriverList.json", $"{dr}");

                Console.WriteLine("[Selenium] Browser closed");
                Console.Read();
            }
        }
    }
}
