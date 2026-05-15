using Cursovaya.Data;
using Cursovaya.Repositories;
using Cursovaya.Services;
using Cursovaya.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Windows;
using System.Windows.Threading;

namespace Cursovaya;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        var context = new AppDbContext(options);

        try
        {
            await DbInitializer.InitializeAsync(context);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Не удалось подготовить базу данных SQL Server.\n" +
                "Если сервер доступен, проверьте, что миграции применились и в TradeAdsDb есть таблицы Users, Categories и Advertisements.\n" +
                "Также проверьте строку подключения в appsettings.json.\n\n" +
                ex.Message,
                "Проблема базы данных",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        var unitOfWork = new UnitOfWork(context);
        var undoRedoService = new UndoRedoService();
        var authService = new AuthService(unitOfWork);
        var advertisementService = new AdvertisementService(unitOfWork, undoRedoService);
        var categoryService = new CategoryService(unitOfWork);
        var userService = new UserService(unitOfWork);
        var dialogService = new DialogService();
        var themeService = new ThemeService();
        var localizationService = new LocalizationService();
        var imageService = new ImageService();
        var navigationService = new NavigationService();

        var mainViewModel = new MainViewModel(
            authService,
            advertisementService,
            categoryService,
            userService,
            dialogService,
            themeService,
            localizationService,
            imageService,
            undoRedoService,
            navigationService);

        var window = new MainWindow
        {
            DataContext = mainViewModel
        };

        window.Show();
        await mainViewModel.ShowAdvertisementsAsync();
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            $"Произошла ошибка:\n{e.Exception.Message}",
            "Ошибка приложения",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        e.Handled = true;
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        e.SetObserved();
    }
}
