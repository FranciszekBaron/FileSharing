using System.Security.Cryptography.X509Certificates;
using FileSharing.Models;
using Microsoft.EntityFrameworkCore;

public class FileSharingDbContext : DbContext
{
    public FileSharingDbContext(DbContextOptions options) : base(options)
    {}
    public DbSet<FileItem> FileItems { get; set; }
    public DbSet<FileItemAccess> FilesAccesess { get; set; }
    public DbSet<User> Users { get; set; }

    public DbSet<RefreshToken> RefreshToken { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FileItem>(e =>
        {
            //Pk
            e.HasKey(e => e.Id);

            //pola
            e.Property(e => e.Name)
                .HasMaxLength(255)
                .IsRequired();
            e.Property(e=>e.Type)
                .HasMaxLength(50)
                .IsRequired();
            e.Property(e=>e.Size);
            e.Property(e=>e.ModifiedDate)
                .IsRequired();



            //FK
            e.Property(e=>e.OwnerId)
              .IsRequired(); 
            e.Property(e=>e.ParentId);


            //pola - kategoryzacja  
            e.Property(e=>e.Starred);
            e.Property(e=>e.Deleted);
            e.Property(e=>e.DeletedAt);

            e.HasOne(e => e.Owner) // usuwasz to => OnDelete
            .WithMany(e => e.OwnedFiles)
            .HasForeignKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.Cascade); //bo softDelete

            e.HasOne(e => e.Parent) // usuwasz to => OnDelete
            .WithMany(e => e.Children)
            .HasForeignKey(e => e.ParentId)
            .OnDelete(DeleteBehavior.Cascade); //bo softDelete


            e.HasIndex(e=>e.OwnerId);
            e.HasIndex(e=>e.ParentId);
            e.HasIndex(e=>e.Deleted);
            e.HasIndex(e=>e.Starred);
            e.HasIndex(e=>e.ModifiedDate);
        }); 


        modelBuilder.Entity<FileItemAccess>(e =>
        {
            //PK
            e.HasKey(e => e.Id);

            //FK
            e.Property(e=>e.UserId)
                .IsRequired();
            e.Property(e=>e.FileItemId)
                .IsRequired();

            
            e.Property(e=>e.PermissionType)
                .HasMaxLength(50)
                .IsRequired();
            e.Property(e=>e.SharedDate)
                .IsRequired();

            e.HasOne(e=>e.User) //usuwamy user => usuwamy tez fileAccesy
            .WithMany(e => e.FileAccesses)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(e=>e.FileItem) //usuwamy user => usuwamy tez fileAccesy
            .WithMany(e => e.FileItemAccesses)
            .HasForeignKey(e => e.FileItemId)
            .OnDelete(DeleteBehavior.Cascade);


            e.HasIndex(e=>e.UserId);
            e.HasIndex(e=>e.FileItemId);
            e.HasIndex(e=>e.SharedDate);
        });


        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
        
            e.Property(u => u.UserName)
                .HasMaxLength(100)
                .IsRequired();
            
            e.Property(u => u.Email)
                .HasMaxLength(255)
                .IsRequired();
            
            e.Property(u => u.PasswordHash)
                .IsRequired();
            
            e.Property(u => u.Avatar);
        
            
            // Email unikalny
            e.HasIndex(u => u.Email)
                .IsUnique();
        });


        modelBuilder.Entity<RefreshToken>(e =>
        {
            //PK
            e.HasKey(t => t.Id);

            //FK
            e.Property(t=>t.UserId)
                .IsRequired();

            e.Property(t=>t.Token)
                .IsRequired();
            e.Property(t=>t.ExpiresAt)
                .IsRequired();
            e.Property(t=>t.CreatedAt)
                .IsRequired();
            e.Property(t=>t.IsRevoked)
                .IsRequired();
            
            e.HasOne(t=>t.User) 
            .WithMany(u=>u.RefreshTokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);


        });
    }
}