using RenTN.Domain.Entities;

namespace RenTN.Domain.Interfaces;

public interface IChangeLogsRepository
{
    Task AddChangeLogs(List<ChangeLog> changeLogs);
}
