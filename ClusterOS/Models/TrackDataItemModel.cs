using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterOS.Models
{
    public class TrackDataItemModel
    {
        public int Id { get; set; }
        public string AppTitle { get; set; }
        public string TabUrl { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
