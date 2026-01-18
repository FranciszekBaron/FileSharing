using System.Collections;

public interface IFileItemAccessRepository : IRepositoryBase<FileItemAccess>
{
    Task<IEnumerable<FileItemAccess>> GetSharedFiles(string userId);

    Task<List<UserGetDto>> GetUsersWhoSharedWithMe(string userId);

    Task<bool> IsAlreadyShared(string fileId, string userId);

    Task<List<UserGetDto>> GetAllUserAccessessForFileAsync(string fileId);
}