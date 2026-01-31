using System.Linq;
using VideoDetectionPOC.Models;
using VideoDetectionPOC.Services;
using Xabe.FFmpeg;

namespace VideoDetectionPOC.DataAccess
{
    public class DetectionRepository
    {
        private readonly ApplicationDBContext _db;
        public DetectionRepository(ApplicationDBContext db)
        {
            _db = db;
        }

        public bool DetectionExists(string fileName)
        {
            var video = _db.Videos.Where(v => v.FileName.Equals(fileName)).FirstOrDefault();
            if (video == null)
            {
                return false;
            }

            var detections = _db.Detections.Where(d => d.VideoId == video.Id).ToList();
            return detections.Count > 0;
        }

        public void AddVideo(string videoFile)
        {
            var videoFileName = Path.GetFileName(videoFile);
            var video = _db.Videos.Where(v => v.FileName.Equals(videoFileName)).FirstOrDefault();
            if (video == null)
            {
                video = new Models.Video
                {
                    FileName = videoFileName,
                    Processed = false,
                    UploadedAt = File.GetCreationTime(videoFile)
                };
                _db.Videos.Add(video);
                _db.SaveChanges();
            }
        }

        public void UpdateVideo(string videoFile, bool processed)
        {
            var videoFileName = Path.GetFileName(videoFile);
            var video = _db.Videos.Where(v => v.FileName.Equals(videoFileName)).FirstOrDefault();
            if (video != null)
            {
                video.Processed = processed;
                _db.SaveChanges();
            }
        }

        public List<Video> GetAllVideos()
        {
            return _db.Videos.ToList();
        }

        public void AddDetection(string videoFile, string frameFile, List<Services.Detection> detections)
        {
            var videoFileName = Path.GetFileName(videoFile);
            var frameFileName = Path.GetFileName(frameFile);

            var video = _db.Videos.Where(v => v.FileName.Equals(videoFileName)).FirstOrDefault();
            if (video == null)
            {
                return;
            }

            var frame = _db.Frames.Where(f => f.FrameName.Equals(frameFileName)).FirstOrDefault();
            if (frame == null)
            {
                frame = new Frame
                {
                    VideoId = video.Id,
                    FrameIndex = int.Parse(Path.GetFileNameWithoutExtension(frameFile).Split('_').Last()),
                    FrameName = frameFileName,
                    FramePath = frameFile
                };
                _db.Frames.Add(frame);
                _db.SaveChanges();
            }

            List<Models.Detection> detations = new List<Models.Detection>();
            foreach (var detection in detections)
            {
                var det = new Models.Detection
                {
                    VideoId = video.Id,
                    FrameId = frame.Id,
                    Label = OnnxYoloDetector.cocoLabels[detection.ClassId],
                    Confidence = detection.Confidence,
                    X1 = (int)detection.X1,
                    Y1 = (int)detection.Y1,
                    X2 = (int)detection.X2,
                    Y2 = (int)detection.Y2,
                    ObjectTypeId = detection.ClassId + 1,
                };
                detations.Add(det);
            }
            _db.Detections.AddRange(detations);
            _db.SaveChanges();
        }

        public void DeleteVideoData(string videoFile)
        {
            var videoFileName = Path.GetFileName(videoFile);

            // Step 1: Get the video record
            var video = _db.Videos.FirstOrDefault(v => v.FileName.Equals(videoFileName));

            if (video == null)
            {
                return; // Nothing to delete
            }

            // Step 2: Get related frames
            var frames = _db.Frames.Where(f => f.VideoId == video.Id).ToList();

            // Step 3: Delete detections for each frame
            var detections = _db.Detections.Where(v => v.VideoId == video.Id).ToList();

            _db.Detections.RemoveRange(detections);
            _db.SaveChanges();

            // Step 4: Delete frames
            _db.Frames.RemoveRange(frames);
            _db.SaveChanges();

            // Step 5: Delete the video itself
            _db.Videos.Remove(video);
            _db.SaveChanges();
        }
    }
}
