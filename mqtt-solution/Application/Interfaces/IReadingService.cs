using Domain.Entities;

namespace Application.Interfaces
{
    public interface IReadingService
    {
        Task<IEnumerable<Reading>> GetAll();
        Task<Reading> CreateAsync(string userId, float value);
    }
}
