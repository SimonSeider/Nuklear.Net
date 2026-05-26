using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Nuklear.Net.Native;

public static class NuklearNetNativeLoader
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        NativeLibrary.SetDllImportResolver(typeof(NkContext).Assembly, Resolve);
    }

    static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName != "NuklearNetNative")
            return IntPtr.Zero;

        if (NativeLibrary.TryLoad(libraryName, assembly, searchPath, out var handle))
            return handle;

        var baseDir = AppContext.BaseDirectory;
        var rid = GetRuntimeIdentifier();

        foreach (var fileName in GetCandidateNames())
        {
            var runtimePath = Path.Combine(baseDir, "runtimes", rid, "native", fileName);
            if (File.Exists(runtimePath) && NativeLibrary.TryLoad(runtimePath, out handle))
                return handle;

            var appLocalPath = Path.Combine(baseDir, fileName);
            if (File.Exists(appLocalPath) && NativeLibrary.TryLoad(appLocalPath, out handle))
                return handle;
        }

        return IntPtr.Zero;
    }

    static string GetRuntimeIdentifier()
    {
        if (OperatingSystem.IsWindows())
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "win-arm64" : "win-x64";

        if (OperatingSystem.IsMacOS())
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "osx-arm64" : "osx-x64";

        return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "linux-arm64" : "linux-x64";
    }

    static IEnumerable<string> GetCandidateNames()
    {
        if (OperatingSystem.IsWindows())
        {
            yield return "NuklearNetNative.dll";
            yield break;
        }

        if (OperatingSystem.IsMacOS())
        {
            yield return "libNuklearNetNative.dylib";
            yield break;
        }

        yield return "libNuklearNetNative.so";
    }
}
