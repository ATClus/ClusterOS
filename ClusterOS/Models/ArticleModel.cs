using System;

namespace ClusterOS.Models
{
    public class ArticleModel
    {
        public string id { get; set; }
        public string title { get; set; }
        public string contentPT { get; set; }
        public string contentEN { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public bool isPublished { get; set; }
        public bool isDeleted { get; set; }
        public bool highlight { get; set; }
        public string author { get; set; }
        public int categoryId { get; set; }
        public string summary { get; set; }
    }
}
