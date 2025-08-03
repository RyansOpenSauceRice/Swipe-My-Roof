# Swipe-My-Roof

A swipe-based mobile application to help validate and add roof colors to OpenStreetMap buildings. Built with C# and designed for privacy-respecting environments like F-Droid and GrapheneOS.

## Project Overview

This app uses a "Tinder-like" swipe interface to make the process of adding roof colors to OpenStreetMap buildings fast, engaging, and accurate. It leverages AI to suggest colors based on satellite imagery, but all decisions are user-confirmed.

## Key Features

- Swipe or button-based interface for quick yes/no decisions
- LLM-powered roof color suggestions
- Practice mode with training and feedback
- Local staging of edits with manual upload control
- Privacy-respecting design (no Google dependencies)
- Cost transparency for LLM API usage

## Documentation

For detailed specifications, see:

- [spec.md](docs/spec.md) - Complete application specification
- [prompts.md](docs/prompts.md) - LLM prompt design and response formats
- [fdroid_manifest.md](docs/fdroid_manifest.md) - F-Droid compatibility guidelines

## Development Status

This project is in the planning stage. See the specification documents for the planned build stages.
