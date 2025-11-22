using MediatR;

namespace Application.Services.ReadingService.Command.ResetReadings;

public record ResetReadingsCommand(string UserId) : IRequest<Unit>;
