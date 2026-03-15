using VideoDetectionPOC.Models.Dtos;
using VideoDetectionPOC.ViewModel;

namespace VideoDetectionPOC.Services
{
    public interface IAdvancedSearchService
    {
        Task<List<FrameDto>> SearchFrames(AdvancedSearchViewModel request);
    }
}
