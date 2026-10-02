using StudyFlow.Bus.Models;

namespace StudyFlow.Bus.Interfaces;

public interface IAcademicEventService
{
    Task<IReadOnlyList<AcademicEvent>> GetEventsAsync(CancellationToken cancellationToken = default);
    Task SaveEventAsync(AcademicEvent academicEvent, CancellationToken cancellationToken = default);
    Task DeleteEventAsync(Guid id, CancellationToken cancellationToken = default);
}
