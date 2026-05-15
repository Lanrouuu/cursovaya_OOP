# TradeAds — информационная система торговых объявлений

Курсовой WPF-проект на C# / .NET 8. Доска объявлений с авторизацией, модерацией, избранным, пагинацией, Undo/Redo, темами и локализацией. Не интернет-магазин: нет корзины, оплаты и доставки.

---

## Технологии

- **Платформа:** C# 12, .NET 8, WPF
- **БД:** Microsoft SQL Server, Entity Framework Core 8, Code First, миграции
- **ORM:** LINQ to Entity, `IQueryable`, `async/await`
- **Архитектура:** MVVM, Repository + Unit of Work, Service Layer
- **UI:** ResourceDictionary, DataTemplate-навигация, UserControl, DependencyProperty, RoutedCommand
- **Безопасность:** SHA-256 хэширование паролей, PasswordBox + attached property
- **Email:** `System.Net.Mail.SmtpClient` (опционально через `appsettings.json`)
- **Прочее:** CSV-экспорт, Drag & Drop, системная тема Windows через реестр

---

## Быстрый старт

### 1. Требования

- Visual Studio 2022 (WPF / Desktop workload)
- Microsoft SQL Server (localhost) или SQL Server Express

### 2. Подключение к базе данных

Откройте `Cursovaya/appsettings.json` и при необходимости измените строку:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TradeAdsDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "From": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

> **SMTP опционально.** Если `From` начинается с `your-`, письма не отправляются — приложение работает без ошибок.

### 3. Запуск

Откройте `Cursovaya.sln` в Visual Studio и нажмите **F5**.

При запуске автоматически:
- Применяются все миграции (`Database.MigrateAsync()`)
- Создаётся база `TradeAdsDb` если её нет
- Добавляются начальные данные (`DbInitializer`): пользователи, категории, объявления

---

## Тестовые аккаунты

| Роль | Email | Пароль |
|------|-------|--------|
| Администратор | `admin@mail.com` | `admin123` |
| Пользователь | `user@mail.com` | `user123` |

---

## Что реализовано

### Авторизация и аккаунт
- Регистрация: имя (2–50 симв.), email (regex), телефон (regex), пароль (мин. 6 симв.)
- Авторизация по email + пароль (SHA-256 хэш)
- Пароли скрыты через `PasswordBox` + `PasswordBoxHelper` (attached property)
- Блокировка аккаунта администратором
- Смена пароля в личном кабинете

### Объявления
- Список с пагинацией: 10 на странице, кнопки ← / →, индикатор «X / Y»
- Полнотекстовый поиск по названию, описанию, городу, имени продавца
- Фильтры: категория, цена от/до, город, состояние, дата, сортировка — применяются мгновенно
- Детальная карточка: изображение, описание, контакты продавца, счётчик просмотров
- Добавление и редактирование: все поля + выбор изображения + Drag & Drop
- Программное удаление (Status=Deleted) с поддержкой Undo
- Статус-бейджи на карточках: Hidden → оранжевый, Blocked → красный

### Избранное
- Кнопка ❤/♡ на каждой карточке (только для авторизованных)
- Toggle без перезагрузки страницы, счётчик обновляется мгновенно
- Список избранных объявлений в личном кабинете

### Срок жизни объявлений
- При создании: `ExpiresAt = now + 30 дней`
- Автоматическое скрытие просроченных при каждом открытии списка
- Email-предупреждение за 3 дня до истечения
- Кнопка «Продлить» в профиле для скрытых объявлений

### Счётчик просмотров
- Поле `ViewCount` в таблице `Advertisements`
- Инкремент при каждом открытии карточки
- Отображается на карточке: 👁 42

### Похожие объявления
- В детальной карточке — горизонтальный скролл: до 5 из той же категории

### Undo / Redo
- Стек для добавления, редактирования и удаления объявлений
- Кнопки в шапке, активны только при наличии действий
- Стек не сбрасывается при навигации

### Темы оформления
- Light (светлая), Dark (тёмная), Optimistic (янтарная), Blue (синяя)
- Определяется автоматически по системным настройкам Windows при первом запуске
- Смена мгновенная через `DynamicResource` без перезапуска

### Локализация
- Русский и английский язык
- Переключение мгновенное
- 107 ключей локализации + `EnumToStringConverter` для значений enum

### Администрирование
- **Пользователи:** таблица, блокировка/разблокировка, email-уведомления
- **Объявления:** таблица, смена статуса (Active/Hidden/Blocked), удаление, экспорт CSV
- **Категории:** добавление, редактирование, удаление (если нет объявлений)
- **Логи:** таблица последних 200 действий администраторов

### Email-уведомления
- Регистрация, блокировка/разблокировка аккаунта
- Блокировка/активация объявления администратором
- Предупреждение за 3 дня до истечения объявления

### Экспорт в CSV
- Пользователи и объявления — через `SaveFileDialog`
- UTF-8, разделитель `;`, RFC 4180 экранирование

---

## Структура проекта

```
Cursovaya/
├── Models/                  # Сущности: User, Advertisement, Category, FavoriteAdvertisement, AppLog
│   └── Enums/               # UserRole, AdvertisementStatus, ItemCondition
├── Data/                    # AppDbContext, DbInitializer
├── Migrations/              # InitialCreate, AddPerformanceAndFeatures
├── Repositories/            # IRepository, Repository, IUnitOfWork, UnitOfWork,
│                            # IAdvertisementRepository, AdvertisementRepository, IUserRepository
├── Services/                # AdvertisementService, AuthService, UserService, CategoryService,
│                            # FavoriteService, AppLogService, EmailService, ExportService,
│                            # ImageService, ThemeService, LocalizationService,
│                            # NavigationService, DialogService, UndoRedoService, PagedResult
├── ViewModels/              # MainViewModel, AdvertisementsViewModel, AdvertisementCardViewModel,
│                            # AdvertisementDetailsViewModel, AddEditAdvertisementViewModel,
│                            # ProfileViewModel, AdminPanelViewModel, LoginViewModel,
│                            # RegisterViewModel, UsersManagementViewModel, CategoriesViewModel
├── Views/                   # AdvertisementsView, AdvertisementDetailsView,
│                            # AddEditAdvertisementView, ProfileView, AdminPanelView,
│                            # LoginView, RegisterView
├── UserControls/            # AdvertisementCardControl, SearchFilterControl, ContactInfoControl
├── Resources/
│   ├── Styles.xaml          # Все стили приложения
│   ├── Themes/              # LightTheme, DarkTheme, OptimisticTheme, BlueTheme
│   └── Languages/           # Strings.ru-RU.xaml, Strings.en-US.xaml
├── Converters/              # EnumToStringConverter, ImagePathConverter, NullToVisibilityConverter,
│                            # RoleToVisibilityConverter, InitialsConverter, BooleanToHeartConverter
├── Helpers/                 # PasswordBoxHelper (attached property для PasswordBox + MVVM)
├── Commands/                # CustomCommands (RoutedUICommand)
├── Assets/UserImages/       # Загруженные изображения объявлений (GUID-имена)
├── appsettings.json         # Строка подключения, SMTP-настройки
└── App.xaml.cs              # Инициализация: БД, сервисы, тема, главное окно
```

---

## База данных

**5 таблиц:**

| Таблица | Назначение |
|---------|-----------|
| `Users` | Пользователи (id, имя, email, телефон, хэш пароля, роль, блокировка) |
| `Advertisements` | Объявления (все поля, статус, ExpiresAt, ViewCount, FK на Users и Categories) |
| `Categories` | Категории (название, описание, IsActive) |
| `FavoriteAdvertisements` | Связь пользователь ↔ объявление (CASCADE при удалении) |
| `AppLogs` | Журнал действий администратора |

**Миграции:**
1. `20260502233000_InitialCreate` — создание всех таблиц
2. `20260515120000_AddPerformanceAndFeatures` — ExpiresAt, ViewCount, UpdatedAt, индексы на Status и CreatedAt

---

## Документация

| Файл | Содержимое |
|------|-----------|
| `docs/architecture.md` | Архитектура, MVVM, навигация, Repository/UoW, жизненный цикл запроса |
| `docs/database.md` | Схема таблиц, связи, миграции, безопасность |
| `docs/features.md` | Полное описание всей функциональности |
| `docs/services.md` | Справочник API всех сервисов |
| `docs/ui.md` | UI-компоненты, UserControls, темы, локализация, стили |
| `docs/defense.md` | Вопросы и ответы для защиты курсовой |

---

## Что проверить вручную

- Доступность SQL Server по имени `localhost`
- Вход под тестовыми аккаунтами
- Добавление объявления с загрузкой изображения через диалог
- Добавление объявления перетаскиванием файла на превью (Drag & Drop)
- Переключение темы и языка в личном кабинете
- Добавление в избранное (❤ / ♡) без перезагрузки страницы
- Пагинация при наличии более 10 объявлений
- Undo/Redo для добавления и удаления объявления
- Модерация в админ-панели: смена статуса, блокировка пользователя
- Экспорт пользователей и объявлений в CSV
