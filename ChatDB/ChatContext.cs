using ChatCommon;
using Microsoft.EntityFrameworkCore;

namespace ChatDB
{
    public class ChatContext : DbContext
    {
        private readonly string connection = @"Server=DESKTOP-17SELU5\MSSQLSERVER, 1433;Database=Chat;" +
            "TrustServerCertificate=True;Trusted_Connection=True";
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connection).UseLazyLoadingProxies();
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                
                entity.ToTable("users");
                
                entity.HasKey(x => x.Id).HasName("user_pk");

                entity.HasIndex(e => e.FullName).IsUnique();
                
                entity.Property(e => e.FullName).HasColumnName("FullName").HasMaxLength(255)
                    .IsRequired();
            });
            modelBuilder.Entity<Message>(entity =>
            {
                
                entity.HasKey(x => x.Id).HasName("message_pk");
                entity.ToTable("messages");
                
                entity.Property(e => e.Text).HasColumnName("message_text");
                entity.Property(e => e.Date).HasColumnName("message_date");
                entity.Property(e => e.IsSent).HasColumnName("is_sent");
                entity.Property(e => e.Id).HasColumnName("id");
                
                entity.HasOne(x => x.To).WithMany(m => m.MessagesTo)
                    .HasForeignKey(x => x.ToId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("message_to_user_fk");
                entity.HasOne(x => x.From).WithMany(m => m.MessagesFrom)
                    .HasForeignKey(x => x.FromId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("message_from_user_fk");
            });
        }
    }
}
