using System;
using System.IO;
using System.IO.Compression;

namespace YuzuUpdater.Core;

public class AfterDownload
{

    public static void AfterDownloadArguments(UpdaterSettings settings)
    {
        if (settings.AdminMode && settings.LaunchAfterUpdate) ProcessHelpers.ExecuteProcess(settings.YuzuExecutable, true);

        if(settings.LaunchAfterUpdate && !settings.LaunchAfterUpdate) ProcessHelpers.ExecuteProcess(settings.YuzuExecutable);
        
        if(settings.AutoClose) Environment.Exit(0);
    }
    
    public static void Unzip(
        string inputFile,
        string outPath,
        bool deleteAfterDone = true,
        bool filesInFolder = false,
        string childFolderName = "")
    {
        using (ZipArchive source = ZipFile.Open(inputFile, ZipArchiveMode.Read, null))
        {
            foreach (ZipArchiveEntry entry in source.Entries)
            {
                string fullPath = Path.GetFullPath(Path.Combine(outPath, entry.FullName));

                if (Path.GetFileName(fullPath).Length != 0)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                    // The boolean parameter determines whether an existing file that has the same name as the destination file should be overwritten
                    entry.ExtractToFile(fullPath, true);
                }
            }

            source.Dispose();
        }

        if (filesInFolder)
        {
            CopyFilesRecursively(childFolderName, outPath);

            Directory.Delete(childFolderName, true);
        }

        if (deleteAfterDone)
        {
            File.Delete(inputFile);
        }
    }
    
    private static void CopyFilesRecursively(string sourcePath, string targetPath)
    {
        //Now Create all of the directories
        foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        }

        //Copy all the files & Replaces any files with the same name
        foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }
    }
}