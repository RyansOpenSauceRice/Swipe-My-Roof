# F-Droid Manifest Guidelines

## Overview
This document outlines the requirements and best practices for making the Roof Color Validation App compatible with F-Droid, ensuring it can be distributed through privacy-respecting app stores.

---

## F-Droid Requirements

### Metadata
- **App Name:** Swipe My Roof
- **Summary:** Help map roof colors in OpenStreetMap with a swipe interface
- **Description:** A privacy-respecting app that helps users contribute roof color data to OpenStreetMap using a simple swipe interface. Uses AI to suggest colors based on satellite imagery, but all decisions are user-confirmed.
- **Categories:** Navigation, Science & Education
- **License:** AGPL-3.0
- **Web Site:** [Project website URL]
- **Source Code:** [GitHub repository URL]
- **Issue Tracker:** [GitHub issues URL]
- **Changelog:** [URL to changelog]

### Technical Requirements
- No proprietary dependencies or blobs
- No Google Play Services dependencies
- Reproducible builds
- No tracking or analytics libraries
- All network connections must be documented and justified

---

## Build Configuration

### Build System
- Use .NET MAUI with MSBuild
- Ensure all build tools are open source
- Document any native dependencies

### Example build.yml
```yaml
Categories:
  - Navigation
  - Science & Education
License: GPL-3.0-or-later
WebSite: https://github.com/RyansOpenSauceRice/Swipe-My-Roof
SourceCode: https://github.com/RyansOpenSauceRice/Swipe-My-Roof
IssueTracker: https://github.com/RyansOpenSauceRice/Swipe-My-Roof/issues
Changelog: https://github.com/RyansOpenSauceRice/Swipe-My-Roof/blob/main/CHANGELOG.md

AutoName: Swipe My Roof

RepoType: git
Repo: https://github.com/RyansOpenSauceRice/Swipe-My-Roof.git

Builds:
  - versionName: '1.0'
    versionCode: 1
    commit: v1.0
    subdir: src/App
    sudo:
      - apt-get update
      - apt-get install -y dotnet-sdk-8.0
    output: bin/Release/net8.0-android/publish/com.ryansopensaucerice.swipemyroof-Signed.apk
    build:
      - dotnet publish -c Release -f net8.0-android /p:AndroidKeyStore=true /p:AndroidSigningKeyStore=keystore.keystore /p:AndroidSigningKeyAlias=key /p:AndroidSigningKeyPass=env:KEY_PASSWORD /p:AndroidSigningStorePass=env:STORE_PASSWORD
    ndk: r25c
```

---

## Privacy Considerations

### Permissions
Document all required Android permissions and their purpose:
- `INTERNET`: Required for OSM API access and LLM API calls
- `ACCESS_NETWORK_STATE`: Check network availability before API calls
- `ACCESS_FINE_LOCATION` (optional): Only if user wants to find buildings near current location

### User Data
- All user credentials (OSM, LLM API keys) must be stored securely
- No data should be sent to third parties without explicit user consent
- Document all data flows in the app description

---

## Reproducible Builds

### Requirements
- Fixed build environment (specific .NET SDK version)
- Deterministic compiler options
- No embedded timestamps or build paths
- Documented build process

### Example MSBuild Properties
```xml
<PropertyGroup>
  <Deterministic>true</Deterministic>
  <PathMap>$(MSBuildProjectDirectory)=.</PathMap>
  <ContinuousIntegrationBuild>true</ContinuousIntegrationBuild>
</PropertyGroup>
```

---

## Anti-Features to Declare
- **NonFreeNet**: If the app requires non-free network services (e.g., proprietary LLM APIs)
- **NonFreeAssets**: If the app includes non-free assets (e.g., proprietary map tiles)

---

## Submission Process
1. Ensure all code is open source and properly licensed
2. Create a reproducible build process
3. Document all network connections and permissions
4. Submit to F-Droid via [their submission process](https://f-droid.org/en/docs/Submitting_to_F-Droid_Quick_Start_Guide/)
5. Be prepared to address any feedback from F-Droid maintainers