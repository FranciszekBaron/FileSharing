using System.Collections;

public interface IFileItemAccessRepository : IRepositoryBase<FileItemAccess>
{
    Task<IEnumerable<FileItemAccess>> GetSharedFiles(string userId);
}