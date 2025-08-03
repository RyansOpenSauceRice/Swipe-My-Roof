# Satellite Imagery in Swipe My Roof

This document explains how satellite imagery is handled in the Swipe My Roof application.

## Overview

Swipe My Roof uses a tile-based system for satellite imagery to present users with building images for roof color validation. **Important: This is not a general-purpose OSM editor like JOSM or iD.**

The application workflow is similar to a dating app:
1. Users select a location and radius for validation
2. The system randomly selects buildings within that area
3. Each building is presented individually with satellite imagery
4. Users swipe/select to confirm or reject the proposed roof color

The application supports multiple tile providers, including:
- Bing Maps Aerial
- OpenStreetMap
- Mapbox Satellite
- ESRI World Imagery

## Architecture

The satellite imagery system consists of the following components:

1. **TileProvider**: Represents a source of satellite imagery tiles
2. **ITileProviderService**: Interface for managing tile providers and fetching tiles
3. **TileProviderService**: Implementation of the tile provider service
4. **BuildingImage**: Represents a satellite image of a building
5. **TileProviderViewModel**: View model for managing tile providers
6. **TileProviderView**: UI for managing tile providers

## Tile Providers

Tile providers use URL templates with placeholders that are replaced with actual values when fetching tiles:

- `{x}`, `{y}`, `{z}`: Tile coordinates and zoom level
- `{apikey}`: API key
- `{quadkey}`: Quadkey (for Bing Maps)
- `{width}`, `{height}`: Image dimensions
- `{latitude}`, `{longitude}`: Coordinates (for Bing Maps)

Example URL templates:

- OpenStreetMap: `https://tile.openstreetmap.org/{z}/{x}/{y}.png`
- Bing Maps: `https://dev.virtualearth.net/REST/v1/Imagery/Map/Aerial/{latitude},{longitude}/{zoom}?mapSize={width},{height}&key={apikey}`
- Mapbox: `https://api.mapbox.com/v4/mapbox.satellite/{z}/{x}/{y}.jpg90?access_token={apikey}`

## Tile Caching

Tiles are cached locally to reduce bandwidth usage and improve performance. The cache is stored in the application data directory:

```
%APPDATA%\SwipeMyRoof\TileCache
```

Each tile is cached with a key that includes the provider ID, zoom level, and coordinates. Cached tiles expire after a configurable number of days (default: 30).

## Building Images

When the application needs to display a building, it:

1. Calculates the optimal zoom level based on the building's bounding box
2. Fetches the appropriate tile(s) from the selected provider
3. Crops the tile(s) to show just the building with a buffer (fixed ratio of extra space around the building)
4. Analyzes the image to extract dominant colors (for roof color suggestions)

**Important constraints:**
- Users cannot move, rotate, or zoom the imagery
- Each building is presented in isolation with a fixed view
- The building is shown with a consistent buffer ratio around it
- The user's task is simply to confirm or reject the proposed roof color

## Adding Custom Providers

Users can add custom tile providers through the UI. Required information includes:

- Name
- URL template
- Attribution text and URL
- Minimum and maximum zoom levels
- API key (if required)

## API Keys

Some tile providers require API keys. The application includes placeholder keys for development and testing purposes:

1. During development and testing, placeholder API keys are used
2. API keys will only be replaced with actual keys once core features are thoroughly tested
3. The application prioritizes free and open-source imagery sources that don't require API keys when possible

## Providers and Their Requirements

### Bing Maps

- Register at: https://www.bingmapsportal.com/
- Free tier: 125,000 transactions per year
- Attribution required: "© Microsoft Bing"

### Mapbox

- Register at: https://account.mapbox.com/
- Free tier: 50,000 map loads per month
- Attribution required: "© Mapbox"

### ESRI World Imagery

- No API key required for basic usage
- Attribution required: "© Esri"

### OpenStreetMap

- No API key required
- Not recommended for heavy usage (consider setting up a tile server)
- Attribution required: "© OpenStreetMap contributors"

## Best Practices

1. **Respect usage limits**: Don't overload free tile services
2. **Always include attribution**: This is required by all providers
3. **Cache tiles efficiently**: Set appropriate cache expiration times
4. **Use appropriate zoom levels**: Don't request unnecessarily high zoom levels
5. **Consider self-hosting**: For production use, consider setting up your own tile server

## Troubleshooting

Common issues:

1. **Tiles not loading**: Check internet connection and API key validity
2. **Wrong location shown**: Verify coordinate system and transformation
3. **Poor image quality**: Try a different provider or increase zoom level
4. **High bandwidth usage**: Adjust cache settings and zoom levels

## Future Improvements

Potential enhancements:

1. Improved color analysis for roof detection - to be implemented after core features are tested
2. Support for vector tiles and OSM data overlay - only if using open-source and free sources with no API key requirements
3. Optimization of tile caching for better performance

**Non-goals:**
1. Historical imagery support - the application focuses only on the latest available imagery
2. Offline mode with pre-downloaded tiles - the application is not designed to archive or store valuable data
3. Full OSM editor functionality - this is a focused validation tool, not a general-purpose editor

## Core Application Purpose

It's important to emphasize that Swipe My Roof is designed as a focused, gamified validation tool for OSM roof colors. The application:

1. Presents buildings one at a time in a "swipe" interface
2. Shows a fixed view of each building with consistent framing
3. Asks users simple validation questions about roof colors
4. Prioritizes ease of use and engagement over complex editing capabilities

This approach allows casual contributors to make meaningful contributions to OSM data quality without requiring extensive training or technical knowledge.