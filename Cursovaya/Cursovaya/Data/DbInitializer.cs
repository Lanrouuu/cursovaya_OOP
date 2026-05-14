using Cursovaya.Models;
using Cursovaya.Services;
using Microsoft.EntityFrameworkCore;

namespace Cursovaya.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        if (await context.Users.AnyAsync())
        {
            return;
        }

        var admin = new User
        {
            UserName = "Администратор",
            Email = "admin@mail.com",
            PhoneNumber = "+7 900 000-00-01",
            PasswordHash = PasswordService.HashPassword("admin123"),
            Role = UserRole.Admin,
            CreatedAt = DateTime.Now
        };

        var user = new User
        {
            UserName = "Тестовый пользователь",
            Email = "user@mail.com",
            PhoneNumber = "+7 900 000-00-02",
            PasswordHash = PasswordService.HashPassword("user123"),
            Role = UserRole.User,
            CreatedAt = DateTime.Now
        };

        var categories = new List<Category>
        {
            new() { Name = "Электроника", Description = "Телефоны, ноутбуки и техника" },
            new() { Name = "Одежда", Description = "Одежда и обувь" },
            new() { Name = "Мебель", Description = "Мебель для дома и офиса" },
            new() { Name = "Транспорт", Description = "Велосипеды, автомобили и запчасти" },
            new() { Name = "Недвижимость", Description = "Квартиры, комнаты, дома" },
            new() { Name = "Услуги", Description = "Разные услуги" },
            new() { Name = "Другое", Description = "Прочие объявления" }
        };

        await context.Users.AddRangeAsync(admin, user);
        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        var electronics = categories.First(x => x.Name == "Электроника");
        var clothes = categories.First(x => x.Name == "Одежда");
        var furniture = categories.First(x => x.Name == "Мебель");
        var transport = categories.First(x => x.Name == "Транспорт");

        var advertisements = new List<Advertisement>
        {
            CreateAdvertisement("Ноутбук Lenovo", "Рабочий ноутбук для учёбы", "Lenovo IdeaPad в хорошем состоянии. Подходит для учёбы, документов и браузера.", 32000, "Минск", ItemCondition.Used, electronics.Id, user),
            CreateAdvertisement("Велосипед", "Городской велосипед", "Удобный велосипед для прогулок по городу. Есть небольшие следы использования.", 8500, "Москва", ItemCondition.Used, transport.Id, user),
            CreateAdvertisement("Куртка зимняя", "Тёплая зимняя куртка", "Куртка синего цвета, размер M. Без повреждений.", 4200, "Санкт-Петербург", ItemCondition.Used, clothes.Id, user),
            CreateAdvertisement("Стул офисный", "Офисный стул на колёсиках", "Регулируемый стул для рабочего места. Механизм исправен.", 3500, "Казань", ItemCondition.Used, furniture.Id, admin),
            CreateAdvertisement("Смартфон Samsung", "Samsung Galaxy в отличном состоянии", "Смартфон с зарядным устройством и чехлом. Экран без трещин.", 18500, "Москва", ItemCondition.Used, electronics.Id, admin)
        };

        await context.Advertisements.AddRangeAsync(advertisements);
        await context.SaveChangesAsync();
    }

    private static Advertisement CreateAdvertisement(
        string title,
        string shortDescription,
        string fullDescription,
        decimal price,
        string city,
        ItemCondition condition,
        int categoryId,
        User seller)
    {
        return new Advertisement
        {
            Title = title,
            ShortDescription = shortDescription,
            FullDescription = fullDescription,
            Price = price,
            City = city,
            Condition = condition,
            Status = AdvertisementStatus.Active,
            CreatedAt = DateTime.Now,
            CategoryId = categoryId,
            UserId = seller.Id,
            SellerContactEmail = seller.Email,
            SellerContactPhone = seller.PhoneNumber,
            ImagePath = string.Empty
        };
    }
}
