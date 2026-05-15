# Интерфейс TradeAds

## Структура окна

```
┌─────────────────────────────────────────────────────┐
│  HEADER (MainWindow — всегда виден)                 │
│  [TradeAds] [Объявления] [Профиль] [Войти] [Undo/Redo] │
├─────────────────────────────────────────────────────┤
│                                                     │
│  CONTENT AREA (ContentControl → DataTemplate)       │
│  ← текущий ViewModel определяет, какой View здесь  │
│                                                     │
└─────────────────────────────────────────────────────┘
```

`MainWindow.xaml` содержит только шапку (Header) и `ContentControl`, привязанный к `MainViewModel.CurrentViewModel`. Каждому типу ViewModel сопоставлен свой `DataTemplate` в разделе ресурсов MainWindow:

```xml
<DataTemplate DataType="{x:Type vm:AdvertisementsViewModel}">
    <views:AdvertisementsView/>
</DataTemplate>
<DataTemplate DataType="{x:Type vm:LoginViewModel}">
    <views:LoginView/>
</DataTemplate>
<!-- ... и т.д. -->
```

---

## Экраны (Views)

### AdvertisementsView

Главный экран. Состоит из:

1. **SearchFilterControl** (UserControl) — панель фильтров в верхней части
2. **Строка результатов** — «Найдено: N объявлений» + кнопка «Добавить объявление»
3. **ScrollViewer** с `ItemsControl` — список карточек `AdvertisementCardControl`
4. **Пагинация** — ← / PageInfo / →, видна только если `ShowPagination = true` (TotalPages > 1)
5. **Пустое состояние** — иллюстрация + текст, если объявлений нет

Привязки ItemsControl:
```xml
<ItemsControl ItemsSource="{Binding Advertisements}">
    <!-- Advertisements — ObservableCollection<AdvertisementCardViewModel> -->
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <uc:AdvertisementCardControl
                Title="{Binding Advertisement.Title}"
                Price="{Binding Advertisement.Price}"
                IsFavorite="{Binding IsFavorite}"
                ToggleFavoriteCommand="{Binding ToggleFavoriteCommand}"
                .../>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

### AdvertisementDetailsView

Карточка деталей одного объявления:

- Большое изображение (или заглушка «Нет изображения»)
- Название, цена, категория, продавец, город, дата, состояние
- Счётчик просмотров 👁 и избранного ❤
- Кнопка «Показать контакты» → раскрывает `ContactInfoControl`
- Блок «Похожие объявления» (горизонтальный `WrapPanel`) — до 5 объявлений из той же категории
- Кнопки Редактировать / Удалить (видны только автору и Admin)

### AddEditAdvertisementView

Форма добавления и редактирования:

- Левая колонка: поля названия, описаний, цены, категории, состояния, города
- Правая колонка: поле email продавца, телефона, превью изображения + кнопка «Выбрать» + Drag & Drop
- Кнопки «Сохранить» / «Отмена»

**Drag & Drop:** `ImageDropBorder` имеет `AllowDrop="True"`. При перетаскивании файла:
- `DragOver` → проверяет `FileDrop`, устанавливает `DragDropEffects.Copy`
- `Drop` → берёт первый файл, вызывает `vm.ApplyDroppedImage(filePath)`

### LoginView / RegisterView

Формы авторизации и регистрации:

- Поля ввода со стилем `WatermarkTextBoxStyle` (placeholder через `Tag`)
- `PasswordBox` со стилем `WatermarkPasswordBoxStyle` + `PasswordBoxHelper` для MVVM-биндинга
- Кнопки «Войти» / «Зарегистрироваться»

**PasswordBoxHelper** (`Helpers/PasswordBoxHelper.cs`) — attached property `BoundPassword`, синхронизирует `PasswordBox.Password` с ViewModel через `PasswordChanged` событие, не нарушая MVVM.

### ProfileView

Личный кабинет, разбит на секции (ScrollViewer → StackPanel):

1. **Аватар** — круглый Border с инициалами пользователя (`InitialsConverter`)
2. **Редактирование профиля** — имя, email, телефон + кнопка «Сохранить»
3. **Смена пароля** — текущий, новый, подтверждение пароля
4. **Настройки интерфейса** — ComboBox темы, ComboBox языка
5. **Мои объявления** — DataGrid с колонками: название, статус, дата, истекает, кнопки «Изменить»/«Продлить»/«Удалить»
6. **Избранное** — DataGrid с активными избранными объявлениями

### AdminPanelView

Панель администратора с `TabControl`:

| Вкладка | Содержимое |
|---------|-----------|
| Пользователи | `UsersManagementView` (отдельный UserControl или View) — таблица, кнопки блокировки |
| Объявления | DataGrid: название, категория, продавец, статус, кнопки изменения статуса и удаления |
| Категории | `CategoriesView` — DataGrid + форма добавления/редактирования |
| Логи | DataGrid с 200 последними записями `AppLogs` (дата, действие, описание, ID адм.) |

Кнопки экспорта CSV на вкладках «Пользователи» и «Объявления».

---

## UserControls

### AdvertisementCardControl

**Файл:** `UserControls/AdvertisementCardControl.xaml` + `.cs`

Горизонтальная карточка объявления. Отображает:

- Миниатюру (Image с конвертером `ImagePathConverter`)
- Название (кликабельно → команда `OpenAdvertisementCommand`)
- Краткое описание
- Теги: категория, продавец, город, дата
- Статус-бейдж (Hidden → оранжевый, Blocked → красный, Active → скрыт)
- Цену (или «Бесплатно» если 0)
- Строку статистики: 👁 ViewCount · ❤/♡ FavoritesCount
- Кнопки: «Подробнее», «Редактировать», «Удалить»

**DependencyProperty в code-behind:**

| Свойство | Тип | Назначение |
|---------|-----|-----------|
| `Title` | string | Название |
| `ShortDescription` | string | Краткое описание |
| `Price` | decimal | Цена |
| `CategoryName` | string | Имя категории |
| `SellerName` | string | Имя продавца |
| `City` | string | Город |
| `CreatedAt` | DateTime | Дата создания |
| `ImagePath` | string | Путь к изображению |
| `Status` | AdvertisementStatus | Для статус-бейджа |
| `ViewCount` | int | Счётчик просмотров |
| `FavoritesCount` | int | Счётчик избранного |
| `IsFavorite` | bool | Состояние кнопки ❤/♡ |
| `ToggleFavoriteCommand` | ICommand | Команда переключения |
| `EditCommand` / `EditCommandParameter` | ICommand / object | Редактировать |
| `DeleteCommand` / `DeleteCommandParameter` | ICommand / object | Удалить |
| `OpenCommandParameter` | object | Параметр команды открытия |

**RoutedCommand `OpenAdvertisementCommand`** определён в `Commands/CustomCommands.cs`, обработчик в code-behind передаёт управление через `OpenCommand` DependencyProperty.

**Кнопка ❤/♡:**
```xml
<Button Command="{Binding ToggleFavoriteCommand, ElementName=Root}">
    <TextBlock>
        <Run Text="{Binding IsFavorite, ElementName=Root,
             Converter={StaticResource BooleanToHeartConverter}}"/>
        <Run Text="{Binding FavoritesCount, ElementName=Root}"/>
    </TextBlock>
</Button>
```

### SearchFilterControl

**Файл:** `UserControls/SearchFilterControl.xaml`

Горизонтальная панель фильтров с 9 колонками:

| Колонка | Элемент | Привязка |
|---------|---------|---------|
| Текстовый поиск | TextBox | `SearchText` |
| Категория | ComboBox | `SelectedCategory` |
| Цена от | TextBox | `MinPriceText` |
| Цена до | TextBox | `MaxPriceText` |
| Город | TextBox | `City` |
| Состояние | ComboBox | `SelectedCondition` |
| Дата от | DatePicker | `DateFrom` |
| Сортировка | ComboBox | `SelectedSortOption` |
| — | Кнопка «Сбросить» | `ClearFiltersCommand` |

Все текстовые поля используют `UpdateSourceTrigger=PropertyChanged` — фильтры применяются мгновенно.

### ContactInfoControl

**Файл:** `UserControls/ContactInfoControl.xaml`

Отображает контакты продавца: email и телефон. Видимость управляется из `AdvertisementDetailsViewModel` через свойство `ContactsVisible`.

---

## Конвертеры

| Конвертер | Назначение |
|----------|-----------|
| `EnumToStringConverter` | `UserRole`, `ItemCondition`, `AdvertisementStatus` → строка на текущем языке |
| `ImagePathConverter` | Строка пути → `BitmapImage` (или null для заглушки) |
| `NullToVisibilityConverter` | null → Collapsed, не-null → Visible |
| `RoleToVisibilityConverter` | `UserRole` → Visibility (для скрытия кнопок от пользователей) |
| `InitialsConverter` | «Иван Петров» → «ИП» (первые буквы слов, не более 2) |
| `BooleanToHeartConverter` | `true` → «❤», `false` → «♡» |

---

## Система тем

**Файл сервиса:** `Services/ThemeService.cs`

Доступно 4 темы, каждая в отдельном ResourceDictionary:

| Тема | Файл | Описание |
|------|------|---------|
| Light | `Resources/Themes/LightTheme.xaml` | Светлый фон (#F1F5F9), тёмно-синяя шапка (#1E293B) |
| Dark | `Resources/Themes/DarkTheme.xaml` | Тёмный фон (#0F172A), все поверхности в тёмных тонах |
| Optimistic | `Resources/Themes/OptimisticTheme.xaml` | Тёплый янтарно-оранжевый фон |
| Blue | `Resources/Themes/BlueTheme.xaml` | Холодная сине-голубая палитра |

Каждый файл темы определяет один набор именованных кистей (`SolidColorBrush`):

```
WindowBackgroundBrush  SidebarBrush      HeaderBrush
TextBrush              MutedTextBrush    DisabledBrush
CardBackgroundBrush    BorderBrush       InputBackgroundBrush
HoverBrush             AccentBrush       AccentHoverBrush
DangerBrush            SuccessBrush      WarningBrush
PriceBrush             HeaderTextBrush   HeaderMutedBrush
...и другие (всего ~35 кистей)
```

Все стили в `Styles.xaml` используют `{DynamicResource ...}` — смена темы заменяет словарь и все элементы перекрашиваются немедленно:

```csharp
var existing = Application.Current.Resources.MergedDictionaries
    .FirstOrDefault(d => d.Source?.ToString().Contains("Theme") == true);
if (existing != null)
    Application.Current.Resources.MergedDictionaries.Remove(existing);
Application.Current.Resources.MergedDictionaries.Add(newTheme);
CurrentTheme = themeName;
```

**Системная тема** определяется из реестра Windows при старте:
```csharp
using var key = Registry.CurrentUser.OpenSubKey(
    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
var value = key?.GetValue("AppsUseLightTheme");
return value is int v && v == 0 ? "Dark" : "Light";
```

---

## Локализация

**Файл сервиса:** `Services/LocalizationService.cs`

Два языка:

| Язык | Файл |
|------|------|
| Русский | `Resources/Languages/Strings.ru-RU.xaml` |
| Английский | `Resources/Languages/Strings.en-US.xaml` |

Каждый файл содержит `sys:String` ресурсы с ключами:

```xml
<sys:String x:Key="AppTitle">TradeAds</sys:String>
<sys:String x:Key="NavAdvertisements">Объявления</sys:String>
...
```

В XAML используются через `{DynamicResource ключ}`. Смена языка аналогична смене темы — замена ResourceDictionary.

**Всего ключей локализации:** 107 (русский и английский полностью совпадают по набору).

`EnumToStringConverter` локализует enum-значения отдельно, через `CultureInfo.CurrentUICulture`.

---

## Стили (Styles.xaml)

Основной файл стилей: `Resources/Styles.xaml`. Определяет:

**Кнопки:**
- `PrimaryButtonStyle` — акцентная кнопка (AccentBrush)
- `SecondaryButtonStyle` — обводная кнопка
- `DangerButtonStyle` — красная кнопка
- `SmallButtonStyle` / `SmallSecondaryButtonStyle` — компактные варианты
- `TopNavigationButtonStyle` — кнопка в шапке (HeaderTextBrush, подсветка при hover)

**Текстовые поля:**
- `WatermarkTextBoxStyle` — TextBox с placeholder через `Tag` и Trigger на пустую строку
- `WatermarkPasswordBoxStyle` — аналог для PasswordBox

**Метки:**
- `FilterLabelStyle` — метка над полем фильтра (MutedTextBrush, FontSize 12)
- `FieldLabelStyle` — метка над полем формы (MutedTextBrush, FontSize 13, SemiBold)

**Карточки:**
- `CardBorderStyle` — белый Border с тенью (BoxShadow-эффект через Opacity)
- `AdvertisementCardBorderStyle` — карточка объявления с hover-эффектом
- `TagStyle` — маленький тег-пилюля (AccentLightBrush фон)

**Текст:**
- `CardTitleStyle` — заголовок карточки (FontSize 16, Bold)
- `PriceTextStyle` — цена (PriceBrush, FontSize 20, Bold)

**DataGrid:**
- `ModernDataGridStyle` — DataGrid без границ, чередование строк, hover-подсветка
- `ModernDataGridColumnHeaderStyle` — шапка столбца с custom сортировкой
