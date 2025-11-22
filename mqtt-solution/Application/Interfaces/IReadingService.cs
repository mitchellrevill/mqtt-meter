using Domain.Entities;

namespace Application.Interfaces
{
    public interface IReadingService
    {
        Task<IEnumerable<Reading>> GetAll();
        Task<IEnumerable<Reading>> GetByUserId(string userId);
        Task<Reading> CreateAsync(string userId, float value);
    }
}
