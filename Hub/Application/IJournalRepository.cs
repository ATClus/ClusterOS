using Hub.Domain;

namespace Hub.Application
{
    public interface IJournalRepository
    {
        Task<Journal> GetByIdAsync(int id);
        Task<IEnumerable<Journal>> GetAllAsync();
        Task<Journal> GetByDateAsync(DateTime date);
        Task AddAsync(Journal journal);
        Task UpdateAsync(Journal journal);
        Task RemoveAsync(Journal journal);
    }
}
