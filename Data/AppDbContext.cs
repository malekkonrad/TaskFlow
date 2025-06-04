
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Services;

namespace TaskFlow.Models;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<UserTask> Tasks { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<TaskHistory> TaskHistories { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }
    public DbSet<Status> Statuses { get; set; }



    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Project>()
            .HasOne(p => p.Owner)
            .WithMany(u => u.OwnedProjects)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Cascade); 

        modelBuilder.Entity<TaskHistory>()
            .HasOne(th => th.User)
            .WithMany(u => u.ActivityHistory)
            .HasForeignKey(th => th.UserId)
            .OnDelete(DeleteBehavior.SetNull); 

        modelBuilder.Entity<UserTask>()
            .HasOne(t => t.Assignee)
            .WithMany()
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<UserTask>()
            .HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserTask>()
        .HasOne(t => t.Status)
        .WithMany()
        .HasForeignKey(t => t.StatusId)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Comment>()
        .HasOne(c => c.Task)
        .WithMany(t => t.Comments)
        .HasForeignKey(c => c.TaskItemId)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Author)
            .WithMany()
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ProjectMember>()
            .HasOne(pm => pm.User)
            .WithMany(u => u.ProjectMemberships)
            .HasForeignKey(pm => pm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProjectMember>()
            .HasOne(pm => pm.Project)
            .WithMany(p => p.Members)
            .HasForeignKey(pm => pm.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProjectMember>()
            .HasIndex(pm => new { pm.UserId, pm.ProjectId })
            .IsUnique();


        var admin = CreateAdmin("admin");
        modelBuilder.Entity<User>().HasData(admin);

        var statuses = CreateDefaultStatuses();
        modelBuilder.Entity<Status>().HasData(statuses);

    }



    private User CreateAdmin(string password)
    {
        var hashedPassword = new PasswordService().HashPassword(password);

        var admin = new User
        {
            Id = 1,
            UserName = "admin",
            Password = hashedPassword,
            Role = "ADMIN",
            ApiToken = Guid.NewGuid().ToString()
        };

        return admin;
    }


    private List<Status> CreateDefaultStatuses()
    {
        return new List<Status>
        {
            new Status { Id = 1, Name = "To Do" },
            new Status { Id = 2, Name = "In Progress" },
            new Status { Id = 3, Name = "Done" }
        };
    }
}
