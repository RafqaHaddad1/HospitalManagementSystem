using System.ComponentModel.DataAnnotations;

namespace Hospital_Management_System.Models
{
    public class OperatingRoom
    {
        [Key]
        public int OR_ID { get; set; }
        public string Room_Name { get; set; }
        public string EquipementsAvailable { get; set; } 

    }
}
