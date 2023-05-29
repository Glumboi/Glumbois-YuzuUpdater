using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace YuzuUpdater.Core;

public struct UpdaterSettings
{
    public string YuzuPath
    {
        get;
        set;
    }

    public bool LaunchAfterUpdate
    {
        get;
        set;
    }

    public bool AdminMode
    {
        get;
        set;
    }

    public bool AutoClose
    {
        get;
        set;
    }

    public int ShownReleasesCount
    {
        get;
        set;
    }

    public string YuzuExecutable
    {
        get
        {
            try
            {
                return Path.Combine(YuzuPath, "yuzu.exe");
            }
            catch
            {
                return "";
            }
        }
    }

    public PropertyInfo[] GetProperties()
    {
        return GetType().GetProperties();
    }

    public UpdaterSettings(string yuzuPath, bool launchAfterUpdate, bool adminMode, bool autoClose, int shownReleasesCount)
    {
        YuzuPath = yuzuPath;
        LaunchAfterUpdate = launchAfterUpdate;
        AdminMode = adminMode;
        AutoClose = autoClose;
        ShownReleasesCount = shownReleasesCount;
    }
}