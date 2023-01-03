using MethodResultCaching.CacheService;

var sample = new MethodResultCacheSample();

var monthsFromOrigin = await sample.GetMonthsAsync(5);
// Data retrieved from original method: Jan, Feb, Mar, Apr, May

var monthsFromCache = await sample.GetMonthsAsync(5);
// Data is retrieved from cache: Jan, Feb, Mar, Apr, May

// compare and validate the values are the same for different resources
Console.WriteLine(string.Compare(monthsFromOrigin, monthsFromCache) == 0); //true

public class MethodResultCacheSample
{ 
    [CachedResult]
    public Task<string> GetMonthsAsync(int monthIndex)
    {
        var values = new string[monthIndex];

        for (int i = 1; i <= values.Length; i++)
        {
            values[i - 1] = new DateTime(2020, i, 1).ToString("MMM");
        }

        var result = string.Join(", ", values);

        Console.WriteLine($"Data retrieved from original method: {result}");

        return Task.FromResult(result);
    }
}
