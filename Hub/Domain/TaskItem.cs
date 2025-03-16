using System.ComponentModel.DataAnnotations;

namespace Hub.Domain
{
    public class TaskItem
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
        public DateTime? UpdatedAt { get; set; }

        public TaskItem(string title, string description, Priority priority, TaskItemStatus status)
        {
            Title = title; 
            Description = description; 
            Priority = priority;
            CreatedAt = DateTime.Now;
            Status = status;
        }

        public void Update(string title, string description, Priority priority, TaskItemStatus status)
        {
            Title = title;
            Description = description;
            Priority = priority;
            UpdatedAt = DateTime.Now;
            Status = status;
        }
    }
}
