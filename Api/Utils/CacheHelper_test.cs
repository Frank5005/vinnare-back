using Api.Utils;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

public class CacheHelper_test
{
    private readonly IMemoryCache _memoryCache;
    private readonly CacheHelper _cacheHelper;

    public CacheHelper_test()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _cacheHelper = new CacheHelper(_memoryCache);
    }

    [Fact]
    public void TryGetValue_ShouldReturnTrue_WhenKeyExists()
    {
        // Arrange
        var expected = new List<string> { "A", "B" };
        _memoryCache.Set("test_key", expected);

        // Act
        var found = _cacheHelper.TryGetValue<string>("test_key", out var result);

        // Assert
        Assert.True(found);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TryGetValue_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        // Act
        var found = _cacheHelper.TryGetValue<string>("missing_key", out var result);

        // Assert
        Assert.False(found);
        Assert.Null(result);
    }

    [Fact]
    public void Set_ShouldStoreValueInCache_WithDefaultExpiration()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };

        // Act
        _cacheHelper.Set<int>("my_key", list);

        // Assert
        var found = _memoryCache.TryGetValue("my_key", out List<int> result);
        Assert.True(found);
        Assert.Equal(list, result);
    }

    [Fact]
    public void RemoveKeys_ShouldRemoveFromCache()
    {
        // Arrange
        _memoryCache.Set("k1", "value1");
        _memoryCache.Set("k2", "value2");

        // Act
        _cacheHelper.RemoveKeys("k1", "k2");

        // Assert
        Assert.False(_memoryCache.TryGetValue("k1", out _));
        Assert.False(_memoryCache.TryGetValue("k2", out _));
    }
}
