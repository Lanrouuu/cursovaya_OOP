using Cursovaya.Models;
using Cursovaya.Services;
using Microsoft.EntityFrameworkCore;

namespace Cursovaya.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        var admin = await EnsureUserAsync(
            context,
            "Администратор",
            "admin@mail.com",
            "+7 900 000-00-01",
            "admin123",
            UserRole.Admin);

        var user = await EnsureUserAsync(
            context,
            "Тестовый пользователь",
            "user@mail.com",
            "+7 900 000-00-02",
            "user123",
            UserRole.User);

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

        foreach (var category in categories)
        {
            await EnsureCategoryAsync(context, category.Name, category.Description);
        }

        await context.SaveChangesAsync();

        if (await context.Advertisements.AnyAsync())
        {
            return;
        }

        var electronics = await context.Categories.FirstAsync(x => x.Name == "Электроника");
        var clothes = await context.Categories.FirstAsync(x => x.Name == "Одежда");
        var furniture = await context.Categories.FirstAsync(x => x.Name == "Мебель");
        var transport = await context.Categories.FirstAsync(x => x.Name == "Транспорт");

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

    private static async Task<User> EnsureUserAsync(
        AppDbContext context,
        string userName,
        string email,
        string phoneNumber,
        string password,
        UserRole role)
    {
        var normalizedEmail = email.Trim().ToLower();
        var user = await context.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == normalizedEmail);

        if (user == null)
        {
            user = new User
            {
                UserName = userName,
                Email = email,
                PhoneNumber = phoneNumber,
                CreatedAt = DateTime.Now
            };
            await context.Users.AddAsync(user);
        }

        user.UserName = userName;
        user.Email = email;
        user.PhoneNumber = phoneNumber;
        user.PasswordHash = PasswordService.HashPassword(password);
        user.Role = role;
        user.IsBlocked = false;
        user.UpdatedAt = DateTime.Now;

        return user;
    }

    private static async Task EnsureCategoryAsync(AppDbContext context, string name, string description)
    {
        var category = await context.Categories.FirstOrDefaultAsync(x => x.Name == name);
        if (category == null)
        {
            await context.Categories.AddAsync(new Category
            {
                Name = name,
                Description = description,
                IsActive = true
            });
            return;
        }

        category.Description = description;
        category.IsActive = true;
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
