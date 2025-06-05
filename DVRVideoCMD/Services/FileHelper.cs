public static class FileHelper
{
    public static string TempDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmp");
    public static string HistoryDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "history");

    static FileHelper()
    {
        Directory.CreateDirectory(TempDir);
        Directory.CreateDirectory(HistoryDir);
    }

    public static string GetTempCropPath(string baseFileName)
    {
        string name = Path.GetFileNameWithoutExtension(baseFileName);
        string ext = Path.GetExtension(baseFileName);
        return Path.Combine(TempDir, $"{name}_crop{ext}");
    }

    public static string GetTempFullPath(string baseFileName)
    {
        string name = Path.GetFileNameWithoutExtension(baseFileName);
        string ext = Path.GetExtension(baseFileName);
        return Path.Combine(TempDir, $"{name}_full{ext}");
    }

    public static string GetHistoryCropPath(string baseFileName)
    {
        string name = Path.GetFileNameWithoutExtension(baseFileName);
        string ext = Path.GetExtension(baseFileName);
        string ts = DateTime.Now.ToString("_yyyy_MM_dd_HH_mm_ss");
        return Path.Combine(HistoryDir, $"{name}{ts}_crop{ext}");
    }
}
