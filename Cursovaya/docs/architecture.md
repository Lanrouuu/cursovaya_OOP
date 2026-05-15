# Архитектура проекта TradeAds

## Общая схема

TradeAds построен по трёхслойной архитектуре с паттерном MVVM:

```
┌──────────────────────────────────────────────┐
│                   View (XAML)                │  ← только отображение
├──────────────────────────────────────────────┤
│               ViewModel (C#)                 │  ← состояние экрана + команды
├──────────────────────────────────────────────┤
│           Services (бизнес-логика)           │  ← правила приложения
├──────────────────────────────────────────────┤
│        Repositories + Unit of Work           │  ← доступ к данным
├──────────────────────────────────────────────┤
│           Entity Framework Core 8            │  ← ORM
├──────────────────────────────────────────────┤
│           Microsoft SQL Server               │  ← база данных
└──────────────────────────────────────────────┘
```

## Паттерн MVVM

**Model** — классы в папке `Models/`. Описывают сущности предметной области (`User`, `Advertisement`, `Category`, `FavoriteAdvertisement`, `AppLog`) и перечисления (`UserRole`, `AdvertisementStatus`, `ItemCondition`).

**View** — XAML-файлы в папке `Views/` и `UserControls/`. Содержат только разметку интерфейса, никакой логики. Связь с ViewModel исключительно через `{Binding}`.

**ViewModel** — классы в папке `ViewModels/`. Хранят состояние экрана, команды и обращаются к сервисам. Реализуют `INotifyPropertyChanged` через базовый класс `ViewModelBase`.

### Поток данных

```
Пользователь нажал кнопку
        ↓
View вызывает Command (ICommand / RelayCommand)
        ↓
ViewModel выполняет метод → вызывает Service
        ↓
Service проверяет правила → обращается к Repository
        ↓
Repository → EF Core → SQL Server
        ↓
Результат поднимается обратно
        ↓
ViewModel обновляет свойство → OnPropertyChanged
        ↓
View автоматически перерисовывается
```

## Навигация

Используется самописный `NavigationService` без Frame/Page — вся навигация через смену `CurrentViewModel` в `MainViewModel`.

```csharp
// MainViewModel
public ViewModelBase? CurrentViewModel { get; private set; }
```

В `MainWindow.xaml` ContentControl привязан к `CurrentViewModel`, а DataTemplates сопоставляют тип ViewModel с нужным View:

```xml
<DataTemplate DataType="{x:Type vm:AdvertisementsViewModel}">
    <views:AdvertisementsView/>
</DataTemplate>
```

При навигации `NavigationService.Navigate(viewModel)` заменяет текущий ViewModel → WPF автоматически подбирает нужный шаблон.

## Repository + Unit of Work

Все операции с БД проходят через интерфейс `IUnitOfWork`:

```csharp
public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IAdvertisementRepository Advertisements { get; }
    ICategoryRepository Categories { get; }
    IRepository<FavoriteAdvertisement> Favorites { get; }
    IRepository<AppLog> AppLogs { get; }

    Task<int> SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
}
```

Сервисы принимают `IUnitOfWork` в конструктор. Это позволяет легко изменить источник данных без правки сервисов.

Специализированные репозитории (`IAdvertisementRepository`) добавляют методы для сложных запросов с `Include`:

```csharp
IQueryable<Advertisement> QueryWithDetails();  // Include User, Category, FavoriteAdvertisements
Task<Advertisement?> GetByIdWithDetailsAsync(int id);
Task<List<Advertisement>> GetByUserAsync(int userId);
```

## Управление состоянием

### Undo/Redo

`UndoRedoService` хранит стек действий. Каждое действие (добавление, редактирование, удаление объявления) реализует интерфейс `IUndoRedoAction` с методами `UndoAsync()` и `RedoAsync()`.

### Авторизация

`AuthService.CurrentUser` — текущий пользователь. При смене пользователя вызывается событие `CurrentUserChanged`, на которое подписан `MainViewModel` для обновления видимости кнопок навигации.

### Темы и локализация

`ThemeService` и `LocalizationService` работают по одному принципу: заменяют `ResourceDictionary` в `Application.Current.Resources.MergedDictionaries`. Смена происходит мгновенно без перезапуска приложения.

## Жизненный цикл запроса (пример: поиск объявлений)

1. Пользователь вводит текст в поисковую строку
2. `SearchText` setter в `AdvertisementsViewModel` вызывает `ReloadAfterChange()`
3. `LoadAsync()` формирует `AdvertisementFilter` и вызывает `GetPagedAsync(filter, page, pageSize)`
4. `AdvertisementService.GetPagedAsync()` строит LINQ-запрос через `QueryWithDetails()`, применяет фильтры, считает `TotalCount`, берёт нужную страницу через `Skip/Take`
5. Результат возвращается как `PagedResult<Advertisement>`
6. ViewModel создаёт `AdvertisementCardViewModel`-обёртки с флагом `IsFavorite` для каждого объявления
7. `ObservableCollection<AdvertisementCardViewModel>` обновляется → WPF перерисовывает список

## Состав файлов по слоям

| Слой | Папка | Файлы |
|------|-------|-------|
| Model | `Models/` | Advertisement, User, Category, FavoriteAdvertisement, AppLog, enums |
| Data | `Data/` | AppDbContext, DbInitializer |
| Repository | `Repositories/` | IRepository, Repository, IUnitOfWork, UnitOfWork, + специальные |
| Service | `Services/` | AdvertisementService, AuthService, UserService, CategoryService, FavoriteService, AppLogService, EmailService, ExportService, ImageService, ThemeService, LocalizationService, NavigationService, DialogService, UndoRedoService, PagedResult |
| ViewModel | `ViewModels/` | MainViewModel, AdvertisementsViewModel, AdvertisementCardViewModel, AdvertisementDetailsViewModel, AddEditAdvertisementViewModel, ProfileViewModel, AdminPanelViewModel, LoginViewModel, RegisterViewModel, UsersManagementViewModel, CategoriesViewModel |
| View | `Views/` | AdvertisementsView, AdvertisementDetailsView, AddEditAdvertisementView, ProfileView, AdminPanelView, LoginView, RegisterView |
| UserControl | `UserControls/` | AdvertisementCardControl, SearchFilterControl, ContactInfoControl |
| Resource | `Resources/` | Styles.xaml, 4 темы, 2 языковых словаря |
| Converter | `Converters/` | EnumToStringConverter, ImagePathConverter, NullToVisibilityConverter, RoleToVisibilityConverter, InitialsConverter, BooleanToHeartConverter |
