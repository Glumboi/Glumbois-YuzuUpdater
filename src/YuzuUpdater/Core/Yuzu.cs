using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace YuzuUpdater.Core;

public static class Yuzu
{
    public static bool IsLocalYuzuOutdated(string localVer, string newestWeb)
    {
        int localVerNum = Int32.Parse(localVer.Split('-').Last());
        int webVerNum = Int32.Parse(newestWeb.Split('-').Last());

        return localVerNum < webVerNum ? true : false;
    }
    
    public static string GetYuzuBinaryVersion(string path)
    {
        try
        {
            Process exeProcess = ProcessHelpers.ExecuteProcess(path);

            while (string.IsNullOrEmpty(exeProcess.MainWindowTitle))
            {
                exeProcess.Refresh();
            }

            exeProcess.Kill();

            return "EA-" + exeProcess.MainWindowTitle.Split(' ').Last();
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}