using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using SwipeMyRoof.Core.Models;
using SwipeMyRoof.Images.Models;
using SwipeMyRoof.Images.Services;
using SwipeMyRoof.LLM.Models;
using SwipeMyRoof.LLM.Services;
using SwipeMyRoof.OSM.Models;
using SwipeMyRoof.OSM.Services;
using SwipeMyRoof.Settings.Models;
using SwipeMyRoof.Settings.Services;
using SwipeMyRoof.Storage.Models;
using SwipeMyRoof.Storage.Services;

// Setup dependency injection
var serviceProvider = new ServiceCollection()
    .AddSingleton<ISettingsService, LocalSettingsService>()
    .AddSingleton<IStorageService, LocalStorageService>()
    .AddSingleton<HttpClient>()
    .AddSingleton<ILlmService>(provider => 
    {
        var httpClient = provider.GetRequiredService<HttpClient>();
        return new OpenAiLlmService(httpClient, "YOUR_API_KEY_HERE");
    })
    .AddSingleton<IOsmService>(provider => 
    {
        var httpClient = provider.GetRequiredService<HttpClient>();
        return new OsmService(httpClient);
    })
    .AddSingleton<IImageService>(provider => 
    {
        var httpClient = provider.GetRequiredService<HttpClient>();
        return new BingImageService(httpClient, "YOUR_BING_MAPS_API_KEY_HERE");
    })
    .BuildServiceProvider();

// Get services
var settingsService = serviceProvider.GetRequiredService<ISettingsService>();
var storageService = serviceProvider.GetRequiredService<IStorageService>();
var llmService = serviceProvider.GetRequiredService<ILlmService>();
var osmService = serviceProvider.GetRequiredService<IOsmService>();
var imageService = serviceProvider.GetRequiredService<IImageService>();

// Display welcome message
Console.WriteLine("=== Swipe My Roof - Console Demo ===");
Console.WriteLine("This is a simple demonstration of the core functionality.");
Console.WriteLine();

// Main menu
bool exit = false;
while (!exit)
{
    Console.WriteLine("Main Menu:");
    Console.WriteLine("1. Start Practice Session");
    Console.WriteLine("2. Start Real Session");
    Console.WriteLine("3. View Upload Queue");
    Console.WriteLine("4. Settings");
    Console.WriteLine("5. Exit");
    Console.Write("Select an option: ");
    
    var option = Console.ReadLine();
    Console.WriteLine();
    
    switch (option)
    {
        case "1":
            await StartSessionAsync(true);
            break;
        case "2":
            await StartSessionAsync(false);
            break;
        case "3":
            await ViewUploadQueueAsync();
            break;
        case "4":
            await ManageSettingsAsync();
            break;
        case "5":
            exit = true;
            break;
        default:
            Console.WriteLine("Invalid option. Please try again.");
            break;
    }
    
    Console.WriteLine();
}

// Start a validation session
async Task StartSessionAsync(bool isPracticeMode)
{
    Console.WriteLine($"Starting {(isPracticeMode ? "Practice" : "Real")} Session");
    
    // Create a session
    var session = await storageService.CreateSessionAsync("user1", isPracticeMode, new SwipeMyRoof.Storage.Models.AreaSelection
    {
        Type = "Radius",
        CenterLat = 40.7128,
        CenterLon = -74.0060,
        Radius = 1000
    });
    
    Console.WriteLine($"Session created with ID: {session.Id}");
    
    // Simulate getting a building
    var building = new BuildingCandidate
    {
        OsmId = 123456789,
        Location = new GeoLocation { Lat = 40.7128, Lon = -74.0060 },
        BoundingBox = new BoundingBox { MinX = -74.0065, MinY = 40.7123, MaxX = -74.0055, MaxY = 40.7133 },
        ExistingRoofColor = null,
        SessionId = session.Id
    };
    
    // Add the building to the session
    await storageService.AddBuildingCandidateAsync(session.Id, building);
    
    // Simulate getting a roof color suggestion
    Console.WriteLine("Getting roof color suggestion...");
    
    // In a real implementation, we would get an image and use the LLM
    // For now, we'll simulate it
    building.ProposedColor = new ProposedColor
    {
        Value = "dark gray",
        Source = "ai",
        Confidence = 0.78,
        Explanation = "uniform tone",
        IsDecoy = false,
        Timestamp = DateTime.UtcNow
    };
    
    // Update the building in the session
    await storageService.UpdateBuildingCandidateAsync(session.Id, building);
    
    // Display the suggestion
    Console.WriteLine($"Proposed roof color: {building.ProposedColor.Value}");
    Console.WriteLine($"Confidence: {building.ProposedColor.Confidence:P0}");
    Console.WriteLine($"Explanation: {building.ProposedColor.Explanation}");
    
    // Get user feedback
    Console.WriteLine();
    Console.WriteLine("What would you like to do?");
    Console.WriteLine("1. Accept");
    Console.WriteLine("2. Reject");
    Console.WriteLine("3. Skip");
    Console.Write("Select an option: ");
    
    var option = Console.ReadLine();
    Console.WriteLine();
    
    switch (option)
    {
        case "1":
            building.UserFeedback = UserFeedback.Accepted;
            Console.WriteLine("Accepted!");
            
            // Add to upload queue if not practice mode and not a decoy
            if (!isPracticeMode && !building.ProposedColor.IsDecoy)
            {
                await storageService.AddToUploadQueueAsync(building);
                Console.WriteLine("Added to upload queue.");
            }
            break;
        case "2":
            building.UserFeedback = UserFeedback.Rejected;
            Console.WriteLine("Rejected!");
            break;
        case "3":
            building.UserFeedback = UserFeedback.Skipped;
            Console.WriteLine("Skipped!");
            break;
        default:
            Console.WriteLine("Invalid option. Skipping by default.");
            building.UserFeedback = UserFeedback.Skipped;
            break;
    }
    
    // Update the building in the session
    await storageService.UpdateBuildingCandidateAsync(session.Id, building);
    
    // End the session
    session.EndTime = DateTime.UtcNow;
    await storageService.UpdateSessionStatisticsAsync(session);
    
    Console.WriteLine("Session ended.");
}

// View the upload queue
async Task ViewUploadQueueAsync()
{
    Console.WriteLine("Upload Queue:");
    
    var queue = await storageService.GetUploadQueueAsync();
    
    if (queue.Count == 0)
    {
        Console.WriteLine("Queue is empty.");
        return;
    }
    
    for (int i = 0; i < queue.Count; i++)
    {
        var item = queue[i];
        Console.WriteLine($"{i + 1}. Building {item.BuildingCandidate.OsmId} - {item.BuildingCandidate.ProposedColor?.Value} - {item.Status}");
    }
    
    Console.WriteLine();
    Console.WriteLine("What would you like to do?");
    Console.WriteLine("1. Upload All");
    Console.WriteLine("2. Remove Item");
    Console.WriteLine("3. Back to Main Menu");
    Console.Write("Select an option: ");
    
    var option = Console.ReadLine();
    Console.WriteLine();
    
    switch (option)
    {
        case "1":
            Console.WriteLine("Uploading all items...");
            // In a real implementation, we would upload the items
            Console.WriteLine("Upload complete!");
            break;
        case "2":
            Console.Write("Enter item number to remove: ");
            if (int.TryParse(Console.ReadLine(), out int itemNumber) && itemNumber > 0 && itemNumber <= queue.Count)
            {
                await storageService.RemoveFromUploadQueueAsync(queue[itemNumber - 1].Id);
                Console.WriteLine("Item removed.");
            }
            else
            {
                Console.WriteLine("Invalid item number.");
            }
            break;
        case "3":
            // Do nothing, return to main menu
            break;
        default:
            Console.WriteLine("Invalid option.");
            break;
    }
}

// Manage settings
async Task ManageSettingsAsync()
{
    Console.WriteLine("Settings:");
    
    var settings = await settingsService.GetSettingsAsync();
    
    Console.WriteLine("1. LLM Settings");
    Console.WriteLine("2. OSM Settings");
    Console.WriteLine("3. Image Settings");
    Console.WriteLine("4. UI Settings");
    Console.WriteLine("5. Decoy Settings");
    Console.WriteLine("6. Reset to Defaults");
    Console.WriteLine("7. Back to Main Menu");
    Console.Write("Select an option: ");
    
    var option = Console.ReadLine();
    Console.WriteLine();
    
    switch (option)
    {
        case "1":
            await ManageLlmSettingsAsync(settings.Llm);
            break;
        case "2":
            await ManageOsmSettingsAsync(settings.Osm);
            break;
        case "3":
            await ManageImageSettingsAsync(settings.Image);
            break;
        case "4":
            await ManageUiSettingsAsync(settings.Ui);
            break;
        case "5":
            await ManageDecoySettingsAsync(settings.Decoy);
            break;
        case "6":
            await settingsService.ResetSettingsAsync();
            Console.WriteLine("Settings reset to defaults.");
            break;
        case "7":
            // Do nothing, return to main menu
            break;
        default:
            Console.WriteLine("Invalid option.");
            break;
    }
}

// Manage LLM settings
async Task ManageLlmSettingsAsync(LlmSettings settings)
{
    Console.WriteLine("LLM Settings:");
    Console.WriteLine($"1. API Endpoint: {settings.Endpoint}");
    Console.WriteLine($"2. API Key: {(string.IsNullOrEmpty(settings.ApiKey) ? "Not set" : "****")}");
    Console.WriteLine($"3. Model: {settings.Model}");
    Console.WriteLine($"4. Use High Resolution: {settings.UseHighResolution}");
    Console.WriteLine($"5. Max Token Budget: {settings.MaxTokenBudget}");
    Console.WriteLine($"6. Warning Threshold: {settings.WarningThresholdPercent}%");
    Console.WriteLine("7. Back to Settings Menu");
    Console.Write("Select an option to change: ");
    
    var option = Console.ReadLine();
    Console.WriteLine();
    
    switch (option)
    {
        case "1":
            Console.Write("Enter new API endpoint: ");
            settings.Endpoint = Console.ReadLine() ?? settings.Endpoint;
            break;
        case "2":
            Console.Write("Enter new API key: ");
            settings.ApiKey = Console.ReadLine() ?? settings.ApiKey;
            break;
        case "3":
            Console.Write("Enter new model: ");
            settings.Model = Console.ReadLine() ?? settings.Model;
            break;
        case "4":
            Console.Write("Use high resolution (true/false): ");
            if (bool.TryParse(Console.ReadLine(), out bool useHighRes))
            {
                settings.UseHighResolution = useHighRes;
            }
            break;
        case "5":
            Console.Write("Enter max token budget: ");
            if (int.TryParse(Console.ReadLine(), out int maxTokens))
            {
                settings.MaxTokenBudget = maxTokens;
            }
            break;
        case "6":
            Console.Write("Enter warning threshold percentage: ");
            if (int.TryParse(Console.ReadLine(), out int threshold))
            {
                settings.WarningThresholdPercent = threshold;
            }
            break;
        case "7":
            // Do nothing, return to settings menu
            break;
        default:
            Console.WriteLine("Invalid option.");
            break;
    }
    
    await settingsService.SaveLlmSettingsAsync(settings);
    Console.WriteLine("Settings saved.");
}

// Manage OSM settings
async Task ManageOsmSettingsAsync(OsmSettings settings)
{
    Console.WriteLine("OSM Settings:");
    Console.WriteLine($"1. Username: {settings.Username}");
    Console.WriteLine($"2. Password: {(string.IsNullOrEmpty(settings.Password) ? "Not set" : "****")}");
    Console.WriteLine($"3. Default Changeset Comment: {settings.DefaultChangesetComment}");
    Console.WriteLine($"4. Auto Upload: {settings.AutoUpload}");
    Console.WriteLine($"5. Max Batch Size: {settings.MaxBatchSize}");
    Console.WriteLine("6. Back to Settings Menu");
    Console.Write("Select an option to change: ");
    
    var option = Console.ReadLine();
    Console.WriteLine();
    
    switch (option)
    {
        case "1":
            Console.Write("Enter new username: ");
            settings.Username = Console.ReadLine() ?? settings.Username;
            break;
        case "2":
            Console.Write("Enter new password: ");
            settings.Password = Console.ReadLine() ?? settings.Password;
            break;
        case "3":
            Console.Write("Enter new default changeset comment: ");
            settings.DefaultChangesetComment = Console.ReadLine() ?? settings.DefaultChangesetComment;
            break;
        case "4":
            Console.Write("Auto upload (true/false): ");
            if (bool.TryParse(Console.ReadLine(), out bool autoUpload))
            {
                settings.AutoUpload = autoUpload;
            }
            break;
        case "5":
            Console.Write("Enter max batch size: ");
            if (int.TryParse(Console.ReadLine(), out int maxBatchSize))
            {
                settings.MaxBatchSize = maxBatchSize;
            }
            break;
        case "6":
            // Do nothing, return to settings menu
            break;
        default:
            Console.WriteLine("Invalid option.");
            break;
    }
    
    await settingsService.SaveOsmSettingsAsync(settings);
    Console.WriteLine("Settings saved.");
}

// Manage Image settings
async Task ManageImageSettingsAsync(ImageSettings settings)
{
    Console.WriteLine("Image Settings:");
    Console.WriteLine($"1. Bing Maps API Key: {(string.IsNullOrEmpty(settings.BingMapsApiKey) ? "Not set" : "****")}");
    Console.WriteLine($"2. Default Width: {settings.DefaultWidth}");
    Console.WriteLine($"3. Default Height: {settings.DefaultHeight}");
    Console.WriteLine($"4. Building Buffer Ratio: {settings.BuildingBufferRatio}");
    Console.WriteLine($"5. JPEG Quality: {settings.JpegQuality}");
    Console.WriteLine("6. Back to Settings Menu");
    Console.Write("Select an option to change: ");
    
    var option = Console.ReadLine();
    Console.WriteLine();
    
    switch (option)
    {
        case "1":
            Console.Write("Enter new Bing Maps API key: ");
            settings.BingMapsApiKey = Console.ReadLine() ?? settings.BingMapsApiKey;
            break;
        case "2":
            Console.Write("Enter default width: ");
            if (int.TryParse(Console.ReadLine(), out int width))
            {
                settings.DefaultWidth = width;
            }
            break;
        case "3":
            Console.Write("Enter default height: ");
            if (int.TryParse(Console.ReadLine(), out int height))
            {
                settings.DefaultHeight = height;
            }
            break;
        case "4":
            Console.Write("Enter building buffer ratio: ");
            if (double.TryParse(Console.ReadLine(), out double ratio))
            {
                settings.BuildingBufferRatio = ratio;
            }
            break;
        case "5":
            Console.Write("Enter JPEG quality (0-100): ");
            if (int.TryParse(Console.ReadLine(), out int quality))
            {
                settings.JpegQuality = quality;
            }
            break;
        case "6":
            // Do nothing, return to settings menu
            break;
        default:
            Console.WriteLine("Invalid option.");
            break;
    }
    
    await settingsService.SaveImageSettingsAsync(settings);
    Console.WriteLine("Settings saved.");
}

// Manage UI settings
async Task ManageUiSettingsAsync(UiSettings settings)
{
    Console.WriteLine("UI Settings:");
    Console.WriteLine($"1. Theme: {settings.Theme}");
    Console.WriteLine($"2. Use Swipe Gestures: {settings.UseSwipeGestures}");
    Console.WriteLine($"3. Show Confidence Indicators: {settings.ShowConfidenceIndicators}");
    Console.WriteLine($"4. Show Token Usage: {settings.ShowTokenUsage}");
    Console.WriteLine($"5. Show Practice Mode Banner: {settings.ShowPracticeModeBanner}");
    Console.WriteLine("6. Back to Settings Menu");
    Console.Write("Select an option to change: ");
    
    var option = Console.ReadLine();
    Console.WriteLine();
    
    switch (option)
    {
        case "1":
            Console.Write("Enter theme (Light/Dark/System): ");
            settings.Theme = Console.ReadLine() ?? settings.Theme;
            break;
        case "2":
            Console.Write("Use swipe gestures (true/false): ");
            if (bool.TryParse(Console.ReadLine(), out bool useSwipe))
            {
                settings.UseSwipeGestures = useSwipe;
            }
            break;
        case "3":
            Console.Write("Show confidence indicators (true/false): ");
            if (bool.TryParse(Console.ReadLine(), out bool showConfidence))
            {
                settings.ShowConfidenceIndicators = showConfidence;
            }
            break;
        case "4":
            Console.Write("Show token usage (true/false): ");
            if (bool.TryParse(Console.ReadLine(), out bool showTokens))
            {
                settings.ShowTokenUsage = showTokens;
            }
            break;
        case "5":
            Console.Write("Show practice mode banner (true/false): ");
            if (bool.TryParse(Console.ReadLine(), out bool showBanner))
            {
                settings.ShowPracticeModeBanner = showBanner;
            }
            break;
        case "6":
            // Do nothing, return to settings menu
            break;
        default:
            Console.WriteLine("Invalid option.");
            break;
    }
    
    await settingsService.SaveUiSettingsAsync(settings);
    Console.WriteLine("Settings saved.");
}

// Manage Decoy settings
async Task ManageDecoySettingsAsync(DecoySettings settings)
{
    Console.WriteLine("Decoy Settings:");
    Console.WriteLine($"1. Frequency: {settings.Frequency:P0}");
    Console.WriteLine($"2. Min Buildings Before Decoys: {settings.MinBuildingsBeforeDecoys}");
    Console.WriteLine($"3. Adapt Frequency Based On Reliability: {settings.AdaptFrequencyBasedOnReliability}");
    Console.WriteLine($"4. Max Frequency: {settings.MaxFrequency:P0}");
    Console.WriteLine("5. Back to Settings Menu");
    Console.Write("Select an option to change: ");
    
    var option = Console.ReadLine();
    Console.WriteLine();
    
    switch (option)
    {
        case "1":
            Console.Write("Enter frequency (0.0-1.0): ");
            if (double.TryParse(Console.ReadLine(), out double frequency))
            {
                settings.Frequency = Math.Clamp(frequency, 0.0, 1.0);
            }
            break;
        case "2":
            Console.Write("Enter min buildings before decoys: ");
            if (int.TryParse(Console.ReadLine(), out int minBuildings))
            {
                settings.MinBuildingsBeforeDecoys = minBuildings;
            }
            break;
        case "3":
            Console.Write("Adapt frequency based on reliability (true/false): ");
            if (bool.TryParse(Console.ReadLine(), out bool adapt))
            {
                settings.AdaptFrequencyBasedOnReliability = adapt;
            }
            break;
        case "4":
            Console.Write("Enter max frequency (0.0-1.0): ");
            if (double.TryParse(Console.ReadLine(), out double maxFrequency))
            {
                settings.MaxFrequency = Math.Clamp(maxFrequency, 0.0, 1.0);
            }
            break;
        case "5":
            // Do nothing, return to settings menu
            break;
        default:
            Console.WriteLine("Invalid option.");
            break;
    }
    
    await settingsService.SaveDecoySettingsAsync(settings);
    Console.WriteLine("Settings saved.");
}
