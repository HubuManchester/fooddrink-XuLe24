# FoodieApp

A cross-platform mobile recipe companion built with **.NET MAUI** for the **6G6Z0014 – Mobile Computing** module. FoodieApp runs on **Android** and **Windows**, demonstrating contemporary mobile development, on-board hardware APIs, and WCAG 2.1 accessibility practices.

---

## Features

### Home – Recipe Search & Shake
- Search recipes via [TheMealDB](https://www.themealdb.com/) API
- Pull-to-refresh recipe list
- **Shake detection** using `Accelerometer.Default` – shake the device to open a random recipe
- **Vibration feedback** via `Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(500))`
- Accelerometer stops automatically when leaving the page

### Recipe Detail – Text-to-Speech
- View recipe image, category, area, ingredients, and step-by-step instructions
- **Read Aloud** button calls `TextToSpeech.Default.SpeakAsync()` to read the **recipe name and ingredients list**
- **Stop** button cancels playback via `CancellationTokenSource` (MAUI TTS does not expose `StopAsync`; cancellation is the supported stop mechanism)
- `CancellationTokenSource` in `RecipeDetailViewModel` controls TTS lifecycle
- Save/remove favourites with haptic feedback

### Scan – Camera
- Capture a food photo with the device camera (`MediaPicker`)
- Pick an existing image from the gallery
- Camera and storage permissions declared in `AndroidManifest.xml`

### Nearby – GPS Location
- Detect current GPS coordinates using `Geolocation.Default`
- Displays latitude and longitude
- Location permissions declared for Android

### Settings – Theme & Accessibility
- **Dark / Light theme** toggle persisted via `SettingsService` and applied on app launch
- **Font size presets**: Normal (16 pt), Large (20 pt), Extra Large (24 pt)
- Settings stored in `Preferences` and applied immediately across the app
- Reset to defaults option

### Accessibility (WCAG 2.1)
- All `Image` controls include `SemanticProperties.Description`
- All `Button` controls include `SemanticProperties.Hint`
- High-contrast label colours (≥ 4.5:1 ratio) in light and dark themes
- Screen reader support for TalkBack (Android) and Narrator (Windows)
- Semantic heading levels on key labels

---

## Architecture

```
FoodieApp/
├── Models/           # Recipe, API response DTOs
├── Services/         # RecipeService, FavouritesService, SettingsService
├── ViewModels/       # MVVM view models (CommunityToolkit.Mvvm)
├── Views/            # XAML pages (MainPage, RecipeDetailPage, etc.)
├── Platforms/        # Android & Windows platform-specific code
├── Resources/        # Styles, colours, fonts, icons, splash screen
├── App.xaml          # Application resources & theme
├── AppShell.xaml     # Tab bar navigation
└── MauiProgram.cs    # DI container & app bootstrap
```

**Pattern:** MVVM with dependency injection (`Microsoft.Extensions.DependencyInjection`).

| Component | Lifetime |
|-----------|----------|
| `RecipeService`, `SettingsService`, `FavouritesService` | Singleton |
| ViewModels & Pages | Transient |
| `AppShell` | Singleton |

---

## Hardware & APIs Used

| Feature | API | Platform |
|---------|-----|----------|
| Recipe data | `HttpClient` + TheMealDB REST API | All |
| Shake detection | `Accelerometer.Default` | Android, physical devices |
| Vibration | `Vibration.Default` | Android |
| Text-to-Speech | `TextToSpeech.Default` | Android, Windows |
| Camera / Gallery | `MediaPicker.Default` | Android |
| GPS Location | `Geolocation.Default` | Android, Windows |
| Haptic feedback | `HapticFeedback` | Android |
| Settings persistence | `Preferences` | All |

---

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) with:
  - **.NET Multi-platform App UI development** workload
  - **Mobile development with .NET** (for Android emulator)
- Android SDK (API 21+, target API 35)
- Windows 10 SDK (10.0.19041.0+) for Windows target

---

## Getting Started

### 1. Clone / open the project

```bash
cd FoodieApp
```

Open `FoodieApp.csproj` in Visual Studio 2022.

### 2. Restore dependencies

```bash
dotnet restore
```

### 3. Run on Windows

In Visual Studio, select **Windows Machine** as the startup target and press **F5**.

Or from the terminal:

```bash
dotnet build -f net9.0-windows10.0.19041.0
dotnet run --project FoodieApp.csproj -f net9.0-windows10.0.19041.0
```

### 4. Run on Android

1. Start an Android emulator (API 30+ recommended) in Visual Studio
2. Select the emulator as the startup target
3. Press **F5** to deploy

Or:

```bash
dotnet build -f net9.0-android
```

> **Note:** Shake and vibration require a physical device or an emulator with sensor simulation enabled. Set a simulated GPS location in the emulator for the Nearby tab.

---

## Permissions (Android)

Declared in `Platforms/Android/AndroidManifest.xml`:

| Permission | Purpose |
|------------|---------|
| `INTERNET` | Fetch recipes from TheMealDB |
| `CAMERA` | Take food photos |
| `ACCESS_FINE_LOCATION` / `ACCESS_COARSE_LOCATION` | GPS location |
| `READ_MEDIA_IMAGES` | Pick photos from gallery |
| `VIBRATE` | Shake feedback |

---

## Navigation

| Tab | Page | Route |
|-----|------|-------|
| Home | `MainPage` | `MainPage` |
| Scan | `ScanPage` | `ScanPage` |
| Nearby | `LocationPage` | `LocationPage` |
| Settings | `SettingsPage` | `SettingsPage` |

Modal routes: `RecipeDetailPage?recipeId={id}`, `HelpPage`

---

## Settings Persistence

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `dark_mode` | bool | `false` | Dark theme enabled |
| `font_size_preset` | string | `Normal` | `Normal`, `Large`, or `ExtraLarge` |

Settings are read and applied in `App.xaml.cs` on startup via `SettingsService.ApplyAll()`.

---

## External API

- **TheMealDB** – free recipe API  
  - Search: `https://www.themealdb.com/api/json/v1/1/search.php?s={keyword}`  
  - Lookup: `https://www.themealdb.com/api/json/v1/1/lookup.php?i={id}`

No API key required. Requires an internet connection.

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| App crashes on Android startup | Clean and rebuild; ensure only one `MainActivity` in the merged manifest |
| Windows won't start from VS | Set launch profile to **Windows Machine** (`commandName: Project`) |
| Shake not working | Use a physical device or enable accelerometer in emulator extended controls |
| TTS silent on Windows | Check Windows speech settings and audio output |
| Location shows unavailable | Grant location permission; set simulated location in emulator |
| No recipes loading | Check internet connection and TheMealDB availability |

---

## Module Information

- **Module:** 6G6Z0014 – Mobile Computing
- **Assessment:** 1CWK100 – Developing a Cross-Platform Mobile App
- **Learning Outcomes:**
  - **LO1** – Create applications capable of running on contemporary mobile devices
  - **LO2** – Utilise specialist on-board mobile hardware via appropriate APIs

---

## License

This project was created for academic assessment purposes.
