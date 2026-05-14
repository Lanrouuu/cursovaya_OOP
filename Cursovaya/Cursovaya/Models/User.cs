namespace Cursovaya.Models;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsBlocked { get; set; }

    public ICollection<Advertisement> Advertisements { get; set; } = new List<Advertisement>();
    public ICollection<FavoriteAdvertisement> FavoriteAdvertisements { get; set; } = new List<FavoriteAdvertisement>();
}
