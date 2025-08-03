# CI/CD Pipeline Documentation

## 🚀 Automated Linux Desktop Builds

This repository has three CI/CD workflows for creating downloadable Linux desktop applications:

### 1. **Test Builds** (`build-test-artifacts.yml`)
**Triggers:** Feature branches, Pull Requests
**Purpose:** Quick testing of new features

- ✅ Builds on every feature branch push
- ✅ Creates test artifacts for download
- ✅ Comments on PRs with download links
- ✅ 7-day retention for test builds

**Download:** Go to Actions tab → Select workflow run → Download artifacts

### 2. **Release Builds** (`build-linux-desktop.yml`)
**Triggers:** Main branch pushes
**Purpose:** Stable releases with GitHub releases

- ✅ Self-contained Linux x64 executable
- ✅ Automatic GitHub release creation
- ✅ Both `.tar.gz` and `.zip` formats
- ✅ 30-day retention

**Download:** Go to Releases tab → Download latest release

### 3. **AppImage Builds** (`build-appimage.yml`)
**Triggers:** Main branch pushes, Manual trigger
**Purpose:** Single-file portable application

- ✅ One-click download and run
- ✅ No installation required
- ✅ Universal Linux compatibility
- ✅ Automatic GitHub release

**Download:** Go to Releases tab → Download `.AppImage` file

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

### For Testers (Feature Branches):
1. Go to **Actions** tab
2. Click on latest workflow run
3. Download `SwipeMyRoof-TEST-[branch]-linux-x64.tar.gz`
4. Extract and run `./SwipeMyRoof.sh`

### For Users (Stable Releases):
1. Go to **Releases** tab
2. Download `SwipeMyRoof-x86_64.AppImage`
3. Make executable: `chmod +x SwipeMyRoof-x86_64.AppImage`
4. Double-click or run: `./SwipeMyRoof-x86_64.AppImage`

## 🔧 Manual Triggering

### Trigger AppImage Build:
1. Go to **Actions** tab
2. Select "Build AppImage" workflow
3. Click "Run workflow"
4. Choose whether to create a release

### Trigger Test Build:
1. Go to **Actions** tab
2. Select "Build Test Artifacts" workflow
3. Click "Run workflow"

## 📊 Build Information

### Build Metadata Included:
- ✅ **Git Commit Hash**: For tracking exact version
- ✅ **Build Date**: UTC timestamp
- ✅ **Branch Name**: Source branch information
- ✅ **Build Type**: TEST vs RELEASE

### File Sizes (Approximate):
- **Tar.gz**: ~40-60MB (compressed)
- **AppImage**: ~80-120MB (self-contained)
- **Extracted**: ~150-200MB (with all dependencies)

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

## 🚀 Deployment Strategy

### Development Flow:
1. **Feature Branch** → Test build artifact
2. **Pull Request** → Test build + PR comment
3. **Merge to Main** → Release build + AppImage
4. **Manual Trigger** → On-demand builds

### Release Types:
- **Pre-release**: Feature branch builds
- **Release**: Main branch builds
- **Latest**: Most recent stable AppImage

This setup provides instant feedback for developers and easy downloads for testers and users!