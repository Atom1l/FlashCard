using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace FlashCard.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        // Login Part
        public string? UserName { get; set; }
        public string? Password { get; set; }

        // Register Part
        public string? Name { get; set; }
        public string? Surname { get; set; }

        // Game Part
        public int Streak { get; set; } = 0;
        public DateTime? LastStreakDate { get; set; }
        public int M1Score { get; set; }
        public int M2Score { get; set; }
        public int M3Score { get; set; }
    }
}
