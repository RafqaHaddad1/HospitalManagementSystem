using Hospital_Management_System.Models;
using Microsoft.EntityFrameworkCore;


namespace Hospital_Management_System.Database
{
    public class HospitalDbContext : DbContext
    {
        public HospitalDbContext(DbContextOptions<HospitalDbContext> options) : base(options)
        {
        }
        public DbSet<Staff> Staff { get; set; } = default!;
    }
}
