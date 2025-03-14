using Hub.Domain;

namespace Hub.Application
{
    public interface ITrackDataRepository
    {
        Task<TrackDataDto> GetTrackDataAsync();
    }
}
