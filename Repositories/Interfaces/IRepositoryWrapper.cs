public interface IRepositoryWrapper
{
   IFileItemRepository fileItemRepo{ get; }

   IFileItemAccessRepository fileItemAccessRepo { get; }

   Task SaveAsync();
}