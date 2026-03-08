using VideoDetectionPOC.Models.Dtos;
using VideoDetectionPOC.ViewModel;

namespace VideoDetectionPOC.Services
{
    public interface ISearchService
    {
        Task<List<VideoSearchResultDto>> SearchAsync(DetectionSearchViewModel request);

        Task<List<TimelineDto>> GetTimelineAsync(Guid videoId);

        Task<SearchSummaryDto> GetSummaryAsync(DetectionSearchViewModel request);
    }
}
