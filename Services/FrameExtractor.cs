using Xabe.FFmpeg;

namespace VideoDetectionPOC.Services
{
    public class FrameExtractor
    {
        private static readonly string FFMPegPath = @"C:\Program Files\ffmpeg-8.0-full_build\ffmpeg-8.0-full_build\bin";
        public static async Task<string[]> ExtractFrames(string videoPath, string framesPath, double fps = 1)
        {
            FFmpeg.SetExecutablesPath(FFMPegPath);

            // Extract frames from video using FFmpeg
            Console.WriteLine("Extracting frames from video...");
            string framePattern = Path.Combine(framesPath, $"{Path.GetFileNameWithoutExtension(videoPath)}_%05d.jpg");

            await FFmpeg.Conversions.New()
                .AddParameter($"-i \"{videoPath}\"")
                .AddParameter($"-vf \"fps={fps}\"")
                .AddParameter($"\"{framePattern}\"")
                .Start();

            // Get all frame files
            var frameFiles = Directory.GetFiles(framesPath, $"{Path.GetFileNameWithoutExtension(videoPath)}_*.jpg")
                                      .OrderBy(f => f)
                                      .ToArray();
            return frameFiles;
        }
        public static async Task<TimeSpan> GetVideoDuration(string videoPath)
        {
            FFmpeg.SetExecutablesPath(FFMPegPath);
            var mediaInfo = await FFmpeg.GetMediaInfo(videoPath);

            return mediaInfo.Duration;
        }
    }
}
