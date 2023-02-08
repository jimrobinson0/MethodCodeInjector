public class MethodInfo
{
    public bool IsParseSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public int StartBraceIndex { get; set; }
    public int EndBraceIndex { get; set; }
    public string Indentation { get; set; }

    public MethodInfo()
    {
        Indentation = string.Empty;
    }
}