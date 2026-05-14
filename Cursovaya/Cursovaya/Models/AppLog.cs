namespace Cursovaya.Models;

public class AppLog
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public int? AdminUserId { get; set; }
}
