namespace Streetcode.Shared.Extensions;

public static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> enumeration)
    {
        foreach (var item in enumeration)
        {
            yield return item;
        }

        await Task.CompletedTask;
    }
}