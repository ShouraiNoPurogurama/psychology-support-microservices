using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Scheduling.API.Enums;
using Scheduling.API.Models;

namespace Scheduling.API.Data
{
        public class SchedulingDbContext : DbContext
        {
            public SchedulingDbContext(DbContextOptions<SchedulingDbContext> options) : base(options)
            {
            }

            public DbSet<Booking> Bookings => Set<Booking>();
            public DbSet<TimeSlotTemplate> TimeSlotTemplates => Set<TimeSlotTemplate>();
            public DbSet<DoctorSlotDuration> DoctorSlotDurations => Set<DoctorSlotDuration>();
            public DbSet<DoctorAvailability> DoctorAvailabilities => Set<DoctorAvailability>();


            public DbSet<Schedule> Schedules => Set<Schedule>();
            public DbSet<Session> Sessions => Set<Session>();
            public DbSet<ScheduleFeedback> ScheduleFeedbacks => Set<ScheduleFeedback>();
            public DbSet<ScheduleActivity> ScheduleActivities => Set<ScheduleActivity>();

           

            protected override void OnModelCreating(ModelBuilder builder)
            {
                base.OnModelCreating(builder);

                builder.Entity<Schedule>()
                    .HasMany(s => s.Sessions)
                    .WithOne()
                    .HasForeignKey(s => s.ScheduleId)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.Entity<Session>()
                    .HasMany(s => s.Activities)
                    .WithOne()
                    .HasForeignKey(a => a.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.Entity<Schedule>()
                    .HasMany(s => s.Feedbacks)
                    .WithOne()
                    .HasForeignKey(f => f.ScheduleId)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.Entity<Booking>()
                   .Property(e => e.Status)
                   .HasConversion(new EnumToStringConverter<BookingStatus>())
                   .HasColumnType("VARCHAR(20)");

                builder.Entity<ScheduleActivity>()
                 .Property(e => e.Status)
                 .HasConversion(new EnumToStringConverter<ScheduleActivityStatus>())
                 .HasColumnType("VARCHAR(20)");
        }
        }
}
