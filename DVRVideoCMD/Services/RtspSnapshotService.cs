using OpenCvSharp;
using System;
using System.Linq;

/// <summary>
/// Captures a single frame from the DVR RTSP stream or from an IP camera (channel > 16).
/// </summary>
public static class RtspSnapshotService
{
    public static string BaseRtspUrl = AppSettingsService.BaseRtspUrl;

    public static bool SaveSnapshot(int channel, string fileName)
    {
        string rtspUrl;

        // 1–16: старый DVR, >16 — отдельные IP-камеры из AppSettingsService.IPCamList
        if (channel >= 1 && channel <= 16)
        {
            rtspUrl = string.Format(BaseRtspUrl, channel);
        }
        else
        {
            // Ищем ссылку для IP-камеры в AppSettingsService.IPCamList
            var ipCam = AppSettingsService.IPCamList?.FirstOrDefault(c => c.Channel == channel);
            if (ipCam == null)
            {
                Console.WriteLine($"No IP camera found for channel {channel}!");
                return false;
            }
            rtspUrl = ipCam.Link;
        }

        using (var capture = new VideoCapture(rtspUrl))
        {
            if (!capture.IsOpened())
            {
                Console.WriteLine($"Failed to open stream for channel {channel:D2} ({rtspUrl}).");
                return false;
            }
            using (var frame = new Mat())
            {
                if (capture.Read(frame) && !frame.Empty())
                {
                    int width = (int)capture.FrameWidth;
                    int height = (int)capture.FrameHeight;
                    Console.WriteLine($"Channel {channel:D2}: {width}x{height}");

                    // Если картинка "квадратная" — растягиваем в 16:9
                    if ((float)width / height < 1.3)
                    {
                        Cv2.Resize(frame, frame, new OpenCvSharp.Size(1920, 1080));
                        Console.WriteLine($"  Channel {channel:D2} auto-stretched to 1920x1080 (16:9)");
                    }

                    Cv2.ImWrite(fileName, frame);
                    Console.WriteLine($"  Snapshot saved as {fileName}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Failed to capture frame for channel {channel:D2}.");
                    return false;
                }
            }
        }
    }
}
