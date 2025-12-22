namespace FileSharing.Models;

public class FileItem {

    // pola
    public string Id  { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public long? Size { get; set; } 
    public DateTime ModifiedDate { get; set; } = DateTime.Now;

    // FK
    public string OwnerId  { get; set; } = string.Empty;
    public string ParentId  { get; set; } = string.Empty;

    // Kategoryzacja
    public bool? Starred { get; set; } 
    public bool? Deleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Tu przechowujemy zawartość 
    public string? ContentUrl { get; set; } 


    //Navigation Properties, eager loading -> robi sie go po to zeby odzwierciedlic w kodzie obiekt z bazy odrazu, a nie 
    //wysylac osobne zapytanie po np Ownera tego File, wtedy 2 queries zamiast jednego, ale przy loopie 100 dodatkowych queries np. N+1 problem

    public User Owner { get; set; } = null!;//Właściciel
    public FileItem Parent { get; set; }  = null!;//W ktorym się znajduje pliku/czy w 0 parent
    public ICollection<FileItem> Children { get; set; }  = new List<FileItem>();//Folder 
    public ICollection<FileItemAccess> FileItemAccesses { get; set; } = new List<FileItemAccess>(); // dlatego ze one to Many do File Access






    


}