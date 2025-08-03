# Android Testing Plan for Swipe My Roof

This document outlines the testing strategy for the Swipe My Roof Android application, focusing on ensuring a high-quality user experience on Android devices.

## Testing Priorities

Given that Android/F-Droid is our primary target platform for the initial release, testing will focus on:

1. **UI/UX on Android devices** - Ensuring the application looks and works well on various Android screen sizes and densities
2. **Performance on mobile hardware** - Optimizing for lower-end devices to ensure broad accessibility
3. **Battery and data usage** - Minimizing resource consumption
4. **F-Droid compatibility** - Ensuring the app meets F-Droid's requirements

## Testing Environments

### Devices

Testing should be performed on:

- Low-end Android devices (2GB RAM, older processors)
- Mid-range Android devices (4GB RAM)
- High-end Android devices (8GB+ RAM)
- Various screen sizes (from 5" phones to 10" tablets)
- Different Android versions (Android 8.0 through latest)

### Emulators

For automated testing and development, use the following emulator configurations:

- Small phone (5.0", 1080x1920, Android 10)
- Large phone (6.7", 1440x3120, Android 13)
- Tablet (10.1", 1200x1920, Android 12)

## Testing Types

### 1. Unit Testing

- Test individual components in isolation
- Focus on core business logic in the ViewModels
- Test the model selection service thoroughly

```bash
dotnet test src/SwipeMyRoof.Tests/UnitTests
```

### 2. Integration Testing

- Test interactions between components
- Focus on data flow between services
- Test API integrations with mock servers

```bash
dotnet test src/SwipeMyRoof.Tests/IntegrationTests
```

### 3. UI Testing

- Test UI components and interactions
- Verify responsive layout on different screen sizes
- Test touch gestures and animations

```bash
dotnet test src/SwipeMyRoof.Tests/UITests
```

### 4. Performance Testing

- Measure app startup time
- Test image loading and processing performance
- Monitor memory usage during extended sessions
- Test battery consumption

### 5. Compatibility Testing

- Test on different Android versions
- Test on various device manufacturers (Samsung, Google, Xiaomi, etc.)
- Verify F-Droid compatibility

## Test Cases

### Core Functionality

1. **Building Validation**
   - Verify building images load correctly
   - Test accept/reject/skip functionality
   - Verify confidence indicator displays correctly
   - Test explanation text rendering

2. **Model Selection**
   - Verify model selection logic works correctly
   - Test fallback mechanisms when preferred model is unavailable
   - Verify confidence calculation

3. **Image Loading**
   - Test image loading from various sources
   - Verify error handling for missing or corrupt images
   - Test image caching

### UI/UX

1. **Responsive Layout**
   - Verify UI adapts correctly to different screen sizes
   - Test orientation changes (portrait/landscape)
   - Verify touch targets are appropriately sized for mobile

2. **Navigation**
   - Test tab navigation
   - Verify back button behavior
   - Test deep linking (if implemented)

3. **Accessibility**
   - Test with screen readers
   - Verify color contrast meets accessibility standards
   - Test keyboard navigation

### Performance

1. **Startup Performance**
   - Measure cold start time
   - Measure warm start time

2. **Runtime Performance**
   - Monitor frame rate during animations
   - Test scrolling performance with many items
   - Measure memory usage during extended sessions

3. **Battery and Data Usage**
   - Monitor battery consumption during typical usage
   - Measure data usage for image loading and API calls

## Automated Testing

### UI Automation

Use Avalonia.TestFramework for UI automation:

```csharp
[Fact]
public async Task ClickAcceptButton_ShouldMoveToNextBuilding()
{
    // Arrange
    var vm = new BuildingValidationViewModel();
    await vm.InitializeAsync();
    var initialBuildingId = vm.CurrentBuilding.OsmId;
    
    // Act
    await vm.AcceptBuildingCommand.ExecuteAsync(null);
    
    // Assert
    Assert.NotEqual(initialBuildingId, vm.CurrentBuilding.OsmId);
}
```

### Performance Testing

Use benchmarking tools to measure performance:

```csharp
[Benchmark]
public async Task LoadBuildingImage()
{
    var vm = new BuildingValidationViewModel();
    await vm.LoadBuildingImageAsync();
}
```

## Manual Testing Checklist

Before each release, perform the following manual tests:

- [ ] Install the app on a clean device
- [ ] Verify all UI elements are visible and properly aligned
- [ ] Test all buttons and interactive elements
- [ ] Verify swipe gestures work correctly
- [ ] Test with different network conditions (fast, slow, offline)
- [ ] Verify error messages are clear and helpful
- [ ] Test with different system font sizes
- [ ] Verify dark/light mode compatibility (if supported)
- [ ] Test battery usage during a 30-minute session

## F-Droid Specific Testing

- [ ] Verify the app works without Google Play Services
- [ ] Ensure all dependencies are open source and compatible with F-Droid
- [ ] Test the app with F-Droid's privileged extension
- [ ] Verify update mechanism works through F-Droid

## Continuous Integration

Set up CI/CD pipeline to:

1. Run unit and integration tests on each commit
2. Build the app for different Android ABIs
3. Run UI tests on emulators
4. Generate test reports
5. Create signed APKs for testing

## Bug Reporting

For bug reports, include:

1. Device model and Android version
2. Steps to reproduce
3. Expected vs. actual behavior
4. Screenshots or screen recordings
5. Logs (if available)

## Release Testing

Before releasing to F-Droid:

1. Perform a full regression test on all supported devices
2. Verify all known bugs are fixed or documented
3. Test upgrade path from previous versions
4. Verify all documentation is up to date
5. Perform a final security review