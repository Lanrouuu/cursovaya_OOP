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
    private static bool _isShowingUnhandledError;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
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
                LocalizedStrings.Format("ErrorPrepareDatabase", ex.Message),
                LocalizedStrings.Get("DatabaseProblemTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        var emailService = new EmailService(configuration);

        var unitOfWork = new UnitOfWork(context);
        var undoRedoService = new UndoRedoService();
        var authService = new AuthService(unitOfWork, emailService);
        var advertisementService = new AdvertisementService(unitOfWork, undoRedoService, emailService);
        var categoryService = new CategoryService(unitOfWork);
        var userService = new UserService(unitOfWork, emailService);
        var favoriteService = new FavoriteService(unitOfWork);
        var appLogService = new AppLogService(unitOfWork);
        var dialogService = new DialogService();
        var themeService = new ThemeService();
        var imageService = new ImageService();
        var exportService = new ExportService();
        var navigationService = new NavigationService();

        var systemTheme = themeService.GetSystemTheme();
        themeService.ApplyTheme(systemTheme);

        var mainViewModel = new MainViewModel(
            authService,
            advertisementService,
            categoryService,
            userService,
            favoriteService,
            appLogService,
            dialogService,
            themeService,
            imageService,
            exportService,
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
        if (!_isShowingUnhandledError)
        {
            try
            {
                _isShowingUnhandledError = true;
                MessageBox.Show(
                    LocalizedStrings.Format("ErrorUnhandledApplication", e.Exception.Message),
                    LocalizedStrings.Get("ApplicationErrorTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                _isShowingUnhandledError = false;
            }
        }

        e.Handled = true;
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        e.SetObserved();
    }
}
