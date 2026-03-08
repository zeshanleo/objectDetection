using Microsoft.EntityFrameworkCore;
using System;
using VideoDetectionPOC.DataAccess;
using VideoDetectionPOC.Models.Dtos;
using VideoDetectionPOC.ViewModel;

namespace VideoDetectionPOC.Services
{
    public class SearchService : ISearchService
    {
        private readonly ApplicationDBContext _context;

        public SearchService(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<VideoSearchResultDto>> SearchAsync(DetectionSearchViewModel request)
        {
            var query = _context.Detections
                .AsNoTracking()
                .Include(d => d.Frame)
                    .ThenInclude(f => f.Video)
                .Include(d => d.ObjectType)
                .AsQueryable();

            if (request.SelectedObjectTypeIds != null && request.SelectedObjectTypeIds.Any())
            {
                query = query.Where(d => request.SelectedObjectTypeIds.Contains(d.ObjectTypeId));
            }

            if (request.MinConfidence.HasValue)
                query = query.Where(d => d.Confidence >= request.MinConfidence.Value);

            if (request.FromDate.HasValue)
                query = query.Where(d => d.Frame.Video.UploadedAt >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                query = query.Where(d => d.Frame.Video.UploadedAt <= request.ToDate.Value);

            if (!string.IsNullOrWhiteSpace(request.VideoName))
                query = query.Where(d => d.Frame.Video.FileName.Contains(request.VideoName));

            if (request.FromSecond.HasValue)
                query = query.Where(d => d.Frame.TimestampMs >= request.FromSecond.Value);

            if (request.ToSecond.HasValue)
                query = query.Where(d => d.Frame.TimestampMs <= request.ToSecond.Value);

            var grouped = await query
                .GroupBy(d => d.Frame.Video)
                .Select(g => new VideoSearchResultDto
                {
                    VideoId = g.Key.Id,
                    VideoName = g.Key.FileName,
                    UploadDate = g.Key.UploadedAt.DateTime,
                    TotalDetections = g.Count(),
                    DistinctObjects = g.Select(x => x.ObjectType.Name).Distinct().Count(),
                    AvgConfidence = g.Average(x => x.Confidence)
                })
                .OrderByDescending(x => x.TotalDetections)
                .ToListAsync();

            return grouped;
        }
        //public async Task<List<VideoSearchResultDto>> SearchAsync(DetectionSearchViewModel request)
        //{
        //    throw new NotImplementedException();
        //    //var query = BuildBaseQuery(request);

        //    //var result = await query
        //    //    .GroupBy(d => new
        //    //    {
        //    //        d.Frame.Video.Id,
        //    //        d.Frames.Video.FileName,
        //    //        d.Frames.Video.UploadedAt
        //    //    })
        //    //    .Select(g => new VideoSearchResultDto
        //    //    {
        //    //        VideoId = g.Key.Id,
        //    //        VideoName = g.Key.FileName,
        //    //        UploadDate = g.Key.UploadedAt,
        //    //        TotalDetections = g.Count(),
        //    //        DistinctObjects = g.Select(x => x.ObjectTypeId).Distinct().Count(),
        //    //        AvgConfidence = g.Average(x => x.Confidence),
        //    //        MaxConfidence = g.Max(x => x.Confidence),
        //    //        TotalFramesWithDetections = g.Select(x => x.FrameId).Distinct().Count()
        //    //    })
        //    //    .OrderByDescending(x => x.TotalDetections)
        //    //    .ToListAsync();

        //    //return result;
        //}

        public async Task<SearchSummaryDto> GetSummaryAsync(DetectionSearchViewModel request)
        {
            var results = await SearchAsync(request);

            return new SearchSummaryDto
            {
                TotalVideos = results.Count,
                TotalDetections = results.Sum(x => x.TotalDetections),
                TotalDistinctObjects = results.Sum(x => x.DistinctObjects)
            };
        }

        public async Task<List<TimelineDto>> GetTimelineAsync(Guid videoId)
        {
            return await _context.Frames
                .AsNoTracking()
                .Where(f => f.VideoId == videoId)
                .Select(f => new TimelineDto
                {
                    Second = f.TimestampMs,
                    DetectionCount = f.Detections.Count()
                })
                .OrderBy(x => x.Second)
                .ToListAsync();
        }

        private IQueryable<Detection> BuildBaseQuery(DetectionSearchViewModel request)
        {
            var query = _context.Detections
                .AsNoTracking()
                .AsQueryable();

            if (request.SelectedObjectTypeIds != null && request.SelectedObjectTypeIds.Any())
                query = query.Where(d => request.SelectedObjectTypeIds.Contains(d.ObjectTypeId));

            if (request.MinConfidence.HasValue)
                query = query.Where(d => d.Confidence >= request.MinConfidence.Value);

            if (request.FromDate.HasValue)
                query = query.Where(d => d.Frame.Video.UploadedAt >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                query = query.Where(d => d.Frame.Video.UploadedAt <= request.ToDate.Value);

            if (!string.IsNullOrWhiteSpace(request.VideoName))
                query = query.Where(d => d.Frame.Video.FileName.Contains(request.VideoName));

            if (request.FromSecond.HasValue)
                query = query.Where(d => d.Frame.TimestampMs >= request.FromSecond.Value);

            if (request.ToSecond.HasValue)
                query = query.Where(d => d.Frame.TimestampMs <= request.ToSecond.Value);

            return (IQueryable<Detection>)query;
        }

        public Task<List<TimelineDto>> GetTimelineAsync(int videoId)
        {
            throw new NotImplementedException();
        }
    }
}
