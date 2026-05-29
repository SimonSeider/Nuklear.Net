# Nuklear.Net.Natve

.NET 10 Native bindings for [Nuklear](https://github.com/Immediate-Mode-UI/Nuklear), a single-header immediate-mode GUI library written in C.

This Contains only the Native Bindings for Nuklear.Net, though not recommened you can still use this Library if you know what you are doing.

---

## Requirements

- .NET 10 SDK
- CMake ≥ 3.16
- A C99 compiler (GCC / Clang / MSVC)

## Building the native library

The native shared library must be built before the managed projects can compile.

**Linux / macOS**

```bash
./scripts/build-native.sh linux-x64   # or linux-arm64 / osx-x64 / osx-arm64
```

**Windows**

```powershell
.\scripts\build-native.ps1 -Rid win-x64   # or win-arm64
```

The script runs CMake, builds the shared library, and copies it to `Nuklear.Net.Native/runtimes/<rid>/native/`.

## Regenerating bindings

```bash
dotnet run --project Nuklear.Net.NativeGen/Nuklear.Net.NativeGen.csproj
```

Requires the native build for the host RID to already exist. Output is written to `Nuklear.Net.Native/Generated/`.
