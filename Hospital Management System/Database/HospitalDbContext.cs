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
        public DbSet<Department> Department { get; set; } = default!;
        public DbSet<Laboratory> Laboratory { get; set; } = default!;
        public DbSet<RadiologyImages> RadiologyImages { get; set; } = default!;
        public DbSet<Patient> Patient { get; set; } = default!;
        public DbSet<ERBeds> ERBeds { get; set; } = default!;
        public DbSet<Schedules> Schedules { get; set; } = default!;
        public DbSet<Login> Login { get; set; } = default!;
        public DbSet<OperatingRoom> OperatingRoom { get; set; } = default!;
        public DbSet<SurgeryBooking> SurgeryBooking { get; set; } = default!;
        public DbSet<Room> Room{ get; set; } = default!;
    }
}
