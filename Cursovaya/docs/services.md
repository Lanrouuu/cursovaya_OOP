# Сервисный слой TradeAds

Все сервисы создаются вручную в `App.xaml.cs` и передаются через конструкторы (без IoC-контейнера).

---

## AdvertisementService

Основной сервис работы с объявлениями.

### Методы

| Метод | Описание |
|-------|----------|
| `GetFilteredAsync(filter, includeInactive)` | Список объявлений с фильтрами, без пагинации |
| `GetPagedAsync(filter, includeInactive, page, pageSize)` | Страница объявлений. Возвращает `PagedResult<Advertisement>` с `TotalCount`, `TotalPages` |
| `GetSimilarAsync(categoryId, excludeId, count)` | Похожие объявления из той же категории |
| `GetByIdAsync(id)` | Объявление по Id + инкремент `ViewCount` |
| `GetByUserAsync(userId)` | Объявления пользователя (кроме удалённых) |
| `AddAsync(advertisement, currentUser)` | Создание объявления. Устанавливает `Status=Active`, `ExpiresAt=+30дней`, регистрирует Undo |
| `UpdateAsync(changedAdvertisement, currentUser)` | Обновление редактируемых полей, регистрирует Undo |
| `DeleteAsync(advertisementId, currentUser)` | Программное удаление (`Status=Deleted`), регистрирует Undo |
| `RenewAsync(advertisementId, currentUser)` | Продлить объявление на 30 дней, вернуть `Status=Active` |
| `ChangeStatusAsync(advertisementId, status, currentUser)` | Смена статуса (только Admin). Пишет в `AppLogs`, отправляет email |
| `ExpireOverdueAsync()` | Скрывает объявления с истёкшим `ExpiresAt` |
| `NotifyExpiringAsync()` | Отправляет email для объявлений, истекающих ровно через 3 дня |

### Правила доступа
- Редактировать/удалять может автор или Admin
- Менять статус через `ChangeStatusAsync` — только Admin
- Добавлять — только авторизованный пользователь

### Валидация объявления
Название (≤120), краткое описание (≤250), полное описание (≤2000), цена (0–999 999 999), категория (выбрана), город (≤80), email (regex), телефон (regex).

---

## AuthService

| Метод / Свойство | Описание |
|-----------------|----------|
| `CurrentUser` | Текущий авторизованный пользователь (null если гость) |
| `IsLoggedIn` | true если пользователь вошёл |
| `IsAdmin` | true если роль Admin |
| `CurrentUserChanged` | Событие при смене пользователя |
| `LoginAsync(email, password)` | Авторизация. Проверяет хэш и статус блокировки |
| `RegisterAsync(userName, email, phone, password, confirmPassword)` | Регистрация с полной валидацией |
| `Logout()` | Очищает `CurrentUser` |
| `UpdateCurrentUserAsync()` | Перечитывает данные текущего пользователя из БД |
| `IsEmailValid(email)` | Статический метод проверки email (regex) |
| `IsPhoneValid(phone)` | Статический метод проверки телефона (regex) |

---

## UserService

| Метод | Описание |
|-------|----------|
| `GetAllAsync()` | Все пользователи |
| `UpdateProfileAsync(user, userName, email, phone)` | Обновление профиля с валидацией |
| `ChangePasswordAsync(user, currentPassword, newPassword, confirmPassword)` | Смена пароля |
| `SetBlockedAsync(userId, isBlocked, adminUser)` | Блокировка / разблокировка. Только Admin. Отправляет email |

---

## FavoriteService

| Метод | Описание |
|-------|----------|
| `GetFavoriteIdsAsync(userId)` | `HashSet<int>` с Id избранных объявлений пользователя |
| `GetFavoritesAsync(userId)` | Список полных объявлений (только Active) |
| `ToggleAsync(userId, advertisementId)` | Добавить если нет, удалить если есть. Возвращает `true` если добавлено |

---

## CategoryService

| Метод | Описание |
|-------|----------|
| `GetAllAsync()` | Все категории |
| `GetActiveAsync()` | Только активные категории (для фильтров) |
| `AddAsync(name, description)` | Добавление |
| `UpdateAsync(category)` | Обновление |
| `DeleteAsync(categoryId)` | Удаление (запрещено если есть объявления) |

---

## AppLogService

| Метод | Описание |
|-------|----------|
| `GetRecentAsync(count = 200)` | Последние N записей журнала, сортировка от новых к старым |

---

## EmailService

Работает через `System.Net.Mail.SmtpClient`. Если SMTP не настроен (`From` начинается с `your-`), письма не отправляются без ошибки.

| Метод | Тема письма |
|-------|------------|
| `SendWelcomeAsync(email, userName)` | «Добро пожаловать в TradeAds!» |
| `SendAccountBlockedAsync(email, userName)` | «Ваш аккаунт заблокирован» |
| `SendAccountUnblockedAsync(email, userName)` | «Ваш аккаунт разблокирован» |
| `SendAdvertisementBlockedAsync(email, userName, title)` | «Ваше объявление заблокировано» |
| `SendAdvertisementActivatedAsync(email, userName, title)` | «Ваше объявление активировано» |
| `SendExpiringWarningAsync(email, userName, title, daysLeft)` | «Ваше объявление скоро истечёт» |

---

## ExportService

| Метод | Описание |
|-------|----------|
| `ExportUsers(users)` | SaveFileDialog → CSV пользователей |
| `ExportAdvertisements(advertisements)` | SaveFileDialog → CSV объявлений |

Строки с символами `;`, `"`, переносом строки экранируются через двойные кавычки.

---

## ImageService

| Метод | Описание |
|-------|----------|
| `CopyToUserImages(sourcePath)` | Проверяет расширение, копирует в `Assets/UserImages/` с GUID-именем |
| `IsAllowedImageExtension(filePath)` | Проверяет расширение (jpg, jpeg, png, gif, bmp, webp) |

---

## ThemeService

| Метод / Свойство | Описание |
|-----------------|----------|
| `Themes` | Список: Light, Dark, Optimistic, Blue |
| `CurrentTheme` | Текущая тема |
| `GetSystemTheme()` | Читает реестр Windows, возвращает «Light» или «Dark» |
| `ApplyTheme(themeName)` | Заменяет ResourceDictionary темы |

---

## LocalizationService

| Метод / Свойство | Описание |
|-----------------|----------|
| `CurrentLanguage` | «ru-RU» или «en-US» |
| `ApplyLanguage(language)` | Заменяет ResourceDictionary языка |

---

## UndoRedoService

| Метод / Свойство | Описание |
|-----------------|----------|
| `CanUndo` / `CanRedo` | Доступность действий |
| `StateChanged` | Событие при изменении стека |
| `AddAction(action)` | Добавить действие в стек |
| `UndoAsync()` | Отменить последнее действие |
| `RedoAsync()` | Повторить отменённое действие |

Действия: `AddedAdvertisementAction`, `EditedAdvertisementAction`, `DeletedAdvertisementAction`.

---

## NavigationService

| Метод | Описание |
|-------|----------|
| `Navigate(viewModel)` | Устанавливает текущий ViewModel |
| `CurrentViewModelChanged` | Событие при навигации |

---

## DialogService

| Метод | Описание |
|-------|----------|
| `ShowMessage(message)` | MessageBox с информацией |
| `ShowError(message)` | MessageBox с ошибкой |
| `Confirm(message)` | MessageBox Да/Нет, возвращает bool |
| `SelectImagePath()` | OpenFileDialog для выбора изображения |

---

## PagedResult\<T\>

Возвращается `GetPagedAsync`. Содержит:

```csharp
public List<T> Items { get; }       // элементы текущей страницы
public int TotalCount { get; }      // всего записей
public int Page { get; }            // текущая страница (1-based)
public int PageSize { get; }        // размер страницы
public int TotalPages { get; }      // Math.Ceiling(TotalCount / PageSize)
public bool HasPrevious { get; }    // Page > 1
public bool HasNext { get; }        // Page < TotalPages
```
