using Domain.Entities;

namespace Application.Interfaces
{
    public interface IReadingService
    {
        Task<IEnumerable<Reading>> GetAll();
        Task<IEnumerable<Reading>> GetByUserId(string userId);
        Task<Reading> CreateAsync(string userId, float value);
        // Remove or archive all readings for a user (used for billing reset)
        Task ResetForUserAsync(string userId);
    }
}
