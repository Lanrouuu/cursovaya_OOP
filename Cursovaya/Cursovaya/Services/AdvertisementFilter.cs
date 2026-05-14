using Cursovaya.Models;

namespace Cursovaya.Services;

public class AdvertisementFilter
{
    public string SearchText { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string City { get; set; } = string.Empty;
    public ItemCondition? Condition { get; set; }
    public AdvertisementStatus? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public string SortMode { get; set; } = "date_desc";
}
