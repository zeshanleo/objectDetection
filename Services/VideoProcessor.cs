
using Microsoft.AspNetCore.SignalR;
using VideoDetectionPOC.DataAccess;
using VideoDetectionPOC.Hub;

namespace VideoDetectionPOC.Services
{
    public class VideoProcessor : BackgroundService
    {
        private readonly IDetectionQueue _detectionQueue;
        private readonly IHubContext<DetectionHub> _hub;
        private readonly OnnxYoloDetector _detector;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _framesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "frames");

        public VideoProcessor(IDetectionQueue detectionQueue, IHubContext<DetectionHub> hub, OnnxYoloDetector detector, IServiceScopeFactory scopeFactory)
        {
            _detectionQueue = detectionQueue;
            _hub = hub;
            _detector = detector;
            _scopeFactory = scopeFactory; 
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessNextVideo(stoppingToken);
                await Task.Delay(500); // Simulate time-consuming processing
            }
        }

        private async Task ProcessNextVideo(CancellationToken stoppingToken)
        {
            // Placeholder for video processing logic
            string? videoPath = String.Empty;
            if ((_detectionQueue != null) && 
                (_detectionQueue.TryDequeue(out videoPath) &&
                (!string.IsNullOrEmpty(videoPath)) &&
                (!string.IsNullOrEmpty(_framesPath))))
            {
                await _hub.Clients.All.SendAsync("ReceiveMessage", $"Started processing video: {Path.GetFileName(videoPath)}", cancellationToken: stoppingToken);
                
                var _detectionRepository = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<DetectionRepository>();

                if (_detectionRepository.DetectionExists(Path.GetFileName(videoPath)))
                {
                    await _hub.Clients.All.SendAsync("ReceiveMessage", $"Detections already exist for video: {Path.GetFileName(videoPath)}. Skipping processing.", cancellationToken: stoppingToken);
                    return;
                }
                var timeSpan = FrameExtractor.GetVideoDuration(videoPath).Result;
                // Add video entry to database
                _detectionRepository.AddVideo(videoPath, timeSpan);

                var frames = FrameExtractor.ExtractFrames(videoPath, _framesPath, 1);
                await _hub.Clients.All.SendAsync("ReceiveMessage", $"File: {Path.GetFileName(videoPath)} converted into {frames.Result.Length} frames.", cancellationToken: stoppingToken);
                
                string[] framesList = frames.Result;
                int totalFrames = framesList.Length;
                for (int i = 0; i < totalFrames; i++)
                {
                    _detector.ProcessFrame(videoPath, framesList[i]);
                    int percent = (i * 100) / totalFrames;
                    await _hub.Clients.All.SendAsync("ReceiveProgress", new
                    {
                        videoName = Path.GetFileName(videoPath),
                        progress = percent,
                        status = "Processing"
                    });
                }
                _detectionRepository.UpdateVideo(videoPath, true);
                await _hub.Clients.All.SendAsync("ReceiveProgress", new
                {
                    videoName = Path.GetFileName(videoPath),
                    progress = 100,
                    status = "Completed"
                });
            }
        }
    }
}
