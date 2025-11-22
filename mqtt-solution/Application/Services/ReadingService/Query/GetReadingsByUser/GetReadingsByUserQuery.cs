using MediatR;

namespace Application.Services.ReadingService.Query.GetReadingsByUser;

public record GetReadingsByUserQuery(string UserId) : IRequest<IEnumerable<Domain.Entities.Reading>>;
