namespace Rce2;

public static class Rce2StringListExtensions
{
    public static string GetValue(this List<string> list, string key)
    {
        try
        {
            for (var i = 0; i < list.Count - 1; i += 2)
            {
                if (list[i] == key)
                {
                    return list[i + 1];
                }
            }

            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}
