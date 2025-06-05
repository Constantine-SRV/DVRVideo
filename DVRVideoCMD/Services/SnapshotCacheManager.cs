public static class SnapshotCacheManager
{
    /// <summary>
    /// Получить путь к актуальному (или создать новый) снимку камеры (с кропом или без)
    /// </summary>
    /// <param name="channel">номер канала</param>
    /// <param name="cacheSeconds">время актуальности кэша, сек</param>
    /// <param name="cropArea">null - полный снимок; иначе - область кропа</param>
    /// <returns>Путь к jpg-файлу</returns>
    public static string GetOrCreateSnapshot(int channel, int cacheSeconds = 10, (int x1, int y1, int x2, int y2)? cropArea = null)
    {
        string baseFileName = $"ch_{channel:D2}.jpg";

        string tempFileName = cropArea == null
            ? FileHelper.GetTempFullPath(baseFileName)
            : FileHelper.GetTempCropPath(baseFileName);

        // 1. Проверяем, есть ли уже свежий файл
        if (File.Exists(tempFileName))
        {
            var fi = new FileInfo(tempFileName);
            var age = DateTime.Now - fi.LastWriteTime;
            if (age.TotalSeconds < cacheSeconds)
            {
                Console.WriteLine($"Reusing cached file: {tempFileName}");
                return tempFileName;
            }
            else
            {
                try { File.Delete(tempFileName); } catch { }
            }
        }

        // 2. Сохраняем полный снимок с камеры (оригинал) СРАЗУ в tmp
        string fullPath = FileHelper.GetTempFullPath(baseFileName);
        if (!CameraSnapshotService.SaveSnapshot(channel, fullPath))
            return null;

        // 3. Возвращаем либо полный путь, либо делаем crop
        if (cropArea == null)
        {
            return fullPath; // Не надо File.Copy!
        }
        else
        {
            return ImageCropper.CropAndSave(fullPath, cropArea.Value.x1, cropArea.Value.y1, cropArea.Value.x2, cropArea.Value.y2, saveToHistory: true);
        }
    }

}
