using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace YuzuUpdater.Core;

public class Github
{
    private static IEnumerable<YuzuVersion> GetYuzuEABuilds(int limit)
    {
        limit++; //Increase limit by one because we gonna remove the "latest" release later
        
        List<string> linksToVisit = Networking.ParseLinks(@"https://pineappleea.github.io/");

        for (int i = 0; i < limit; i++)
        {
            yield return new YuzuVersion(linksToVisit[i].Split('/').Last(),GetYuzuEADDL(linksToVisit[i]));
        }
    }

    public static string GetYuzuEADDL(string versionLink)
    {
        var links = Networking.GetAllLinksFromGithubReleases(versionLink);

        foreach (var link in links)
        {
            if (link.Contains(".zip") || link.Contains(".7z"))
            {
                return link;
            }
        }

        return string.Empty;
    }
    
    public static List<YuzuVersion> GetYuzuVersionInstances(int limit)
    {
        List<YuzuVersion> result = new List<YuzuVersion>();
        
        foreach (var item in GetYuzuEABuilds(limit))
        {
            result.Add(item);
        }
        result.RemoveAt(0); //Remove the "latest" release
        return result;
    }
}