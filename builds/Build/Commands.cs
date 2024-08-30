using static Build.Helper.DotnetHelper;
using static Bullseye.Targets;
using static SimpleExec.Command;
using static Build.Helper.BuildHelper;

namespace Build;

using System;

public static class Commands
{

    const string Restore = "restore";
    const string Delete = "delete";
    const string Clean = "clean";
    const string Build = "build";
    const string Publish = "publish";
    const string Release = "release";
    const string Pack = "pack";
    const string PackAll = "pack-all";

    const string Nuget = "pack-all";


    const string Test = "test";
    const string Format = "format";
    const string Reg= "reg";
    const string RegFormat = "reg-format";

    const string CheckTools = "check-tools";
    const string InstallTools = "install-tools";
    const string Manifest = "manifest";
    const string ManifestCheck = "manifest-check";

    private static bool IsValidArgumentLengthForCommand(int length, string name)
    {
        switch (name)
        {
            case Restore:
            case Delete:
            case Clean:
            case Build:
            case Test:
            case Format:
            case Publish:
            case InstallTools:
            case Manifest:
            case ManifestCheck:
            case Release:
            case CheckTools:
            case Reg:
            case RegFormat:
                return length >= 1;


            default:
                return false;
        }
    }
    public static void AddTargets(string[] args)
    {

        if (args.Length == 0)
        {
            return;
        }

        if (!IsValidArgumentLengthForCommand(args.Length, args[0]))
        {
            return;
        }

        Target(Manifest, CheckForManifest);
        Target(ManifestCheck, ListAllDotnetTools);

        Target(
            Delete,
            () =>
            {
                RemoveCaches(["obj", "bin", "nupkg"]);
            }
        );
        Target(
            Restore,
            DependsOn(Delete),
            () =>
            {
                Run("dotnet", "restore");
            }
        );
        Target(
            Clean,
            DependsOn(Restore),
            () =>
            {
                Run("dotnet", "build");
            }
        );


        Target(
            RegFormat,
            () =>
            {
                Run("dotnet", "tool restore");
                Run("dotnet", "reglint");
                Run("dotnet", "csharpier .");
            }
        );


        Target(
            Format,
            () =>
            {
                Run("dotnet", "tool restore");
                Run("dotnet", "csharpier .");
            }
        );


        Target(Reg, () =>   Run("dotnet", "reglint"));

        Target(Build,() => Run("dotnet", "build . -c Release"));

        Target(Test, DependsOn(Build), ForEach(GetProjectsNameTestProjectFile()), projectPath => Run("dotnet", $"test {projectPath} --no-restore --no-build --verbosity=normal"));

        Target(
            Publish,
            ForEach(GetProjectsNameProjectFile()),
            project =>
            {
                Run("dotnet", $"publish {project} -c Release -f net8.0 -o ./publish --no-restore --no-build --verbosity=normal");
            }
        );

        Target(
            Publish,
            ForEach(GetProjectsNameProjectFile()),
            project =>
            {
                Run("dotnet", $"publish {project} -c Release -f net8.0 -o ./publish --no-restore --no-build --verbosity=normal");
            }
        );



        Target("default", DependsOn(Publish), () => Console.WriteLine("Done!"));
    }
}
