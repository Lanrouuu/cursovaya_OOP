namespace Cursovaya.Models;

public class FavoriteAdvertisement
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public int AdvertisementId { get; set; }
    public Advertisement? Advertisement { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
