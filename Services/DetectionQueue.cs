using System.Collections.Concurrent;

namespace VideoDetectionPOC.Services
{
    public class DetectionQueue : IDetectionQueue
    {
        private readonly ConcurrentQueue<string> _queue = new();
        public void Enqueue(string videoPath)
        {
            _queue.Enqueue(videoPath);
        }

        public bool TryDequeue(out string? videoPath)
        {
            return _queue.TryDequeue(out videoPath);
        }
    }
}
