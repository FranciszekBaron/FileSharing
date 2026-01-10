public interface IRepositoryWrapper
{
   IFileItemRepository fileItemRepo{ get; }

   Task SaveAsync();
}