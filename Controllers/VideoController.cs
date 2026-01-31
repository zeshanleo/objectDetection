using Microsoft.AspNetCore.Mvc;
using VideoDetectionPOC.DataAccess;
using VideoDetectionPOC.Services;
using VideoDetectionPOC.ViewModel;

namespace VideoDetectionPOC.Controllers
{
    public class VideoController : Controller
    {
        private readonly string _vidoesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "videos");
        private readonly string _framesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "frames");
        private readonly string _framesOutputPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "framesoutput");
        private readonly IDetectionQueue _detectionQueue;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public VideoController(IDetectionQueue detectionQueue, IServiceScopeFactory serviceScopeFactory)
        {
            _detectionQueue = detectionQueue;
            _serviceScopeFactory = serviceScopeFactory;
        }
        public IActionResult Index()
        {
            var _detectionRepository = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<DetectionRepository>();
            var videosFromDb = _detectionRepository.GetAllVideos();

            // Load all video files from directory
            var files = Directory.GetFiles(_vidoesPath, "*.mp4");
            var videoList = new List<VideoItemViewModel>();

            // 1️ First: Add videos from database and check file existence
            foreach (var dbVideo in videosFromDb)
            {
                var filePath = files.FirstOrDefault(f => Path.GetFileName(f).Equals(dbVideo.FileName));
                bool exists = !string.IsNullOrEmpty(filePath) && Path.Exists(filePath);

                videoList.Add(new VideoItemViewModel
                {
                    FileName = dbVideo.FileName,
                    Status = exists ? dbVideo.Processed ? "Completed" : "Pending" : "Not found",
                    UploadedAt = exists ? System.IO.File.GetCreationTime(filePath) : DateTime.Now,
                    TotalFrames = 0,
                    DetectedObjects = 0
                });
            }

            // 2️ Second: Add videos that exist in directory but NOT in database
            var dbVideoNames = videosFromDb.Select(v => v.FileName).ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                if (!dbVideoNames.Contains(fileName)) // NOT IN DATABASE → ADD IT
                {
                    videoList.Add(new VideoItemViewModel
                    {
                        FileName = fileName,
                        Status = "Pending",   // Or "Not Processed"
                        UploadedAt = System.IO.File.GetCreationTime(file),
                        TotalFrames = 0,
                        DetectedObjects = 0
                    });
                }
            }

            return View(videoList);
        }

        [HttpPost]
        [RequestSizeLimit(1073741824)]
        public async Task<IActionResult> UploadVideo(IFormFile videoFile)
        {
            if (videoFile == null || videoFile.Length == 0)
                return RedirectToAction("Index");

            if (!Directory.Exists(_vidoesPath))
                Directory.CreateDirectory(_vidoesPath);

            var filePath = Path.Combine(_vidoesPath, videoFile.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await videoFile.CopyToAsync(stream);
            }
            _detectionQueue.Enqueue(filePath);
            return RedirectToAction("Index");
        }

        public IActionResult Details(string fileName)
        {
            var frames = Directory.Exists(_framesOutputPath)
                ? Directory.GetFiles(_framesOutputPath, $"{Path.GetFileNameWithoutExtension(fileName)}_*.jpg").Select(x => Path.GetFileName(x)).ToList()
                : new List<string>();

            ViewBag.Frames = frames;
            ViewBag.VideoName = fileName;

            return View();
        }

        public IActionResult Delete(string fileName)
        {
            var _detectionRepository = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<DetectionRepository>();
            _detectionRepository.DeleteVideoData(fileName);

            var baseName = Path.GetFileNameWithoutExtension(fileName);
            
            var videoFilePath = Path.Combine(_vidoesPath, fileName);
            System.IO.File.Delete(videoFilePath);

            var frameFiles = Directory.GetFiles(_framesPath, $"{baseName}*.jpg");
            foreach (var frame in frameFiles)
            {
                try
                {
                    System.IO.File.Delete(frame);
                    Console.WriteLine($"Deleted frame: {frame}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to delete {frame}: {ex.Message}");
                }
            }

            var frameOutputFiles = Directory.GetFiles(_framesOutputPath, $"{baseName}*.jpg");
            foreach (var outPutFrame in frameOutputFiles)
            {
                try
                {
                    System.IO.File.Delete(outPutFrame);
                    Console.WriteLine($"Deleted frame: {outPutFrame}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to delete {outPutFrame}: {ex.Message}");
                }
            }

            return RedirectToAction("Index");
        }

        private bool IsProcessed(string fileName)
        {
            var frames = Directory.Exists(_framesOutputPath)
                ? Directory.GetFiles(_framesOutputPath, $"{Path.GetFileNameWithoutExtension(fileName)}_*.jpg")
                : Array.Empty<string>();
            return frames.Length > 0;
        }
    }
}
