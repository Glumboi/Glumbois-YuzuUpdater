using System.Linq;

namespace YuzuUpdater.Core;

public struct YuzuVersion
{
    public string VersionNumber
    {
        get => Name.Split(' ').Last();
    }    
    
    public string DirectDownloadUrl
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }
    
    public YuzuVersion(string name, string directDownloadUrl)
    {
        Name = name;
        DirectDownloadUrl = directDownloadUrl;
    }
}