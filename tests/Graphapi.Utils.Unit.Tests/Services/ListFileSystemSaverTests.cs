using FluentAssertions;
using Graphapi.Utils.Models;
using Graphapi.Utils.Services;
using LanguageExt.UnitTesting;
using Moq;
using NUnit.Framework;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;

namespace Graphapi.Utils.Unit.Tests.Services;

[TestFixture]
public class ListFileSystemSaverTests
{
    private int[] _testData;
    private ListDownloaderOptions _options;
    private MockFileSystem _mockFileSystem;
    private ListFileSystemSaver<int> _sut;

    [SetUp]
    public void SetUp()
    {
        _testData = new[] { 1, 2 };
        _options = new ListDownloaderOptions { Directory = "./dir" };
        _mockFileSystem = new MockFileSystem();
        _sut = new ListFileSystemSaver<int>(
            _mockFileSystem);
    }

    [Test]
    public async Task SaveAsync_Data_ShouldCreateDirectory()
    {
        await _sut.SaveAsync(_testData, _ => $"{_}.json", _options);

        _mockFileSystem
            .AllDirectories
            .Should()
            .Contain(GetMockFullPath());
    }

    [Test]
    public async Task SaveAsync_Data_ShouldSaveFiles()
    {
        await _sut.SaveAsync(_testData, _ => $"{_}.json", _options);

        _mockFileSystem
            .File
            .ReadAllText(_mockFileSystem.Path.Combine(GetMockFullPath(), "1.json"))
            .Should()
            .Be(JsonSerializer.Serialize(1));
        _mockFileSystem
            .File
            .ReadAllText(_mockFileSystem.Path.Combine(GetMockFullPath(), "2.json"))
            .Should()
            .Be(JsonSerializer.Serialize(2));
    }

    [Test]
    public async Task SaveAsync_Data_ReturnFilePaths()
    {
        var result = await _sut.SaveAsync(_testData, _ => $"{_}.json", _options);

        result
            .ShouldBeRight(_ =>
                _
                    .Should()
                    .BeEquivalentTo(
                        _mockFileSystem.Path.Combine(GetMockFullPath(), "1.json"),
                        _mockFileSystem.Path.Combine(GetMockFullPath(), "2.json")));
    }

    [Test]
    public async Task SaveAsync_Data_SomethingThrows()
    {
        var mockFileSystem = new Mock<IFileSystem>();
        var mockDirectory = new Mock<IDirectory>();
        var expectedEx = new Exception();
        mockFileSystem
            .Setup(_ => _.Directory)
            .Returns(mockDirectory.Object);
        mockDirectory
            .Setup(_ => _.CreateDirectory(It.IsAny<string>()))
            .Throws(expectedEx);
        _sut = new ListFileSystemSaver<int>(mockFileSystem.Object);

        var result = await _sut.SaveAsync(_testData, _ => $"{_}.json", _options);

        result.ShouldBeLeft(err => err.ToException().Should().Be(expectedEx));
    }

    private string GetMockFullPath() => _mockFileSystem.Path.GetFullPath(_options.Directory);
}