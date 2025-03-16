using System;

namespace ClusterOS.Models
{
    public class TaskItem
    {
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public Priority priority { get; set; }
        public TaskItemStatus status { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime? updatedAt { get; set; }

        public TaskItem() { }

        public TaskItem(string ctitle, string cdescription, Priority cpriority, TaskItemStatus cstatus)
        {
            title = ctitle;
            description = cdescription;
            priority = cpriority;
            status = cstatus;
            createdAt = DateTime.Now;
        }

        public void Update(string ctitle, string cdescription, Priority cpriority, TaskItemStatus cstatus)
        {
            title = ctitle;
            description = cdescription;
            priority = cpriority;
            status = cstatus;
            updatedAt = DateTime.Now;
        }
    }
}
