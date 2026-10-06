using Arkana.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Arkana.Data;

public class AppDbContext : DbContext
{
    // todos los contextos comparten la misma base en memoria
    static readonly InMemoryDatabaseRoot root = new InMemoryDatabaseRoot();

    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Follow> Follows { get; set; }

    // base de datos en memoria (se borra cuando cierras la app)
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("ArkanaDB", root);
    }

    // datos de ejemplo
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Name = "Luna", Username = "luna", Email = "luna@arkana.com", Password = "1234" },
            new User { Id = 2, Name = "Sebas", Username = "sebas", Email = "sebas@arkana.com", Password = "1234" },
            new User { Id = 3, Name = "Vale", Username = "vale", Email = "vale@arkana.com", Password = "1234" },
            new User { Id = 4, Name = "Sofía Martínez", Username = "sofi", Email = "sofi@arkana.com", Password = "1234" },
            new User { Id = 5, Name = "Carlos Rivera", Username = "carlos", Email = "carlos@arkana.com", Password = "1234" },
            new User { Id = 6, Name = "Daniela Torres", Username = "daniela", Email = "daniela@arkana.com", Password = "1234" }
        );

        modelBuilder.Entity<Post>().HasData(
            new Post { Id = 1, UserId = 1, Username = "luna", Content = "Noche perfecta para mirar las estrellas ✨", Date = new DateTime(2026, 10, 1, 21, 0, 0), Likes = 128 },
            new Post { Id = 2, UserId = 3, Username = "vale", Content = "Hoy el jardín amaneció así 🌸", Date = new DateTime(2026, 10, 2, 8, 30, 0), Likes = 76 },
            new Post { Id = 3, UserId = 2, Username = "sebas", Content = "Ensayo de guitarra 🎸", Date = new DateTime(2026, 10, 3, 18, 15, 0), Likes = 45 },
            new Post { Id = 4, UserId = 4, Username = "sofi", Content = "La vida siempre sorprende cuando menos lo esperas 💜", Date = new DateTime(2026, 10, 4, 10, 0, 0), Likes = 24 },
            new Post { Id = 5, UserId = 5, Username = "carlos", Content = "Nuevos proyectos, misma vibra 🔥", Date = new DateTime(2026, 10, 4, 16, 45, 0), Likes = 31 }
        );

        modelBuilder.Entity<Follow>().HasData(
            new Follow { Id = 1, FollowerId = 1, FollowingId = 2 },
            new Follow { Id = 2, FollowerId = 1, FollowingId = 3 },
            new Follow { Id = 3, FollowerId = 4, FollowingId = 1 },
            new Follow { Id = 4, FollowerId = 5, FollowingId = 1 },
            new Follow { Id = 5, FollowerId = 6, FollowingId = 1 }
        );
    }
}