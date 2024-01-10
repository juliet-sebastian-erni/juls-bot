using Microsoft.EntityFrameworkCore;

namespace EchoBot.Bots
{
    public class MyDbContext : DbContext
    {
        public DbSet<Services> Services { get; set; }
        public DbSet<Dentist> Dentists { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<ChatLog> ChatLogs { get; set; }
        public DbSet<Reminder> Reminder { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // optionsBuilder.UseInMemoryDatabase("DentalBotDb");

            var connectionString = "Server=tcp:juls-dental-bot-server.database.windows.net,1433;Initial Catalog=DentalBotDB;Persist Security Info=False;User ID=azureuser;Password=Password123@;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}

