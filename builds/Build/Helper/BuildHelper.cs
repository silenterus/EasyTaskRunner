namespace Build.Helper;

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
public static class BuildHelper
{

    private static TokenLoader.BuildConfig Config = new TokenLoader.BuildConfig();


    private static readonly string filePath = @".\config\token.json";
    public static void LoadConfig()
    {

        try
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                Config = JsonSerializer.Deserialize<TokenLoader.BuildConfig>(json);

                if (Config != null)
                {
                    return;
                }

                Console.WriteLine("The token.json file is empty or invalid. You'll need to re-enter the tokens.");
                SaveConfig();
                return;
            }
            else
            {
                Console.WriteLine("The token.json file does not exist. You'll need to enter the tokens.");
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error parsing the token.json file: {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error reading the token.json file: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
        SaveConfig();

    }

    private static void SaveConfig()
    {
        try
        {
            FileInfo fileInfo = new FileInfo(filePath);
            string directoryPath = fileInfo.DirectoryName;
            if (!Directory.Exists(directoryPath))
            {
                if (directoryPath != null)
                {
                    Directory.CreateDirectory(directoryPath);
                }
            }

            string jsonString = JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true });
            string tempFilePath = $"{filePath}.tmp";
            File.WriteAllText(tempFilePath, jsonString);
            File.Move(tempFilePath, filePath, true);
            Console.WriteLine("Tokens have been saved successfully.");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error saving the tokens: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Permission error while saving the tokens: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }


    public static string GetProjectName()
    {
        return Config.Name;
    }

    public static string[] GetProjectsNameFolders()
    {
        return Config.ProjectsNameFolder;
    }

    public static string[] GetProjectsNameProjectFile()
    {
        return CreateProjectFileNames( Config.ProjectsNameFolder);
    }

    public static string[] GetProjectsNameBuild()
    {
        return CreateProjectFileNames( Config.ProjectNameTestFolder);
    }

    public static string[] GetProjectsNameTestFolders()
    {
        return  Config.ProjectNameTestFolder;
    }

    public static string[] GetProjectsNameTestProjectFile()
    {
        return CreateProjectFileNames( Config.ProjectNameTestFolder);
    }

    private static string[] GetProjectsNameCache(string prefix = "src/", string appendix = "bin")
    {
        return CreateProjectFileNames( Config.ProjectsNameFolder);
    }

    public static void RemoveCaches(string[] locations)
    {
        foreach (var dir in locations)
        {
            foreach (var folder in GetProjectsNameCache("src/", $"/{dir}"))
            {
                RemoveDirectory($"{folder}");
            }
        }
    }

    private static void RemoveDirectory(string d)
    {
        if (Directory.Exists(d))
        {
            Console.WriteLine($"Cleaning {d}");
            Directory.Delete(d, true);
        }
    }

    private static string[] CreateProjectFileNames(string[] names, string prefix = "src/", string appendix = ".csproj")
    {
        string[] projectFiles = new string[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            names[i] = $"{prefix}{names[i]}/{names[i]}{appendix}";
        }

        return projectFiles;
    }


    static string GetSolutionDirectory() => Path.GetFullPath(Path.Combine(GetScriptDirectory(), @"../.."));

    static string GetScriptDirectory([CallerFilePath] string filename = null) => Path.GetDirectoryName(filename);

    static string GetVsLocation() => string.Empty;

    static string GetJetbrainsLocation() => string.Empty;

    static string GetJetbrainsRiderLocation() => string.Empty;

    static string GetJetbrainsFleetLocation() => string.Empty;

    static string GetMsBuildLocation() => Path.Combine(GetVsLocation(), @"MSBuild/Current/Bin/MSBuild.exe");

}
