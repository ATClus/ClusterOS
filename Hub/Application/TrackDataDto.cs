namespace Hub.Application
{
    public class TrackDataItemDto
    {
        public int Id { get; set; }
        public string AppTitle { get; set; }
        public string TabUrl { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public class TrackDataDto
    {
        public List<TrackDataItemDto> Items { get; set; }
    }
}
