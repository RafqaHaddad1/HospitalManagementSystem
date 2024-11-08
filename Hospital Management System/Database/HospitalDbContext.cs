using Microsoft.EntityFrameworkCore;


namespace Hospital_Management_System.Database
{
    public class HospitalDbContext : DbContext
    {
        public HospitalDbContext(DbContextOptions<HospitalDbContext> options) : base(options)
        {
        }
    }
}
