namespace EasyTaskRunner.Tests.TaskRunner;

using System;
using System.Linq;
using EasyTaskRunner.Data.Utilities;
using Xunit;
using Xunit.Abstractions;
public class ExceptionEntityTests
{
    private readonly ITestOutputHelper testOutputHelper;

    public ExceptionEntityTests(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Constructor_Initializes_Collections()
    {
        // Arrange & Act
        var exceptionEntity = new ExceptionEntity();

        // Assert
        Assert.NotNull(exceptionEntity.StackTraces);
        Assert.NotNull(exceptionEntity.DataEntries);
        Assert.Empty(exceptionEntity.StackTraces);
        Assert.Empty(exceptionEntity.DataEntries);
    }

    [Fact]
    public void GenerateHash_Creates_Correct_Hash()
    {
        // Arrange
        var exceptionEntity = new ExceptionEntity
        {
            Message = "Test Message",
            Source = "Test Source",
            TargetSite = typeof(ExceptionEntityTests).GetMethod(nameof(GenerateHash_Creates_Correct_Hash)),
            HelpLink = new Uri("http://example.com")
        };

        exceptionEntity.StackTraces.Add(new StackTraceEntity("Test Stack Trace"));

        // Act
        exceptionEntity.GenerateHash();

        // Recreate the dataToHash string used in the GenerateHash method
        var expectedDataToHash = $"{exceptionEntity.Message ?? ""}{exceptionEntity.Source ?? ""}{exceptionEntity.TargetSite?.Name ?? ""}{exceptionEntity.HelpLink?.ToString() ?? ""}";

        if (exceptionEntity.StackTraces.Count > 0)
        {
            expectedDataToHash += exceptionEntity.StackTraces.First().Line;
        }

        // Print the dataToHash for debugging purposes
        testOutputHelper.WriteLine($"Data to hash: {expectedDataToHash}");

        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var expectedHash = Convert.ToBase64String(sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(expectedDataToHash)));

            // Assert
            Assert.Equal(expectedHash, exceptionEntity.Hash);
        }
    }


    [Fact]
    public void MapExceptionToEntity_Correctly_Maps_Exception()
    {
        // Arrange
        var exception = new Exception("Test exception")
        {
            Source = "Test source",
            HelpLink = "http://example.com"
        };
        exception.Data["Key1"] = "Value1";

        // Act
        var exceptionCache = new ExceptionCache();
        var exceptionEntity = exceptionCache.GetOrAdd(exception);

        // Assert
        Assert.Equal("Test exception", exceptionEntity.Message);
        Assert.Equal("Test source", exceptionEntity.Source);
        Assert.Equal(new Uri("http://example.com"), exceptionEntity.HelpLink);
        Assert.Equal(exception.HResult, exceptionEntity.HResult);
        Assert.Single(exceptionEntity.DataEntries);
        Assert.Equal("Key1", exceptionEntity.DataEntries.First().Key);
        Assert.Equal("Value1", exceptionEntity.DataEntries.First().Value);
    }

    [Fact]
    public void GetOrAdd_Returns_Same_Entity_For_Same_Exception()
    {
        // Arrange
        var exception = new Exception("Test exception");

        var exceptionCache = new ExceptionCache();

        // Act
        var firstEntity = exceptionCache.GetOrAdd(exception);
        var secondEntity = exceptionCache.GetOrAdd(exception);

        // Assert
        Assert.Equal(firstEntity.Id, secondEntity.Id);
        Assert.Equal(firstEntity.Hash, secondEntity.Hash);
    }

    [Fact]
    public void GetOrAdd_Returns_Different_Entity_For_Different_Exceptions()
    {
        // Arrange
        var firstException = new Exception("First exception");
        var secondException = new Exception("Second exception");

        var exceptionCache = new ExceptionCache();

        // Act
        var firstEntity = exceptionCache.GetOrAdd(firstException);
        var secondEntity = exceptionCache.GetOrAdd(secondException);

        // Assert
        Assert.NotEqual(firstEntity.Id, secondEntity.Id);
        Assert.NotEqual(firstEntity.Hash, secondEntity.Hash);
    }
}
