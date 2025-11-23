using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Services.ReadingService.Query.GetReadingsByUser;

public class GetReadingsByUserQueryHandler : IRequestHandler<GetReadingsByUserQuery, IEnumerable<Domain.Entities.Reading>>
{
    private readonly IReadingRepository _readingRepository;

    public GetReadingsByUserQueryHandler(IReadingRepository readingRepository)
    {
        _readingRepository = readingRepository;
    }

    public async Task<IEnumerable<Domain.Entities.Reading>> Handle(GetReadingsByUserQuery request, CancellationToken cancellationToken)
    {
        return await _readingRepository.GetByUserId(request.UserId);
    }
}
