
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;

namespace TaskFlow.Models;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = Convert.ToHexString(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes("1234")));


        // Dodanie domyślnego administratora
        var admin = new User
        {
            Id = 1,
            UserName = "admin",
            Password = hash,
            Role = "ADMIN",
            ApiToken = Guid.NewGuid().ToString()
        };
        modelBuilder.Entity<User>().HasData(admin);
    }
}
