using Hub.Application;
using Hub.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hub.Infrastructure.Repositories
{
    public class JournalRepository : IJournalRepository
    {
        private readonly HubDbContext _context;

        public JournalRepository(HubDbContext context)
        {
            _context = context;
        }

        public async Task<Journal> GetByIdAsync(int id)
        {
            return await _context.Journals.FindAsync(id);
        }

        public async Task<IEnumerable<Journal>> GetAllAsync()
        {
            return await _context.Journals.ToListAsync();
        }

        public async Task<Journal> GetByDateAsync(DateTime date)
        {
            return await _context.Journals.FirstOrDefaultAsync(j => j.Created.Date == date.Date);
        }

        public async Task AddAsync(Journal journal)
        {
            await _context.Journals.AddAsync(journal);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Journal journal)
        {
            _context.Journals.Update(journal);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(Journal journal)
        {
            _context.Journals.Remove(journal);
            await _context.SaveChangesAsync();
        }
    }
}
