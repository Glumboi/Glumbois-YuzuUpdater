using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using HtmlAgilityPack;
using Microsoft.WindowsAPICodePack.Taskbar;
using Path = System.IO.Path;

namespace YuzuUpdater.Core;

public class Networking
{
    private static Window _window;
    private static Stopwatch _stopWatch = new Stopwatch();
    private static bool _downloading = false;
    private static string _progressBarText;
    private static int _currentProgress;
    private static UpdaterSettings _settings;
    private static string _downloadedUrl;
    private static string _downloadedPath;
    
    public static List<string> ParseLinks(string urlToCrawl)
    {
        List<string> hrefs = GetHrefs(urlToCrawl);
        HashSet<string> result = new HashSet<string>();

        foreach (var href in hrefs)
        {
            result.Add(GetAbsoluteUrlString(urlToCrawl, href));
        }
        return result.ToList();
    }
    
    public static List<string> GetAllLinksFromGithubReleases(string site)
    {
        string expandedReleases = site.Replace("releases/tag", "releases/expanded_assets");

        var doc = new HtmlWeb().Load(expandedReleases);
        var linkTags = doc.DocumentNode.Descendants("link");
        var linkedPages = doc.DocumentNode.Descendants("a")
            .Select(a => a.GetAttributeValue("href", null))
            .Where(u => !string.IsNullOrEmpty(u));
        var res = new List<string>();

        foreach (var item in linkedPages)
        {
            res.Add("https://github.com" + item);
        }

        return res;
    }
    
    public static List<string> GetHrefs(string url)
    {
        // declaring & loading dom
        HtmlWeb web = new HtmlWeb();
        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
        doc = web.Load(url);
        List<string> resultList = new List<string>();

        // extracting all links
        foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
        {
            HtmlAttribute att = link.Attributes["href"];

            if (att.Value.Contains("a"))
            {
                resultList.Add(att.Value);
            }
        }

        return resultList;
    }
    
    private static string GetAbsoluteUrlString(string baseUrl, string url)
    {
        var uri = new Uri(url, UriKind.RelativeOrAbsolute);
        if (!uri.IsAbsoluteUri)
            uri = new Uri(new Uri(baseUrl), uri);
        return uri.ToString();
    }

    public static void AssignWindow(Window window)
    {
        _window = window;
    }
    
    public static void DownloadFile(
        string directDownloadLink, 
        string destinationFile, 
        PropertyInfo progressText, 
        PropertyInfo progressValue, 
        PropertyInfo progressBarVisibility, 
        object propertySrc,
        UpdaterSettings settings)
    {
        _downloadedPath = destinationFile;
        _downloadedUrl = directDownloadLink;
        _settings = settings;
        progressBarVisibility.SetValue(propertySrc ,Visibility.Visible);

        _downloading = true;
        _stopWatch.Start(); 
        using (WebClient client = new WebClient()) 
        {
            client.DownloadProgressChanged += ClientOnDownloadProgressChanged; 
            client.DownloadFileCompleted += ClientOnDownloadFileCompleted;
            Task.Run(() =>
            {
                while (_downloading)
                {
                    progressText.SetValue(propertySrc ,_progressBarText);
                    progressValue.SetValue(propertySrc ,_currentProgress);
                }

                progressBarVisibility.SetValue(propertySrc ,Visibility.Hidden);
            });
            client.DownloadFileAsync(new Uri(directDownloadLink), destinationFile);
        }
    }
    
    private static void ClientOnDownloadFileCompleted(object? sender, AsyncCompletedEventArgs e)
    {
        _downloading = false;
        _stopWatch.Reset();
        
        AfterDownload.Unzip(_downloadedPath, Path.GetDirectoryName(_downloadedPath), true, true, Path.Combine(Path.GetDirectoryName(_downloadedPath), "yuzu-windows-msvc-early-access"));
        AfterDownload.AfterDownloadArguments(_settings);
    }

    private static void ClientOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        double bytesIn = double.Parse(e.BytesReceived.ToString());
        double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
        double percentage = bytesIn / totalBytes * 100;
        int percentageString = int.Parse(Math.Truncate(percentage).ToString());

        //Used to display a progressbar on the taskbar
        if (_window != null)
        {
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal, _window);
            TaskbarManager.Instance.SetProgressValue(e.ProgressPercentage, 100, _window);
        }

        // Calculate progress values
        double fileSize = totalBytes / 1024.0 / 1024.0;
        double downloadSpeed = e.BytesReceived / 1024.0 / 1024.0 / _stopWatch.Elapsed.TotalSeconds;

        //Calculate ETA
        double remainingBytes = totalBytes - bytesIn;
        double remainingTime = remainingBytes / (downloadSpeed * 1024 * 1024);
        string remainingTimeString = TimeSpan.FromSeconds(remainingTime).ToString(@"hh\:mm\:ss");

        _currentProgress = e.ProgressPercentage;
        
        _progressBarText = string.Format("{0} MB/s | {1}: {2} MB | ETA: {3}",
            downloadSpeed.ToString("0.00"),
            "File Size",
            GetFileSizeWithoutComma(fileSize),
            remainingTimeString);
    }
    
    private static string GetFileSizeWithoutComma(double totalBytes)
    {
        if (totalBytes.ToString().Contains(","))
        {
            return totalBytes.ToString().Split(',')[0];
        }

        return totalBytes.ToString().Split('.')[0];
    }
}