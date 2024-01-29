namespace FileSyncAgent.Extensions;

public static class ArrayExtensions
{
    public static string ToUtf8String(this byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace("-","");
    }
}
