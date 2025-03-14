using Hub.Domain;
using Hub.Application;
using Microsoft.EntityFrameworkCore;

namespace Hub.Infrastructure.Repositories
{
    public class TrackDataRepository : ITrackDataRepository
    {
        private readonly HubDbContext _dbContext;

        public TrackDataRepository(HubDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TrackDataDto> GetTrackDataAsync()
        {
            var result = await (
                from te in _dbContext.TimeEntries
                join t in _dbContext.Tabs on te.TabId equals t.Id into tabs
                from t in tabs.DefaultIfEmpty()
                join a in _dbContext.Applications on te.ApplicationId equals a.Id into apps
                from a in apps.DefaultIfEmpty()
                select new TrackDataItemDto
                {
                    Id = te.Id,
                    AppTitle = a != null ? a.Title : string.Empty,
                    TabUrl = t != null ? t.Url : string.Empty,
                    Date = te.Date,
                    Duration = te.Duration
                }
            ).ToListAsync();

            return new TrackDataDto { Items = result };
        }
    }
}
