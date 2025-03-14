namespace Hub.Domain
{
    public class TrackData
    {
        public int Id { get; set; }
        public string AppTitle { get; set; }
        public string TabUrl { get; set; }
        public DateOnly Date {  get; set; }
        public TimeSpan Duration { get; set; }
    }
}
