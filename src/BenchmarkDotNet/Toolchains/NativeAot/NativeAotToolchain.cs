﻿using System;
using System.Collections.Generic;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.DotNetCli;

namespace BenchmarkDotNet.Toolchains.NativeAot
{
    public class NativeAotToolchain : Toolchain
    {
        /// <summary>
        /// compiled as net5.0, targets experimental 6.0.0-* NativeAOT build from the https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-experimental/nuget/v3/index.json
        /// </summary>
        public static readonly IToolchain Net50 = GetBuilderForOldExperimentalFeed().TargetFrameworkMoniker("net5.0").ToToolchain();
        /// <summary>
        /// compiled as net6.0, targets experimental 6.0.0-* NativeAOT build from the https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-experimental/nuget/v3/index.json
        /// </summary>
        public static readonly IToolchain Net60 = GetBuilderForOldExperimentalFeed().TargetFrameworkMoniker("net6.0").ToToolchain();
        /// <summary>
        /// compiled as net7.0, targets latest (7.0.0-*) NativeAOT build from the .NET 7 feed: https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet7/nuget/v3/index.json
        /// </summary>
        public static readonly IToolchain Net70 = CreateBuilder().UseNuGet().TargetFrameworkMoniker("net7.0").ToToolchain();

        private static NativeAotToolchainBuilder GetBuilderForOldExperimentalFeed()
            => CreateBuilder().UseNuGet("6.0.0-*", "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-experimental/nuget/v3/index.json");

        internal NativeAotToolchain(string displayName,
            string ilCompilerVersion, string ilcPath, bool useCppCodeGenerator,
            string runtimeFrameworkVersion, string targetFrameworkMoniker, string runtimeIdentifier,
            string customDotNetCliPath, string packagesRestorePath,
            Dictionary<string, string> feeds, bool useNuGetClearTag, bool useTempFolderForRestore,
            bool rootAllApplicationAssemblies, bool ilcGenerateCompleteTypeMetadata, bool ilcGenerateStackTraceData, string trimmerDefaultAction)
            : base(displayName,
                new Generator(ilCompilerVersion, useCppCodeGenerator, runtimeFrameworkVersion, targetFrameworkMoniker, customDotNetCliPath,
                    runtimeIdentifier, feeds, useNuGetClearTag, useTempFolderForRestore, packagesRestorePath,
                    rootAllApplicationAssemblies, ilcGenerateCompleteTypeMetadata, ilcGenerateStackTraceData, trimmerDefaultAction),
                new DotNetCliPublisher(customDotNetCliPath, GetExtraArguments(useCppCodeGenerator, runtimeIdentifier), GetEnvironmentVariables(ilcPath)),
                new Executor())
        {
            IlcPath = ilcPath;
        }

        public string IlcPath { get; }

        public static NativeAotToolchainBuilder CreateBuilder() => NativeAotToolchainBuilder.Create();

        public static string GetExtraArguments(bool useCppCodeGenerator, string runtimeIdentifier)
            => useCppCodeGenerator ? $"-r {runtimeIdentifier} /p:NativeCodeGen=cpp" : $"-r {runtimeIdentifier}";

        // https://github.com/dotnet/corert/blob/7f902d4d8b1c3280e60f5e06c71951a60da173fb/Documentation/how-to-build-and-run-ilcompiler-in-console-shell-prompt.md#compiling-source-to-native-code-using-the-ilcompiler-you-built
        // we have to pass IlcPath env var to get it working
        private static IReadOnlyList<EnvironmentVariable> GetEnvironmentVariables(string ilcPath)
            => ilcPath == null ? Array.Empty<EnvironmentVariable>() : new[] { new EnvironmentVariable("IlcPath", ilcPath) };
    }
}
