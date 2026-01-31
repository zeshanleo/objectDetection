using Microsoft.EntityFrameworkCore;
using VideoDetectionPOC.Models;

namespace VideoDetectionPOC.DataAccess
{
    public class ApplicationDBContext : DbContext
    {
        public DbSet<Video> Videos { get; set; }
        public DbSet<Frame> Frames { get; set; }
        public DbSet<Detection> Detections { get; set; }
        public DbSet<Embedding> Embeddings { get; set; }
        public DbSet<ObjectType> ObjectTypes { get; set; }

        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> dbContextOptions) : base(dbContextOptions)
        {
                
        }
    }
}
