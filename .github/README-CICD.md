# CI/CD Pipeline Documentation

## 🚀 Simplified AppImage-Only Builds

This repository uses a single, streamlined CI/CD workflow for creating downloadable Linux desktop applications:

### **AppImage Builds** (`build-appimage.yml`)
**Triggers:** All branches (main, feature/*), Pull Requests, Manual trigger
**Purpose:** Universal one-click portable applications

**For All Builds:**
- ✅ One-click download and run
- ✅ No installation required  
- ✅ Universal Linux compatibility
- ✅ Self-contained with all dependencies

**Build Types:**
- **Main Branch**: Development builds (pre-release)
- **Feature Branches**: Feature test builds (pre-release)
- **Pull Requests**: Test artifacts only

**Download:** 
- **Releases Tab**: For pushed builds (main/feature branches)
- **Actions Tab**: For PR builds (artifacts)

## 📦 What Gets Built

### Self-Contained Features:
- ✅ **No .NET Runtime Required**: Everything bundled
- ✅ **Bing Maps Integration**: Satellite imagery support
- ✅ **OSM Submission Pipeline**: Direct OpenStreetMap integration
- ✅ **AI-Powered Analysis**: Roof color validation
- ✅ **SQLite Database**: F-Droid compliant storage
- ✅ **Clean UI**: Avalonia desktop interface

### Build Optimizations:
- ✅ **Single File**: PublishSingleFile=true
- ✅ **Trimmed**: Removes unused code
- ✅ **Compressed**: Smaller download size
- ✅ **Native Libraries**: Included for self-extraction

## 🎯 One-Click Download Workflow

### For All Testing (Simplified):
1. Go to **Releases** tab
2. Download latest `SwipeMyRoof-x86_64.AppImage`
3. Make executable: `chmod +x SwipeMyRoof-x86_64.AppImage`
4. Double-click or run: `./SwipeMyRoof-x86_64.AppImage`

### For PR Testing:
1. Go to **Actions** tab → Select PR workflow run
2. Download `SwipeMyRoof-x86_64.AppImage` from artifacts
3. Same steps as above

## 🔧 Manual Triggering

### Trigger AppImage Build:
1. Go to **Actions** tab
2. Select "Build AppImage" workflow
3. Click "Run workflow"
4. Select branch to build from

## 📊 Build Information

### Build Metadata Included:
- ✅ **Git Commit Hash**: For tracking exact version
- ✅ **Build Date**: UTC timestamp
- ✅ **Branch Name**: Source branch information
- ✅ **Build Type**: TEST vs RELEASE

### File Sizes (Approximate):
- **AppImage**: ~80-120MB (self-contained, portable)
- **Extracted**: ~150-200MB (when AppImage mounts)

## 🐛 Troubleshooting Builds

### Common Build Issues:

**Missing Dependencies:**
- Linux build dependencies are installed automatically
- SkiaSharp native libraries included

**Build Failures:**
- Check Actions logs for detailed error messages
- Ensure all project references are correct
- Verify .NET 8.0 compatibility

**Runtime Issues:**
- Graphics drivers required on target system
- Wayland users may need `GDK_BACKEND=x11`
- AppImage requires FUSE (usually pre-installed)

### Testing Checklist:

**Before Release:**
- [ ] App starts without errors
- [ ] Settings tab loads correctly
- [ ] Can configure API keys
- [ ] Building validation view works
- [ ] Satellite imagery loads (with API key)
- [ ] Swipe gestures function
- [ ] OSM submission works

## 🚀 Simplified Deployment Strategy

### Development Flow:
1. **Feature Branch Push** → AppImage pre-release
2. **Pull Request** → AppImage artifact + PR comment
3. **Main Branch Push** → AppImage pre-release (development)
4. **Manual Trigger** → On-demand AppImage builds

### All Builds Are Pre-releases:
Since the app is in active development, all builds are marked as pre-releases to set proper expectations:
- **Main Branch**: Development builds (latest features)
- **Feature Branches**: Feature test builds (experimental)
- **Pull Requests**: Test artifacts (review builds)

### Benefits:
- ✅ **Single Format**: Only AppImage (universal compatibility)
- ✅ **One-Click Testing**: Download and run immediately
- ✅ **No Confusion**: All builds clearly marked as development
- ✅ **Easy Distribution**: Share single AppImage file
- ✅ **Instant Feedback**: Every push creates testable build

This simplified approach makes it super easy for anyone to test the latest features!