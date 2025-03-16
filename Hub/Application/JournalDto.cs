using System.ComponentModel.DataAnnotations;

namespace Hub.Application
{
    public class JournalDto
    {
        [Required]
        [MaxLength(10000)]
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
    }
}
