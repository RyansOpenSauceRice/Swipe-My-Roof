# Developer Guide for Swipe My Roof

This document provides technical information for developers who want to build, modify, or contribute to the Swipe My Roof application.

## Project Structure

The project is organized into the following modules:

- **SwipeMyRoof.Core**: Domain models and shared logic
- **SwipeMyRoof.UI**: User interface controls and view models
- **SwipeMyRoof.LLM**: AI service for roof color inference
- **SwipeMyRoof.OSM**: OpenStreetMap authentication and data operations
- **SwipeMyRoof.Images**: Satellite image retrieval and processing
- **SwipeMyRoof.Storage**: Local persistence and session management
- **SwipeMyRoof.Settings**: Application configuration
- **SwipeMyRoof.AvaloniaUI**: Cross-platform UI using Avalonia
- **SwipeMyRoof.AvaloniaUI.Android**: Android-specific implementation
- **SwipeMyRoof.Tests**: Unit and integration tests

## Development Environment Setup

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022, Visual Studio Code, or JetBrains Rider
- For Android development:
  - Android SDK
  - Java JDK 11 or newer

### Setting Up the Development Environment

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/Swipe-My-Roof.git
   cd Swipe-My-Roof
   ```

2. Install the .NET Android workload:
   ```bash
   dotnet workload install android
   ```

3. Restore dependencies:
   ```bash
   dotnet restore
   ```

## Building the Project

### Building for Desktop

```bash
dotnet build
```

### Building for Linux (Fast Testing)

```bash
dotnet build src/SwipeMyRoof.AvaloniaUI.Linux/SwipeMyRoof.AvaloniaUI.Linux.csproj
```

This Linux-specific build is optimized for faster testing and development cycles.

### Building for Android

```bash
dotnet build src/SwipeMyRoof.AvaloniaUI.Android/SwipeMyRoof.AvaloniaUI.Android.csproj -c Release
```

### Creating an APK

```bash
dotnet publish src/SwipeMyRoof.AvaloniaUI.Android/SwipeMyRoof.AvaloniaUI.Android.csproj -c Release -f net8.0-android
```

The APK will be located in `src/SwipeMyRoof.AvaloniaUI.Android/bin/Release/net8.0-android/publish/`.

## Running the Application

### Running the Desktop Application

```bash
dotnet run --project src/SwipeMyRoof.AvaloniaUI/SwipeMyRoof.AvaloniaUI.csproj
```

### Running the Linux Application (Fast Testing)

```bash
dotnet run --project src/SwipeMyRoof.AvaloniaUI.Linux/SwipeMyRoof.AvaloniaUI.Linux.csproj
```

The Linux build includes additional debugging features and is optimized for faster development cycles.

### Running on an Android Emulator

```bash
dotnet build src/SwipeMyRoof.AvaloniaUI.Android/SwipeMyRoof.AvaloniaUI.Android.csproj -t:Run
```

### Deploying to a Connected Android Device

```bash
dotnet build src/SwipeMyRoof.AvaloniaUI.Android/SwipeMyRoof.AvaloniaUI.Android.csproj -t:Install
```

## Testing

### Running Unit Tests

```bash
dotnet test src/SwipeMyRoof.Tests/SwipeMyRoof.Tests.csproj
```

### Fast Testing with Linux

For rapid development cycles, use the provided testing script:

```bash
./scripts/test_linux.sh
```

This script:
1. Builds the Linux version of the app
2. Runs all unit tests
3. Offers to launch the app if tests pass

### Running UI Tests

UI tests require a display server. On headless environments, you can use Xvfb:

```bash
xvfb-run dotnet test src/SwipeMyRoof.Tests/SwipeMyRoof.Tests.csproj --filter Category=UI
```

## F-Droid Publishing

### Creating a Signing Key

```bash
keytool -genkey -v -keystore swipemyroof.keystore -alias swipemyroof -keyalg RSA -keysize 2048 -validity 10000
```

### Signing the APK

```bash
jarsigner -verbose -sigalg SHA1withRSA -digestalg SHA1 -keystore swipemyroof.keystore SwipeMyRoof.AvaloniaUI.Android.apk swipemyroof
```

### F-Droid Metadata

Create a metadata file in the format required by F-Droid. See [F-Droid Documentation](https://f-droid.org/en/docs/Build_Metadata_Reference/) for details.

## Architecture

### MVVM Pattern

The application follows the Model-View-ViewModel (MVVM) pattern:

- **Models**: Domain entities in the Core project
- **ViewModels**: In the AvaloniaUI project, handling UI logic
- **Views**: XAML files in the AvaloniaUI project

### Dependency Injection

The application uses dependency injection to manage service dependencies:

```csharp
// Example from App.axaml.cs
services.AddSingleton<ILlmService, OpenAiService>();
services.AddSingleton<IImageService, BingMapsImageService>();
services.AddSingleton<IOsmService, OsmService>();
```

### Service Interfaces

All services implement interfaces to allow for easy testing and swapping implementations:

- `ILlmService`: AI model inference
- `IImageService`: Satellite imagery retrieval
- `IOsmService`: OpenStreetMap data operations
- `IStorageService`: Local data persistence
- `ISettingsService`: Application configuration

## Contributing

### Branching Strategy

- `main`: Stable releases
- `develop`: Integration branch for features
- Feature branches: Named `feature/feature-name`
- Bug fixes: Named `fix/bug-description`

### Pull Request Process

1. Create a feature or fix branch from `develop`
2. Implement your changes with appropriate tests
3. Ensure all tests pass
4. Submit a pull request to the `develop` branch
5. Address any review comments

### Coding Standards

- Follow C# coding conventions
- Use XML documentation comments for public APIs
- Write unit tests for all new functionality
- Keep methods small and focused
- Use meaningful variable and method names

## Troubleshooting

### Common Issues

#### Android Build Failures

- Ensure Android SDK is properly installed and configured
- Check that the Android workload is installed: `dotnet workload list`
- Verify Java JDK version (11 or newer)

#### Missing Dependencies

- Run `dotnet restore` to restore all NuGet packages
- Check for any package version conflicts

#### UI Rendering Issues

- Avalonia UI requires a display server
- On headless environments, use Xvfb

## Additional Resources

- [Avalonia UI Documentation](https://docs.avaloniaui.net/)
- [OpenStreetMap API Documentation](https://wiki.openstreetmap.org/wiki/API_v0.6)
- [F-Droid Inclusion Guidelines](https://f-droid.org/en/docs/Inclusion_Policy/)