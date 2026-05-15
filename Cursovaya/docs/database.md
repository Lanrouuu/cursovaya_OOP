# База данных TradeAds

## Технологии

- **СУБД:** Microsoft SQL Server (localhost)
- **ORM:** Entity Framework Core 8, Code First
- **База:** `TradeAdsDb`
- **Подход:** миграции через `dotnet ef migrations`

## Схема таблиц

### Users

| Столбец | Тип | Ограничения |
|---------|-----|-------------|
| Id | int | PK, Identity |
| UserName | nvarchar(50) | NOT NULL |
| Email | nvarchar(100) | NOT NULL, Unique Index |
| PhoneNumber | nvarchar(20) | NOT NULL |
| PasswordHash | nvarchar(64) | NOT NULL |
| Role | int | NOT NULL (0=User, 1=Admin) |
| IsBlocked | bit | NOT NULL, default 0 |
| CreatedAt | datetime2 | NOT NULL |
| UpdatedAt | datetime2 | nullable |

### Categories

| Столбец | Тип | Ограничения |
|---------|-----|-------------|
| Id | int | PK, Identity |
| Name | nvarchar(80) | NOT NULL |
| Description | nvarchar(500) | NOT NULL |
| IsActive | bit | NOT NULL, default 1 |

### Advertisements

| Столбец | Тип | Ограничения |
|---------|-----|-------------|
| Id | int | PK, Identity |
| Title | nvarchar(120) | NOT NULL |
| ShortDescription | nvarchar(250) | NOT NULL |
| FullDescription | nvarchar(2000) | NOT NULL |
| Price | decimal(18,2) | NOT NULL |
| City | nvarchar(80) | NOT NULL |
| Condition | int | NOT NULL (0=New, 1=Used, 2=Refurbished) |
| Status | int | NOT NULL (0=Active, 1=Hidden, 2=Blocked, 3=Deleted) |
| ImagePath | nvarchar(500) | NOT NULL, default '' |
| SellerContactEmail | nvarchar(100) | NOT NULL |
| SellerContactPhone | nvarchar(20) | NOT NULL |
| ViewCount | int | NOT NULL, default 0 |
| CreatedAt | datetime2 | NOT NULL |
| UpdatedAt | datetime2 | nullable |
| ExpiresAt | datetime2 | nullable |
| UserId | int | FK → Users.Id, RESTRICT |
| CategoryId | int | FK → Categories.Id, RESTRICT |

**Индексы:** `IX_Advertisements_UserId`, `IX_Advertisements_CategoryId`, `IX_Advertisements_Status`, `IX_Advertisements_CreatedAt`

### FavoriteAdvertisements

| Столбец | Тип | Ограничения |
|---------|-----|-------------|
| Id | int | PK, Identity |
| UserId | int | FK → Users.Id, CASCADE |
| AdvertisementId | int | FK → Advertisements.Id, CASCADE |
| CreatedAt | datetime2 | NOT NULL |

### AppLogs

| Столбец | Тип | Ограничения |
|---------|-----|-------------|
| Id | int | PK, Identity |
| Action | nvarchar(max) | NOT NULL |
| Description | nvarchar(max) | NOT NULL |
| CreatedAt | datetime2 | NOT NULL |
| AdminUserId | int | nullable |

## Связи

```
Users ──< Advertisements        (один-ко-многим, RESTRICT — удаление пользователя
                                  не удаляет объявления)

Categories ──< Advertisements   (один-ко-многим, RESTRICT — удаление категории
                                  невозможно при наличии объявлений)

Users ──< FavoriteAdvertisements (один-ко-многим, CASCADE)
Advertisements ──< FavoriteAdvertisements (один-ко-многим, CASCADE)
```

## Миграции

### InitialCreate (`20260502233000_InitialCreate`)

Создаёт таблицы `Users`, `Categories`, `Advertisements`, `FavoriteAdvertisements`, `AppLogs` с базовыми полями и индексами FK.

### AddPerformanceAndFeatures (`20260515120000_AddPerformanceAndFeatures`)

Добавляет:
- `Advertisements.ExpiresAt` — срок жизни объявления
- `Advertisements.ViewCount` — счётчик просмотров
- `Users.UpdatedAt` — дата последнего изменения профиля
- Индексы `IX_Advertisements_Status` и `IX_Advertisements_CreatedAt`

## Начальное заполнение (DbInitializer)

`DbInitializer.InitializeAsync()` вызывается при каждом запуске. Добавляет данные только если таблицы пустые:

**Пользователи:**
- `admin@mail.com` / `admin123` — роль Admin
- `user@mail.com` / `user123` — роль User

**Категории:** Электроника, Одежда, Мебель, Транспорт, Недвижимость, Хобби, Другое

**Объявления:** несколько тестовых объявлений от тестовых пользователей

## Настройки в AppDbContext

Настроено через Fluent API в `OnModelCreating`:

```csharp
// Точность цены
builder.Property(a => a.Price).HasColumnType("decimal(18,2)");

// Каскадное удаление избранного
builder.HasMany(u => u.FavoriteAdvertisements)
       .WithOne(f => f.User)
       .OnDelete(DeleteBehavior.Cascade);

// Ограничение удаления пользователя с объявлениями
builder.HasMany(u => u.Advertisements)
       .WithOne(a => a.User)
       .OnDelete(DeleteBehavior.Restrict);
```

## FavoritesCount

`Advertisement.FavoritesCount` — вычисляемое свойство C# (не хранится в БД):

```csharp
public int FavoritesCount => FavoriteAdvertisements.Count;
```

Работает корректно когда `QueryWithDetails()` включает `.Include(x => x.FavoriteAdvertisements)`.

## Безопасность

- Пароли хранятся как SHA-256 хэш (через `PasswordService.HashPassword`)
- Прямого хранения паролей нет
- Email проверяется на уникальность перед регистрацией
- Все пользовательские строки проходят `.Trim()` перед сохранением
- Длины строк ограничены на уровне валидации и модели EF
