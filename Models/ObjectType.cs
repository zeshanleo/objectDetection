using System.ComponentModel.DataAnnotations;

namespace VideoDetectionPOC.Models
{
    public class ObjectType
    {
        [Key] public int Id { get; set; }
        public string Name { get; set; } = "";
    }
}
