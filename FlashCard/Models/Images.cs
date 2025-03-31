namespace FlashCard.Models
{
    public class Images
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Answer { get; set; }
        public byte[]? Imgbytes { get; set; }
        public string? Module { get; set; }
        public string? SubModule { get; set; }
        public bool? HasShown { get; set; }
    }
}
