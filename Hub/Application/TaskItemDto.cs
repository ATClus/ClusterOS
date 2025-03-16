using Hub.Domain;
using System.ComponentModel.DataAnnotations;

namespace Hub.Application
{
    public class TaskItemDto
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public Priority Priority { get; set; }
        [Required]
        public TaskItemStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
