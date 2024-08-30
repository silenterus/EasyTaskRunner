namespace Build.Helper;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SimpleExec;

public static class DotnetHelper
{

    const string ProjectsName = "EasyTaskRunner";
    static readonly string[] ProjectsNameFolders = new[] { ProjectsName, $"{ProjectsName}.Core", $"{ProjectsName}.Data", $"{ProjectsName}.Extensions", $"{ProjectsName}.Tests" };
    static readonly string[] ProjectNameTestFolders = new[] { $"{ProjectsName}.Tests" };


    private static string _manifestFile = Path.Combine(Directory.GetCurrentDirectory(), ".config", "dotnet-tools.json");

    private static bool ManifestInstalled = false;

    public static readonly List<ToolData> TestTools = new List<ToolData> { new ToolData("csharpminifierconsole", "1.3.0", "csmin") };

    // Static list of predefined tools
    public static readonly List<ToolData> NeededTools = new List<ToolData>
    {
        new ToolData("codediagramcreator", "0.0.5", "diagram"),
        new ToolData("csharpier", "0.29.1", "dotnet-csharpier"),
        new ToolData("csharpminifierconsole", "1.3.0", "csmin"),
        new ToolData("dotnet-dev-certs", "2.2.0", "dotnet-dev-certs"),
        new ToolData("dotnet-document", "0.1.4-alpha", "dotnet-document"),
        new ToolData("dotnet-ef", "7.0.13", "dotnet-ef"),
        new ToolData("dotnet-format", "5.1.250801", "dotnet-format"),
        new ToolData("dotnet-outdated-tool", "4.6.0", "dotnet-outdated"),
        new ToolData("dotnet-project-licenses", "2.7.1", "dotnet-project-licenses"),
        new ToolData("dotnet-releaser", "0.7.1", "dotnet-releaser"),
        new ToolData("dotnet-tools-outdated", "0.6.0", "dotnet-tools-outdated"),
        new ToolData("jetbrains.resharper.globaltools", "2024.1.4", "jb"),
        new ToolData("orang.dotnet.cli", "0.7.0", "orang"),
        new ToolData("roslynator.dotnet.cli", "0.8.9", "roslynator"),
        new ToolData("xamlstyler.console", "3.2206", "xamlstyler"),
        new ToolData("dotnet-script", "0.22.0", "dotnet-script"),
        new ToolData("regitlint", "", "regitlint"),
    };

    public static void CheckForManifest()
    {
        if (!ManifestInstalled)
        {
            try
            {
                Command.Run("dotnet", "new tool-manifest", noEcho: true);
                ManifestInstalled = true;
            }
            catch
            {
                // File could exist but encounter error during manifest creation, recheck the file
                if (File.Exists(_manifestFile))
                {
                    ManifestInstalled = true;
                }
            }
        }
    }

    internal static void ListAllDotnetTools()
    {
        List<ToolDetails> tools = GetDotnetTools().GetAwaiter().GetResult();
        foreach (var tool in tools)
        {
            Console.WriteLine(tool);
        }
    }

    private static async Task<List<ToolDetails>> GetDotnetTools()
    {
        List<ToolDetails> toolDetailsList = new List<ToolDetails>();
        string letters = "abcdefghijklmnopqrstuvwxyz";
        foreach (var letter in letters)
        {
            var (output, error) = await Command.ReadAsync("dotnet", $"tool search {letter} --detail");
            var lineParts = output.Split("----------------");
            for (int i = 0; i < lineParts.Length; i++)
            {
                var toolDetails = ParseToolDetails(lineParts[i]);
                if (toolDetails != null && !string.IsNullOrWhiteSpace(toolDetails.PackageId))
                {
                    toolDetailsList.Add(toolDetails);
                }
            }
            break;
        }
        return toolDetailsList;
    }

    public static ToolDetails ParseToolDetails(string part)
    {
        var lines = part.Split(Environment.NewLine);

        if (lines.Length < 6)
            return null;

        ToolDetails details = new ToolDetails();
        details.PackageId = lines[1].Trim();

        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            ParseToolDetailsLine(line, ref details);
        }

        return details;
    }

    static void ParseToolDetailsLine(string entry, ref ToolDetails details)
    {
        var split = entry.Split(new[] { ":" }, 2, StringSplitOptions.None);
        switch (split[0].Trim().ToLowerInvariant())
        {
            case "packageid":
                details.PackageId = split[1].Trim();
                break;
            case "current version":
            case "aktuelle version":
                details.CurrentVersion = split[1].Trim();
                break;
            case "authors":
            case "autoren":
                details.Authors = split[1].Trim();
                break;
            case "downloads":
                if (int.TryParse(split[1].Trim(), out int downloads))
                {
                    details.Downloads = downloads;
                }
                break;
            case "verified":
            case "überprüft":
                details.Verified = split[1].Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
                break;
            case "summary":
            case "zusammenfassung":
                details.Summary = split[1].Trim();
                break;
            case "description":
            case "beschreibung":
                details.Description = split[1].Trim();
                break;
        }
    }

    public class ToolDetails
    {
        public string PackageId { get; set; }
        public string CurrentVersion { get; set; }
        public string Authors { get; set; }
        public int Downloads { get; set; }
        public bool Verified { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return $"PackageId: {PackageId}, CurrentVersion: {CurrentVersion}, Authors: {Authors}, Downloads: {Downloads}, Verified: {Verified}, Summary: {Summary}, Description: {Description}";
        }
    }

    public class ToolData(string packageId, string version, string cmd, bool installGlobal = false, bool ignoreVersion = true)
    {
        internal string PackageId { get; set; } = packageId;
        internal string Version { get; set; } = version;
        internal string Cmd { get; set; } = cmd;
        internal bool IsGlobal { get; set; } = installGlobal;
        internal bool IgnoreVersion { get; set; } = ignoreVersion;

        private bool _installed;

        public void SetInstalled()
        {
            _installed = true;
        }

        public bool IsInstalled()
        {
            return _installed;
        }

        public void Check()
        {
            if (IsInstalled())
            {
                Console.WriteLine($"{Cmd} is already installed.");

                return;
            }

            try
            {
                Command.Run("dotnet", $"{Cmd} --version");
                Console.WriteLine($"{Cmd} is already installed.");
                _installed = true;
            }
            catch
            {
                Console.WriteLine($"{Cmd} is not installed. Installing...");

                string installCmd = $"tool install {PackageId}";

                if (!IgnoreVersion)
                {
                    installCmd += $" --version {Version}";
                }

                if (IsGlobal)
                {
                    installCmd += " --global";
                }
                else
                {
                    CheckForManifest();
                    installCmd += " --local";
                }

                try
                {
                    Command.Run("dotnet", installCmd);
                    _installed = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during installation: {ex.Message}");
                }
            }
        }
    }

    public static void InstallToolsIfNotPresent(string[] toolNames)
    {
        foreach (var toolName in toolNames)
        {
            try
            {
                // Run the command to check if the tool is already installed
                Command.Run("dotnet", $"tool run {toolName} --version");

                Console.WriteLine($"{toolName} is already installed.");
            }
            catch
            {
                // If the tool is not installed, install it globally
                Console.WriteLine($"{toolName} is not installed. Installing...");
                Command.Run("dotnet", $"tool install {toolName} --global");
            }
        }
    }

    public static ToolData[] GetToolData(bool global = true)
    {
        return TestTools.ToArray();
    }

    public static void InstallToolsIfNotPresent(ToolData[] tools)
    {
        foreach (var tool in tools)
        {
            tool.Check();
        }
    }



}
