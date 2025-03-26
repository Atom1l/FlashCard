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
    }
}
