using System.ComponentModel.DataAnnotations;

namespace Hospital_Management_System.Models
{
    public class Room
    {
        [Key]
        public int? RoomID { get; set; } 
        public string? RoomType { get; set; }
        public int? Capacity { get; set; } 
        public bool? IsAvailable { get; set; } 
        public int? Department { get; set; } 
        public string? RoomLocation { get; set; } 

    }
}
