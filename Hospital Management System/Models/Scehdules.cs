namespace Hospital_Management_System.Models
{
    public class Scehdules
    {
        public  int EventID { get; set; }

        public DateOnly Date { get; set; } 
        public TimeOnly Start {  get; set; }
        public TimeOnly End { get; set; }
        public int StaffID { get; set; }
        public int DepartmentID { get; set; } 

    }
}
