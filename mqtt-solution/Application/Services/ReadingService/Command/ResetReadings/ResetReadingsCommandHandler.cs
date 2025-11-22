using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Services.ReadingService.Command.ResetReadings;

public class ResetReadingsCommandHandler : IRequestHandler<ResetReadingsCommand, Unit>
{
    private readonly IReadingRepository _readingRepository;

    public ResetReadingsCommandHandler(IReadingRepository readingRepository)
    {
        _readingRepository = readingRepository;
    }

    public async Task<Unit> Handle(ResetReadingsCommand request, CancellationToken cancellationToken)
    {
        await _readingRepository.DeleteByUserIdAsync(request.UserId);
        return Unit.Value;
    }
}
