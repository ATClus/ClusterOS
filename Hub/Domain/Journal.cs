using System.ComponentModel.DataAnnotations;

namespace Hub.Domain
{
    public class Journal
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(10000)]
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }

        public Journal(string content)
        {
            Content = content;
            Created = DateTime.Now;
        }

        public void UpdateContent(string content)
        {
            if (Created.Date != DateTime.Now.Date)
            {
                throw new InvalidOperationException("Cannot update the Journal after the end of the day.");
            }

            Content = content;
            Updated = DateTime.Now;
        }
    }
}
