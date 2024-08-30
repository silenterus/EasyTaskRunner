using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Build.Helper;

public static class TokenLoader
{
    public class TokenConfig
    {
        private Dictionary<string, string> Tokens = new Dictionary<string, string>()
        {
            {"nuget",""},
            {"openai",""},
            {"git",""},
        };
    }

    const string ProjectsName = "EasyTaskRunner";
    static readonly string[] ProjectsNameFolders = new[] { ProjectsName, $"{ProjectsName}.Core", $"{ProjectsName}.Data", $"{ProjectsName}.Extensions", $"{ProjectsName}.Tests" };
    static readonly string[] ProjectNameTestFolders = new[] { $"{ProjectsName}.Tests" };



    public class BuildConfig
    {
        public TokenConfig Tokens { get; set; } = new TokenConfig();
        public string Name { get; set; } = ProjectsName;
        public string[] Rids { get; set; } = new string[] {"win-x64","win-x86","linux-x64","linux-arm","linux-arm64","osx-x64","osx-arm64","win-arm64","linux-bionic-arm64" };
        public string[] Frameworks { get; set; } = new string[] { "net5.0","net6.0","net7.0","net8.0","netcoreapp3.1","netstandard2.1","net472","net48","net481" };
        public string[] ProjectsNameFolder { get; set; } = ProjectsNameFolders;
        public string[] ProjectNameTestFolder { get; set; } = ProjectNameTestFolders;

    }

    private static readonly string[] TokenKeys = { "nuget", "openai", "git" };
    private static readonly string filePath = @".\config\token.json";

    public static Dictionary<string, string> LoadTokens()
    {
        Dictionary<string, string> tokens = new Dictionary<string, string>();

        try
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                tokens = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                if (tokens == null)
                {
                    Console.WriteLine("The token.json file is empty or invalid. You'll need to re-enter the tokens.");
                    tokens = AskForTokens();
                }
            }
            else
            {
                Console.WriteLine("The token.json file does not exist. You'll need to enter the tokens.");
                tokens = AskForTokens();
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error parsing the token.json file: {ex.Message}");
            tokens = AskForTokens();
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error reading the token.json file: {ex.Message}");
            tokens = AskForTokens();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            tokens = AskForTokens();
        }

        return tokens;
    }

    private static Dictionary<string, string> AskForTokens()
    {
        Dictionary<string, string> tokens = new Dictionary<string, string>();

        foreach (string tokenKey in TokenKeys)
        {
            string tokenValue;
            do
            {
                Console.Write($"Enter value for {tokenKey} or 's' to skip: ");
                tokenValue = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(tokenValue))
                {
                    Console.WriteLine("Input cannot be empty. Please enter a valid token or 's' to skip.");
                }
                else if (tokenValue.ToLower() == "s")
                {
                    break;
                }
                else
                {
                    tokens[tokenKey] = tokenValue;
                    break;
                }
            } while (true);
        }

        SaveTokens(tokens);
        return tokens;
    }


    public static void SaveTokens(Dictionary<string, string> tokens)
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

            string jsonString = JsonSerializer.Serialize(tokens, new JsonSerializerOptions { WriteIndented = true });
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
}
