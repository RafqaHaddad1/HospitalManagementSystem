namespace Hospital_Management_System.Models
{
    public class Room
    {
        public int RoomID { get; set; } 
        public string RoomNumber { get; set; }
        public string RoomType { get; set; }
        public int Capacity { get; set; } 
        public bool IsAvailable { get; set; } 
        public int Deparmtent { get; set; } 
        public string RoomLocation { get; set; } 

    }
}
