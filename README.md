# Swipe-My-Roof

A swipe-based mobile application designed for OpenStreetMap volunteers to easily validate and add roof colors to buildings. Built with C# and designed for privacy-respecting environments like F-Droid and GrapheneOS.

## For OSM Volunteers

Swipe-My-Roof makes contributing to OpenStreetMap easier than ever:

- **Simple Interface**: Swipe right to accept a suggested roof color, left to reject, or tap to modify
- **Confidence Indicators**: Clear visual cues show how confident the AI is about each suggestion
- **Practice Mode**: Learn the ropes without affecting real data
- **Full Control**: Review all changes before they're uploaded to OSM
- **Privacy First**: Your data stays on your device, and you control when uploads happen

No technical knowledge required - just your local knowledge and a few minutes of your time to help improve the map!

## Project Overview

This app uses a "Tinder-like" swipe interface to make the process of adding roof colors to OpenStreetMap buildings fast, engaging, and accurate. It leverages AI to suggest colors based on satellite imagery, but all decisions are user-confirmed.

## Key Features

- Swipe or button-based interface for quick yes/no decisions
- AI-powered roof color suggestions with confidence indicators
- Practice mode with training and feedback
- Local staging of edits with manual upload control
- Privacy-respecting design (no Google dependencies)
- Cost transparency for AI API usage

## Documentation

For OSM volunteers:
- [user_guide.md](docs/user_guide.md) - How to use the application

For developers:
- [spec.md](docs/spec.md) - Complete application specification
- [prompts.md](docs/prompts.md) - AI prompt design and response formats
- [fdroid_manifest.md](docs/fdroid_manifest.md) - F-Droid compatibility guidelines

## Project Structure

The project is organized into the following modules:

- **Core**: Domain models and shared logic
- **UI**: User interface controls and view models
- **LLM**: AI service for roof color inference
- **OSM**: OpenStreetMap authentication and data operations
- **Images**: Satellite image retrieval and processing
- **Storage**: Local persistence and session management
- **Settings**: Application configuration
- **ConsoleApp**: Simple console demo of the core functionality

## Development Status

The project is currently in Stage 1 (MVP) development. The core architecture and domain models have been implemented, and a simple console demo is available to demonstrate the basic functionality.

### Current Implementation

- ✅ Core domain models
- ✅ UI controls including confidence indicators
- ✅ AI service interface and OpenAI implementation
- ✅ OSM service interface and basic implementation
- ✅ Image service interface and Bing Maps implementation
- ✅ Storage service for local persistence
- ✅ Settings service for application configuration
- ✅ Console demo application with confidence indicator demo

### Next Steps

- [x] Implement Avalonia UI for cross-platform support with Android focus
- [ ] Add decoy detection and user reliability tracking
- [x] Improve the interface with swipe gestures
- [ ] Support multiple AI endpoints and local models
- [ ] Add asynchronous upload queue with conflict resolution

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- OpenAI API key (or other compatible AI API)
- Bing Maps API key (for satellite imagery)
- OpenStreetMap account
- For Android development:
  - Android SDK
  - Java JDK 11 or newer

### Building the Project

```bash
# Build the entire solution
dotnet build

# Build the Android app specifically
dotnet build src/SwipeMyRoof.AvaloniaUI.Android/SwipeMyRoof.AvaloniaUI.Android.csproj -c Release
```

### Running the Console Demo

```bash
dotnet run --project src/SwipeMyRoof.ConsoleApp/SwipeMyRoof.ConsoleApp.csproj
```

### Building and Deploying for Android

1. Install the Android workload:
   ```
   dotnet workload install android
   ```

2. Build the Android app:
   ```
   dotnet build src/SwipeMyRoof.AvaloniaUI.Android/SwipeMyRoof.AvaloniaUI.Android.csproj -c Release
   ```

3. Create an APK:
   ```
   dotnet publish src/SwipeMyRoof.AvaloniaUI.Android/SwipeMyRoof.AvaloniaUI.Android.csproj -c Release -f net8.0-android
   ```

4. Deploy to a connected Android device:
   ```
   dotnet build src/SwipeMyRoof.AvaloniaUI.Android/SwipeMyRoof.AvaloniaUI.Android.csproj -t:Install
   ```

### F-Droid Publishing

To publish the app on F-Droid:

1. Create a signing key (if you don't already have one):
   ```
   keytool -genkey -v -keystore swipemyroof.keystore -alias swipemyroof -keyalg RSA -keysize 2048 -validity 10000
   ```

2. Sign the APK:
   ```
   jarsigner -verbose -sigalg SHA1withRSA -digestalg SHA1 -keystore swipemyroof.keystore SwipeMyRoof.AvaloniaUI.Android.apk swipemyroof
   ```

3. Create a metadata file for F-Droid in the format required by their repository.

## License

This project is open source under the [MIT License](LICENSE).
