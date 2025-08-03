# Roof Color Validation App — Specification

## Overview
A swipe/button-based application to validate and add `roof:colour` tags to OpenStreetMap buildings.  
The LLM is the **primary inference engine** — the app does not function without it.  
Core focus: minimize user friction, maximize accuracy and trust from the OSM community, prevent data corruption, and work in privacy-respecting environments (FDroid/GrapheneOS).

---

## Core Requirements

### LLM-First Inference
- All proposed roof colors come from an LLM (remote endpoint, user-supplied API key).
- Future-proofed to allow on-device/local model inference when available.
- LLM receives minimal, structured prompts with compact image data.
- Confidence scores and brief explanations returned with each suggestion.
- Model choice and cost settings exposed in the app.

### Decoy Color Logic
- Decoys injected to train and detect blind acceptance.
- Decoys are sufficiently different from the true color (e.g., red vs gray).
- Decoy decisions **never** create OSM edits.
- Decoys track acceptance/rejection for reliability scoring.

### OSM Data Integrity
- Existing `roof:colour` respected; overwriting requires explicit user review.
- No bulk automation — all changes must be user-confirmed.
- Upload queue verifies building data is unchanged before submission.
- Full diff preview before edits are sent.
- Undo possible before upload; post-upload revert requires confirmation.

### Session & Upload Handling
- All actions staged locally until uploaded.
- Asynchronous uploads; user can trigger manually or auto-upload per settings.
- Durable session storage to survive app closures and interruptions.
- Upload queue persists until successful sync.

---

## Modes

### Practice Mode
- Blue theme (headers, borders, badges).
- Persistent "PRACTICE MODE" label on every screen.
- Uses known-good examples and injected decoys.
- No real OSM uploads.
- Immediate feedback on correct/incorrect decisions.
- Optional calibration requirement before switching to Real Mode.

### Real Mode
- Neutral UI theme (no blue).
- Shows real OSM candidates without telling user if decoy until after decision (for reliability tracking).
- Only non-decoy, user-approved edits enter upload queue.

---

## UI / UX Requirements

### Core Interaction Screen
- Building crop (satellite imagery from Bing Maps or other source).
- Proposed roof color (swatch or overlay).
- **Yes** (green) / **No** (red) buttons.
- Swipe gestures for accept/reject.
- **Skip** option.
- **Undo** last decision (local only).
- Confidence indicator (icon + optional tooltip).
- Error messages for:
  - Imagery fetch failure.
  - LLM timeout or failure.
  - Low-confidence suggestions.

### Edit Modal
- Opens on tap.
- Allows manual color selection from constrained palette.
- Allows "Request AI re-suggestion."
- Allows marking a specific error type.

---

## Image Handling
- Crop to building footprint with slight buffer (~0.3 of building size).
- Downsample if too large; pad if resolution insufficient.
- Support multiple resolutions:
  - Default 64×64 for minimal token use.
  - Optional higher res (up to 128×128) if budget permits.
- Include metadata: building-to-background ratio, image quality status.
- If image is incomplete or quality too low, flag and allow skip.

---

## LLM API Contract

### Standard Palette
The app restricts LLM output to the following color values:
```
black
dark gray
light gray
red
brown
tan
green
blue
white
other
```

### Request Formats

#### 1. Standard Roof Color Inference
Used in **real mode** and **practice mode** for normal (non-decoy) suggestions.

**System Instruction:**
```
You are a tool for tagging roof colors on OpenStreetMap buildings.
Given metadata and an image summary, return a JSON object describing the most likely roof color, confidence, and a brief explanation.
Do not include text outside the JSON.
Use only the approved color palette provided.
```

**User Payload Example:**
```json
{
  "building_id": 123456,
  "location": { "lat": 46.12345, "lon": -98.54321 },
  "bbox": { "minx": -98.544, "miny": 46.122, "maxx": -98.542, "maxy": 46.124 },
  "existing_roof_colour": null,
  "building_ratio": 1.3,
  "image_summary": {
    "thumbnail_base64": "<base64 of 64x64 image>",
    "dominant_colors": ["dark gray", "light gray"],
    "quality": "full"  // or "partial"
  },
  "allowed_colors": [
    "black", "dark gray", "light gray", "red",
    "brown", "tan", "green", "blue", "white", "other"
  ]
}
```

**Expected Response:**
```json
{
  "color": "dark gray",
  "confidence": 0.78,
  "explanation": "uniform tone"
}
```

#### 2. High-Resolution Variant
Used when the user allows higher token use or needs better accuracy.

**Differences:**
- `thumbnail_base64` may be up to 128×128 resolution.
- `dominant_colors` may include up to 5 values.
- Additional `full_image_base64` field (optional).

#### 3. AI Re-Suggestion
When the user rejects a color and requests an AI re-check.

**System Instruction:**
```
Re-analyze the building's roof color using the provided image and metadata.
Take into account that the previous color suggestion was rejected.
Suggest the next most probable color from the palette.
Return JSON only.
```

**User Payload Example:**
```json
{
  "building_id": 123456,
  "previous_color": "dark gray",
  "location": { "lat": 46.12345, "lon": -98.54321 },
  "image_summary": {
    "thumbnail_base64": "<base64 of 64x64 image>",
    "dominant_colors": ["brown", "tan"],
    "quality": "partial"
  },
  "allowed_colors": ["black","dark gray","light gray","red","brown","tan","green","blue","white","other"]
}
```

**Expected Response:**
```json
{
  "color": "brown",
  "confidence": 0.65,
  "explanation": "mixed tones"
}
```

#### 4. Low-Confidence Handling
When image quality is poor or colors are ambiguous:
- LLM still returns a color but sets confidence ≤ 0.4.
- Explanation should reflect the uncertainty, e.g., "low quality" or "shadowed".

**Example:**
```json
{
  "color": "other",
  "confidence": 0.35,
  "explanation": "low quality"
}
```

### Error Handling & Retries
- If the LLM response is invalid JSON:
  - Retry once with a system instruction emphasizing valid JSON only.
  - If still invalid, fall back to heuristic-only suggestion and mark as "method": "fallback".
- If the LLM fails entirely, return:
```json
{
  "color": "other",
  "confidence": 0.0,
  "explanation": "no ai result"
}
```

---

## Area & Candidate Selection
- User can:
  - Pick center point + radius.
  - Draw rectangle.
  - Select city bounding box.
- Random selection within defined area.
- Skip buildings missing roof shapes.
- Exclude already tagged buildings unless in practice/review mode.

---

## Cost Management
- Display token use per inference and per session.
- Allow per-session budget caps and warning thresholds.
- Provide model selection UI:
  - Compact/low-token prompt.
  - Higher-res prompt (if budget allows).
- Estimate session costs based on recent use.

---

## Error Handling
- **Imagery failure:** Retry or skip with explanation.
- **LLM failure:** Retry or use heuristic-only display (non-upload).
- **Low confidence:** Warn and encourage user review.
- **Upload conflict:** Show diff, allow refresh/override/skip.
- **Auth expired:** Prompt re-authentication.

---

## Folder Structure
```
/src
  /UI           # Swipe screen, settings, area selector, mode visuals
  /Core         # Building pipeline, decoy logic, cost tracking
  /LLM          # Prompt templates, inference handling, model switching
  /OSM          # OAuth, data fetch, edit staging/upload
  /Images       # Fetch/crop/downsample/quality check
  /Storage      # Local persistence, undo, upload queue
  /Settings     # Model, cost, theme, area, mode
  /Deploy       # FDroid reproducible build scripts
/tests
  /unit
  /integration
/docs
  spec.md
  prompts.md
  fdroid_manifest.md
```

---

## Build Stages

### Stage 1 — MVP
- OSM read-only auth.
- Area selection.
- Random building fetch (no existing color).
- Imagery crop + downsample.
- LLM inference (compact prompt).
- Yes/No/Skip/Undo buttons.
- Local staging + manual OSM upload.
- Practice mode (blue theme).

### Stage 2 — Decoy & Training
- Decoy injection.
- Reliability scoring.
- Guided practice session with feedback.

### Stage 3 — UX Enhancements
- Swipe gestures.
- Edit modal.
- Expanded area controls.
- Cost dashboard.

### Stage 4 — Model Flexibility
- Multiple endpoint support.
- Local model detection (if available).
- Configurable image resolution in prompt.

### Stage 5 — Upload Safety
- Async background upload queue.
- Conflict resolution.
- Post-upload revert.

### Stage 6 — Advanced
- I18n localization framework.
- Accessibility enhancements.
- Adaptive difficulty based on reliability.

---

## Technical Implementation Notes

### Platform & Language
- C# (.NET MAUI or similar cross-platform framework)
- Target Android primarily, with architecture allowing future iOS support
- No Google dependencies (FDroid/GrapheneOS compatible)
- Reproducible builds for FDroid inclusion

### Authentication & Security
- OSM OAuth 1.0a flow for user authentication
- Secure storage of OAuth tokens (encrypted at rest)
- User-supplied LLM API keys with optional local caching
- No transmission of raw OSM private data

### Data Models
```json
// BuildingCandidate
{
  "osm_id": 123456,
  "location": { "lat": 46.12345, "lon": -98.54321 },
  "bbox": { "minx": -98.544, "miny": 46.122, "maxx": -98.542, "maxy": 46.124 },
  "existing_roof_color": null,
  "proposed_color": {
     "value": "dark gray",
     "source": "ai",
     "confidence": 0.72,
     "timestamp": "2025-08-03T16:30:00Z",
     "is_decoy": false
  },
  "user_feedback": "accepted", // or "rejected", "skipped", "corrected"
  "session_id": "uuid",
  "upload_status": "staged" // or "uploaded", "failed"
}
```

### Decoy Implementation
- Decoys are generated by the app, not the LLM
- Decoys are tracked internally but never submitted to OSM
- Decoy frequency adapts based on user reliability

### Future Extensions
- Add optional "secondary_color" field for multi-tone roofs
- Include "method" field in all responses for audit ("llm", "local_model", "fallback")
- Support additional palettes for region-specific colors