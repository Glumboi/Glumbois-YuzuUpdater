using System.Diagnostics;

namespace YuzuUpdater.Core;

public static class ProcessHelpers
{
    public static Process ExecuteProcess(string fileName, bool asAdmin = false)
    {
        return ExecuteProcess(asAdmin ? "runas" : "", fileName);
    }

    private static Process ExecuteProcess(string args, string fileName)
    {
        Process proc = new Process();
        proc.StartInfo.FileName = fileName;
        proc.StartInfo.UseShellExecute = true;
        proc.StartInfo.Verb = args;// "runas";
        proc.Start();
        return proc;
    }
}