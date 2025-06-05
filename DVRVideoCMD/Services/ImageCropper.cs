using OpenCvSharp;

public static class ImageCropper
{
    public static string CropAndSave(
        string imagePath,
        int x1, int y1, int x2, int y2,
        bool saveToHistory = false
    )
    {
        using (var src = Cv2.ImRead(imagePath))
        {
            x1 = Math.Max(0, x1);
            y1 = Math.Max(0, y1);
            x2 = Math.Min(src.Width - 1, x2);
            y2 = Math.Min(src.Height - 1, y2);

            int width = x2 - x1 + 1;
            int height = y2 - y1 + 1;

            if (width <= 0 || height <= 0)
            {
                Console.WriteLine("Invalid crop area!");
                return null;
            }

            Rect cropRect = new Rect(x1, y1, width, height);
            using (var cropped = new Mat(src, cropRect))
            {
                string tempPath = FileHelper.GetTempCropPath(imagePath);

                // Просто перезаписываем файл (если был — затрётся)
                Cv2.ImWrite(tempPath, cropped);

                if (saveToHistory)
                {
                    string histPath = FileHelper.GetHistoryCropPath(imagePath);
                    File.Copy(tempPath, histPath, true);
                    Console.WriteLine($"History crop saved: {histPath}");
                }

                Console.WriteLine($"Temp crop saved: {tempPath} ({width}x{height})");
                return tempPath;
            }
        }
    }
}
