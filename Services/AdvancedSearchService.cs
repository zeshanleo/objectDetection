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

                        //case "object":

                        //    query = query.Where(x => x.ObjectType.Name == filter.Value);
                        //    break;

                        //case "video":

                        //    query = query.Where(x => x.Video.FileName.Contains(filter.Value));
                        //    break;

                        //case "device":

                        //    query = query.Where(x => x.Video.DeviceId == filter.Value);
                        //    break;

                        //case "confidence":

                        //    if (float.TryParse(filter.Value, out float conf))
                        //        query = query.Where(x => x.Detection.Confidence >= conf);

                        //    break;

                        //case "timestamp":

                        //    var date = DateTimeOffset.Parse(filter.Value);

                        //    if (filter.Operator == "gte")
                        //        query = query.Where(x => x.Detection.StartTime >= date);

                        //    if (filter.Operator == "lte")
                        //        query = query.Where(x => x.Detection.StartTime <= date);

                        //    break;
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
                    Label = x.Detection.Label,
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
