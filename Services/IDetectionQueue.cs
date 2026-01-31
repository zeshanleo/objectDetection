namespace VideoDetectionPOC.Services
{
    public interface IDetectionQueue
    {
        void Enqueue(string videoPath);
        bool TryDequeue(out string? videoPath);
    }
}
