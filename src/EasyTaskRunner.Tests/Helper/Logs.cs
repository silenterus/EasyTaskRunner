namespace EasyTaskRunner.Tests.Helper;

using System;
using System.Reflection;
using Xunit.Abstractions;
public static class Logs
{
    private static ITestOutputHelper? _testOutputHelper;
    private static bool _logActive = true;
    private static readonly bool DontAllowLog = false;

    public static void Initialize(ITestOutputHelper testOutputHelper, bool logActive = true)
    {
        if(DontAllowLog)return;
        _testOutputHelper = testOutputHelper;
        _logActive = logActive;
    }

    public static void Log(string msg,string from)
    {
        if(DontAllowLog)return;
        if (!_logActive || _testOutputHelper == null) return;
        _testOutputHelper.WriteLine($"From: [{from}] ");
        _testOutputHelper.WriteLine($"      [{msg}] ");

    }

    public static void Log<T>(string from, T obj)
    {
        if(DontAllowLog)return;
        if (!_logActive || _testOutputHelper == null) return;

        Type type = typeof(T);
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        _testOutputHelper.WriteLine("");
        _testOutputHelper.WriteLine(from.TrimEnd());
        _testOutputHelper.WriteLine($"Class:[{type.Name}] ");
        _testOutputHelper.WriteLine("");
        int maxPropertyNameLength = 0;
        foreach (var property in properties)
        {
            if (property.Name.Length > maxPropertyNameLength)
            {
                maxPropertyNameLength = property.Name.Length;
            }
        }

        foreach (var property in properties)
        {
            object? value = property.GetValue(obj, null);
            string formattedPropertyName = property.Name.PadRight(maxPropertyNameLength);
            string logMessage = $"{formattedPropertyName} :[{value}] ";
            _testOutputHelper.WriteLine(logMessage.TrimEnd());
        }
    }
}
