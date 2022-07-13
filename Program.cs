using System.Net.Http;
using HtmlAgilityPack; // dotnet add package htmlagilitypack
using System.IO;
using System.Collections.Generic;
using System.Globalization;

string cachePath = "latest.txt";
string baseUrl = "https://webbtelescope.org";
string baseContent = "/contents/media/images/2022/031/01G780WF1VRADDSD5MDNDRKAGY?Type=Observations";

if(File.Exists(cachePath))
{
    string text = File.ReadAllText(cachePath).TrimEnd('\n');  
    baseContent = text;
}

Console.WriteLine(baseContent);

// https://medium.com/c-sharp-progarmming/create-your-own-web-scraper-in-c-in-just-a-few-minutes-c42649adda8
HtmlWeb web = new HtmlWeb();
HtmlDocument doc = web.Load($"{baseUrl}{baseContent}");

var controls = doc.DocumentNode.SelectNodes("//div[@class='controls']");

if(controls.Any())
{
    //Console.WriteLine(controls.First().InnerText);
    if(controls.First().InnerText.Contains("Previous"))
    {
        Console.WriteLine("Not the latest image, traveling..");

        // not the latest image
        // TODO: don't get all links on page, only this single control
        var links = controls.First().SelectNodes("//a[@href]");

        if(links.Any())
        {
            foreach(var link in links)
            {
                string hrefValue = link.GetAttributeValue( "href", string.Empty );

                if(hrefValue.StartsWith("/contents/media"))
                {
                    Console.WriteLine(hrefValue);
                    
                    File.WriteAllTextAsync(cachePath, hrefValue);

                    break;
                }
            }
        }
    }
    else
    {
        Console.WriteLine("Latest image");
    }
}
