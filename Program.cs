using System.Net.Http;
using HtmlAgilityPack; // dotnet add package htmlagilitypack
using System.IO;
using System.Collections.Generic;
using System.Globalization;

string cachePath = "latest.txt";
string baseUrl = "https://webbtelescope.org";
//string baseImageUrl = "https://stsci-opo.org";
string baseContent = "/contents/media/images/2022/031/01G780WF1VRADDSD5MDNDRKAGY?Type=Observations";
string imageType = "png"; // .tif is huge

if(File.Exists(cachePath))
{
    string text = File.ReadAllText(cachePath).TrimEnd('\n');  
    baseContent = text;
}

Console.WriteLine($"{baseUrl}{baseContent}");

// https://medium.com/c-sharp-progarmming/create-your-own-web-scraper-in-c-in-just-a-few-minutes-c42649adda8
HtmlWeb web = new HtmlWeb();
HtmlDocument doc = web.Load($"{baseUrl}{baseContent}");

var controls = doc.DocumentNode.SelectNodes("//div[@class='controls']");

bool latest = true;

if(controls.Any())
{
    //Console.WriteLine(controls.First().InnerText);
    if(controls.First().InnerText.Contains("Previous"))
    {
        latest = false;
        Console.WriteLine("Not the latest image, traveling..");

        // not the latest image
        // TODO: don't get all links on page, only this single control
        var links = controls.First().SelectNodes("//a[@href]");

        foreach(var link in links)
        {
            string hrefValue = link.GetAttributeValue( "href", string.Empty );

            if(hrefValue.StartsWith("/contents/media"))
            {
                Console.WriteLine(hrefValue);
                
                await File.WriteAllTextAsync(cachePath, hrefValue);

                break;
            }
        }
    }
    else
    {
        Console.WriteLine("Latest image");
    }
}

string imagePath = "";

if(latest)
{
    var links = controls.First().SelectNodes("//a[@href]");

    foreach(var link in links)
    {
        string hrefValue = link.GetAttributeValue( "href", string.Empty );

        //Console.WriteLine(hrefValue);

        if(hrefValue.EndsWith(imageType))
        {
            imagePath = hrefValue;
            break; // first is full res
        }
    }
}

if(!string.IsNullOrEmpty(imagePath))
{
    string fileName = imagePath.Split('/').Last();

    if(!File.Exists(fileName))
    {
        // TODO: delete prior images on disk

        string fullImagePath = $"https:{imagePath}";

        Console.WriteLine("Downloading...");
        Console.WriteLine(fullImagePath);

        using(var handler = new HttpClientHandler())
        {
            // handler.Headers.Add("User-Agent: Other");
            handler.UseDefaultCredentials = true;

            using(var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AcmeInc/1.0)");

                HttpResponseMessage response = await client.GetAsync(fullImagePath);

                try
                {
                    response.EnsureSuccessStatusCode();

                    Console.WriteLine("Successful, saving image");
                    var binaryData = await response.Content.ReadAsByteArrayAsync();

                    await File.WriteAllBytesAsync(fileName, binaryData);

                    Console.WriteLine("Done");
                }
                catch(HttpRequestException ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
            }
        }
    }
    else
    {
        Console.WriteLine($"Already downloaded: {fileName}");
    }
}