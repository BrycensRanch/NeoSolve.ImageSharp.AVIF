using System;
using System.IO;
using System.Runtime.InteropServices;

namespace NeoSolve.ImageSharp.AVIF;

public static class Native {
    private static string GetExecutablePath(string name)
    {
        var path = Path.Combine(AppContext.BaseDirectory, name + ExecutableExtension);
        return File.Exists(path) ? path : name;
    }

    public static string CAVIFENC => GetExecutablePath("avifenc");
    public static string CAVIFDEC => GetExecutablePath("avifdec");

    private static string ExecutableExtension => OperatingSystem.IsWindows() ? ".exe" : string.Empty;
}
