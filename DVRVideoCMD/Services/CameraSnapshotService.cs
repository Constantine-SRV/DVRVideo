using OpenCvSharp;

public static class CameraSnapshotService
{
    public static string BaseRtspUrl =  AppSettingsService.BaseRtspUrl;
        //"rtsp://user:!qaz2wsx3edc@192.168.0.252:554/cam/realmonitor?channel={0}&subtype=0";

    public static bool SaveSnapshot(int channel, string fileName)
    {
        string rtspUrl = string.Format(BaseRtspUrl, channel);
        using (var capture = new VideoCapture(rtspUrl))
        {
            if (!capture.IsOpened())
            {
                Console.WriteLine($"Failed to open stream for channel {channel:D2}.");
                return false;
            }
            using (var frame = new Mat())
            {
                if (capture.Read(frame) && !frame.Empty())
                {
                    int width = (int)capture.FrameWidth;
                    int height = (int)capture.FrameHeight;
                    Console.WriteLine($"Channel {channel:D2}: {width}x{height}");

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
