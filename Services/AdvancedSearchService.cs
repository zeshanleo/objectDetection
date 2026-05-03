using Microsoft.EntityFrameworkCore;
using VideoDetectionPOC.DataAccess;
using VideoDetectionPOC.Models;
using VideoDetectionPOC.Models.Dtos;
using VideoDetectionPOC.ViewModel;

namespace VideoDetectionPOC.Services
{
    public class AdvancedSearchService : IAdvancedSearchService
    {
        private readonly ApplicationDBContext _context;
        public AdvancedSearchService(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<List<FrameDto>> SearchFrames(AdvancedSearchViewModel request)
        {
            var query =
                from d in _context.Detections
                join f in _context.Frames on d.FrameId equals f.Id
                join v in _context.Videos on f.VideoId equals v.Id
                join o in _context.ObjectTypes on d.ObjectTypeId equals o.Id
                select new
                {
                    Detection = d,
                    Frame = f,
                    Video = v,
                    ObjectType = o
                };

            // Cursor pagination
            if ((request.Cursor!=null) && (request.Cursor.HasValue))
            {
                query = query.Where(x => x.Detection.CreatedAt < request.Cursor.Value);
            }

            // Apply filters
            if (request.Filters != null && request.Filters.Any())
            {
                foreach (var filter in request.Filters)
                {
                    var key = filter.Key.ToLower();

                    switch (key)
                    {
                        case "label":

                            query = query.Where(x => x.Detection.Label == filter.Value);
                            break;

                        case "range":

                            if (filter.Operator.Equals("eq", StringComparison.InvariantCultureIgnoreCase))
                            {
                                var parts = filter.Value.Split("to",StringSplitOptions.None);

                                if (parts.Length == 2 &&
                                    DateTimeOffset.TryParse(parts[0].Trim(), out var start) &&
                                    DateTimeOffset.TryParse(parts[1].Trim(), out var end))
                                {
                                    query = query.Where(x => x.Detection.StartTime >= start && x.Detection.StartTime <= end);
                                }
                            }
                            break;
                    }
                }
            }

            // Free text search
            if (!string.IsNullOrWhiteSpace(request.Text))
            {
                var text = request.Text.ToLower();

                query = query.Where(x =>
                    x.ObjectType.Name.ToLower().Contains(text) ||
                    x.Detection.Label.ToLower().Contains(text) ||
                    x.Video.FileName.ToLower().Contains(text)
                );


            }

            // Order newest first
            var results = await query
                .OrderByDescending(x => x.Detection.CreatedAt)
                .Take(request.Limit)
                .Select(x => new FrameDto
                {
                    FrameId = x.Frame.Id,
                    FramePath = Path.GetFileName(x.Frame.FramePath),
                    FrameIndex = x.Frame.FrameIndex,
                    VideoId = x.Video.Id,
                    VideoFile = x.Video.FileName,
                    ObjectType = x.ObjectType.Name,
                    Label = x.Detection.Label ?? String.Empty,
                    Confidence = x.Detection.Confidence,
                    DetectionTime = x.Detection.StartTime,
                    X1 = x.Detection.X1,
                    Y1 = x.Detection.Y1,
                    X2 = x.Detection.X2,
                    Y2 = x.Detection.Y2
                })
                .ToListAsync();

            return results;
        }
    }
}
