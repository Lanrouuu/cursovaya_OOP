using Cursovaya.Models;
using Microsoft.Win32;
using System.IO;
using System.Text;

namespace Cursovaya.Services;

public class ExportService
{
    public bool ExportUsers(IEnumerable<User> users)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Экспорт пользователей",
            Filter = "CSV файлы (*.csv)|*.csv",
            FileName = $"users_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        };

        if (dialog.ShowDialog() != true) return false;

        using var writer = new StreamWriter(dialog.FileName, false, Encoding.UTF8);
        writer.WriteLine("Id;Имя;Email;Телефон;Роль;Заблокирован;Дата регистрации");

        foreach (var u in users)
        {
            writer.WriteLine(
                $"{u.Id};" +
                $"{Escape(u.UserName)};" +
                $"{Escape(u.Email)};" +
                $"{Escape(u.PhoneNumber)};" +
                $"{u.Role};" +
                $"{u.IsBlocked};" +
                $"{u.CreatedAt:dd.MM.yyyy HH:mm}");
        }

        return true;
    }

    public bool ExportAdvertisements(IEnumerable<Advertisement> advertisements)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Экспорт объявлений",
            Filter = "CSV файлы (*.csv)|*.csv",
            FileName = $"advertisements_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        };

        if (dialog.ShowDialog() != true) return false;

        using var writer = new StreamWriter(dialog.FileName, false, Encoding.UTF8);
        writer.WriteLine("Id;Название;Цена;Категория;Продавец;Город;Статус;Просмотры;Дата создания;Истекает");

        foreach (var a in advertisements)
        {
            writer.WriteLine(
                $"{a.Id};" +
                $"{Escape(a.Title)};" +
                $"{a.Price:N0};" +
                $"{Escape(a.Category?.Name ?? "")};" +
                $"{Escape(a.User?.UserName ?? "")};" +
                $"{Escape(a.City)};" +
                $"{a.Status};" +
                $"{a.ViewCount};" +
                $"{a.CreatedAt:dd.MM.yyyy HH:mm};" +
                $"{a.ExpiresAt?.ToString("dd.MM.yyyy") ?? ""}");
        }

        return true;
    }

    private static string Escape(string value)
    {
        if (value.Contains(';') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
