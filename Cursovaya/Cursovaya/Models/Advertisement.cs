namespace Cursovaya.Models;

public class Advertisement
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string FullDescription { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string City { get; set; } = string.Empty;
    public ItemCondition Condition { get; set; } = ItemCondition.Used;
    public AdvertisementStatus Status { get; set; } = AdvertisementStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string SellerContactEmail { get; set; } = string.Empty;
    public string SellerContactPhone { get; set; } = string.Empty;

    public int UserId { get; set; }
    public User? User { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public ICollection<FavoriteAdvertisement> FavoriteAdvertisements { get; set; } = new List<FavoriteAdvertisement>();
}
