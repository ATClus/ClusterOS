using Hub.Domain;

namespace Hub.Application
{
    public interface ITaskItemRepository
    {
        Task AddAsync(TaskItem task);
        Task RemoveAsync(TaskItem task);
        Task UpdateAsync(TaskItem task);
        Task<IEnumerable<TaskItem>> GetAllAsync();
        Task<TaskItem> GetByIdAsync(int id);
    }
}
