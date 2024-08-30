using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace EasyTaskRunner.Data.Utilities;

public class ExceptionData
{
    public string Message { get; set; }
    public string Source { get; set; }
    public List<string> StackTrace { get; set; }
    public MethodBase TargetSite { get; set; }
    public Dictionary<object, object> Data { get; set; }
    public ExceptionData InnerExceptionData { get; set; }
    public Uri? HelpLink { get; set; }
    public int HResult { get; set; }

    public ExceptionData(Exception ex)
    {
        // Populate properties with exception data
        Message = ex.Message;
        Source = ex.Source;
        StackTrace = new List<string>(ex.StackTrace?.Split(new[] { Environment.NewLine }, StringSplitOptions.None) ?? []);
        TargetSite = ex.TargetSite;
        HelpLink = !string.IsNullOrEmpty(ex.HelpLink) ? new Uri(ex.HelpLink) : null;
        HResult = ex.HResult;
        Data = new Dictionary<object, object>();

        // Populate the Data dictionary
        foreach (var key in ex.Data.Keys)
        {
            Data[key] = ex.Data[key];
        }

        // Recursively populate the inner exception data
        if (ex.InnerException != null)
        {
            InnerExceptionData = new ExceptionData(ex.InnerException);
        }
    }

    // Method to display the exception data (for demonstration purposes)
    public void Display()
    {
        Console.WriteLine("Exception Data:");
        Console.WriteLine($"Message: {Message}");
        Console.WriteLine($"Source: {Source}");

        if (StackTrace.Count > 0)
        {
            Console.WriteLine("StackTrace:");
            foreach (var line in StackTrace)
            {
                Console.WriteLine($"    {line}");
            }
        }

        if (TargetSite != null)
        {
            Console.WriteLine($"TargetSite: {TargetSite.Name}");
        }

        if (HelpLink != null)
        {
            Console.WriteLine($"HelpLink: {HelpLink.AbsoluteUri}");
        }

        Console.WriteLine($"HResult: {HResult}");

        if (Data.Count > 0)
        {
            Console.WriteLine("Data:");
            foreach (var entry in Data)
            {
                Console.WriteLine($"    Key: {entry.Key}, Value: {entry.Value}");
            }
        }

        if (InnerExceptionData != null)
        {
            Console.WriteLine("Inner Exception Data:");
            InnerExceptionData.Display();
        }
    }
}

public class ExceptionEntity
{
    public int Id { get; set; }

    public string Message { get; set; }
    public string Source { get; set; }
    public MethodBase TargetSite { get; set; }
    public Uri? HelpLink { get; set; }
    public int HResult { get; set; }

    public string Hash { get; set; }

    public ICollection<StackTraceEntity> StackTraces { get; set; }
    public ICollection<DataEntity> DataEntries { get; set; }
    public InnerExceptionEntity InnerException { get; set; }

    public ExceptionEntity()
    {
        StackTraces = new List<StackTraceEntity>();
        DataEntries = new List<DataEntity>();
    }

    public void GenerateHash()
    {
        using (var sha256 = SHA256.Create())
        {
            var dataToHash = $"{Message ?? ""}{Source ?? ""}{TargetSite?.Name ?? ""}{HelpLink?.ToString() ?? ""}";

            if (StackTraces.Count > 0)
            {
                dataToHash += StackTraces.First().Line;
            }

            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(dataToHash));
            Hash = Convert.ToBase64String(hashBytes);
        }
    }
}

public class StackTraceEntity
{
    public StackTraceEntity(string line)
    {
        Line = line;
    }

    public int Id { get; set; }
    public string Line { get; set; }
}

public class DataEntity
{
    public DataEntity(string key)
    {
        Key = key;
    }

    public int Id { get; set; }
    public string Key { get; set; }
    public string? Value { get; set; }
}

public class InnerExceptionEntity
{
    public InnerExceptionEntity(ExceptionEntity innerException)
    {
        InnerException = innerException;
    }

    public int Id { get; set; }
    public ExceptionEntity InnerException { get; set; }
}

public class ExceptionCache
{
    private readonly Dictionary<string, ExceptionEntity> _cache = new Dictionary<string, ExceptionEntity>();
    private int _currentId = 1;

    public ExceptionEntity GetOrAdd(Exception ex)
    {
        var exceptionEntity = MapExceptionToEntity(ex);

        if (_cache.TryGetValue(exceptionEntity.Hash, out var existingException))
        {
            return existingException;
        }

        // Assign a new ID to the exception and add it to the cache
        exceptionEntity.Id = _currentId++;
        _cache[exceptionEntity.Hash] = exceptionEntity;

        return exceptionEntity;
    }

    private ExceptionEntity MapExceptionToEntity(Exception ex)
    {
        var exceptionEntity = new ExceptionEntity
        {
            Message = ex.Message,
            Source = ex.Source,
            TargetSite = ex.TargetSite,
            HelpLink = !string.IsNullOrEmpty(ex.HelpLink) ? new Uri(ex.HelpLink) : null,
            HResult = ex.HResult,
        };

        if (!string.IsNullOrEmpty(ex.StackTrace))
        {
            exceptionEntity.StackTraces = ex.StackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Select(line => new StackTraceEntity(line)).ToList();
        }

        foreach (var key in ex.Data.Keys)
        {
            exceptionEntity.DataEntries.Add(new DataEntity(key.ToString()) { Value = ex.Data[key]?.ToString() });
        }

        exceptionEntity.GenerateHash();

        if (ex.InnerException != null)
        {
            var innerExceptionEntity = MapExceptionToEntity(ex.InnerException);
            exceptionEntity.InnerException = new InnerExceptionEntity(innerExceptionEntity);
        }

        return exceptionEntity;
    }
}
