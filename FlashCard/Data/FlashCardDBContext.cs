using Microsoft.EntityFrameworkCore;
using FlashCard.Models;

namespace FlashCard.Data
{
    public class FlashCardDBContext : DbContext
    {
        public FlashCardDBContext() { }
        public FlashCardDBContext(DbContextOptions<FlashCardDBContext> options) : base(options) { }

        // DB Set
        public DbSet<FlashCard.Models.User> UsersDB { get; set; }
        public DbSet<FlashCard.Models.Images> ImagesDB { get; set; }
        public DbSet<FlashCard.Models.UserFlashCard> UserCardDB { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // กำหนด Primary Key สำหรับ UserFlashCard
            modelBuilder.Entity<UserFlashCard>()
                .HasKey(uf => uf.Id);  // ตั้งให้ Id เป็น Primary Key

            // กำหนดความสัมพันธ์ระหว่าง UserFlashCard กับ Users
            modelBuilder.Entity<UserFlashCard>()
                .HasOne<FlashCard.Models.User>()
                .WithMany()
                .HasForeignKey(uf => uf.UserId)
                .OnDelete(DeleteBehavior.Cascade);  // ลบข้อมูลถ้า User ถูกลบ

            // กำหนดความสัมพันธ์ระหว่าง UserFlashCard กับ Images
            modelBuilder.Entity<UserFlashCard>()
                .HasOne(uf => uf.Image)
                .WithMany()
                .HasForeignKey(uf => uf.ImageId)
                .OnDelete(DeleteBehavior.Cascade);  // ลบข้อมูลถ้าภาพถูกลบ
        }
    }
}
