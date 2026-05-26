using CppSharp;
using CppSharp.AST;
using CppSharp.Generators;
using CppSharp.Passes;
using System.Runtime.InteropServices;

public class NuklearLibrary : ILibrary
{
    static readonly string[] NuklearDefines =
    [
        "NK_INCLUDE_FIXED_TYPES",
        "NK_INCLUDE_STANDARD_IO",
        "NK_INCLUDE_STANDARD_VARARGS",
        "NK_INCLUDE_DEFAULT_ALLOCATOR",
        "NK_INCLUDE_STANDARD_BOOL",
        "NK_INCLUDE_VERTEX_BUFFER_OUTPUT",
        "NK_INCLUDE_FONT_BAKING",
        "NK_INCLUDE_DEFAULT_FONT",
        "NK_KEYSTATE_BASED_INPUT",
    ];

    public void Setup(Driver driver)
    {
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        var nativeDir = Path.Combine(repoRoot, "native");
        var includeDir = Path.Combine(nativeDir, "include");
        var nuklearSrcDir = Path.Combine(nativeDir, "nuklear", "src");
        var buildDir = FindNativeBuildDir(nativeDir);
        var outputDir = Path.Combine(repoRoot, "Nuklear.Net.Native", "Generated");

        var options = driver.Options;
        options.GeneratorKind = GeneratorKind.CSharp;
        options.OutputDir = outputDir;
        options.Encoding = System.Text.Encoding.UTF8;

        var module = options.AddModule("Nuklear.Net.Native");
        module.OutputNamespace = "Nuklear.Net.Native";
        module.SharedLibraryName = "NuklearNetNative";
        module.IncludeDirs.Add(includeDir);
        module.IncludeDirs.Add(nuklearSrcDir);
        module.Headers.Add("nuklear_net.h");
        module.LibraryDirs.Add(buildDir);
        module.Libraries.Add("NuklearNetNative");

        var parserOptions = driver.ParserOptions;
        parserOptions.TargetTriple = GetTargetTriple();
        parserOptions.AddArguments("-x c -std=c99");

        foreach (var define in NuklearDefines)
            parserOptions.AddDefines(define);
    }

    public void SetupPasses(Driver driver)
    {
        driver.Context.TranslationUnitPasses.RenameDeclsUpperCase(RenameTargets.Any);
        driver.Context.TranslationUnitPasses.AddPass(new FunctionToInstanceMethodPass());
    }

    public void Preprocess(Driver driver, ASTContext ctx)
    {
        ctx.IgnoreHeadersWithName("stb_*");
        ctx.IgnoreHeadersWithName("nuklear_internal.h");
        ctx.IgnoreClassField("nk_list_view", "end");
    }

    public void Postprocess(Driver driver, ASTContext ctx)
    {
    }

    static string GetTargetTriple()
    {
        if (OperatingSystem.IsWindows())
            return Environment.Is64BitProcess ? "x86_64-pc-win32-msvc" : "i686-pc-win32-msvc";

        if (OperatingSystem.IsMacOS())
            return Environment.Is64BitProcess ? "x86_64-apple-darwin" : "arm64-apple-darwin";

        return Environment.Is64BitProcess ? "x86_64-unknown-linux-gnu" : "aarch64-unknown-linux-gnu";
    }

    static string FindNativeBuildDir(string nativeDir)
    {
        var buildRoot = Path.Combine(nativeDir, "build");
        if (!Directory.Exists(buildRoot))
            return buildRoot;

        var hostRid = GetHostRuntimeIdentifier();
        var preferred = Path.Combine(buildRoot, hostRid);
        if (Directory.Exists(preferred))
            return preferred;

        var ridDirs = Directory.GetDirectories(buildRoot);
        return ridDirs.Length > 0 ? ridDirs[^1] : buildRoot;
    }

    static string GetHostRuntimeIdentifier()
    {
        if (OperatingSystem.IsWindows())
            return Environment.Is64BitProcess ? "win-x64" : "win-x86";

        if (OperatingSystem.IsMacOS())
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "osx-arm64" : "osx-x64";

        return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "linux-arm64" : "linux-x64";
    }
}

public class Program
{
    public static void Main(string[] args) => ConsoleDriver.Run(new NuklearLibrary());
}
