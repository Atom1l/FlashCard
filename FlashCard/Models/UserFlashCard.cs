using System.ComponentModel.DataAnnotations.Schema;

namespace FlashCard.Models
{
    public class UserFlashCard
    {
        public int Id { get; set; }
        public int? UserId { get; set; }  // อ้างอิงผู้ใช้
        public int ImageId { get; set; }  // อ้างอิงการ์ดที่เคยเห็น
        public bool HasShown { get; set; }  // บันทึกว่าผู้ใช้ได้เห็นหรือยัง

        [ForeignKey("ImageId")]
        public Images? Image { get; set; }
    }
}
