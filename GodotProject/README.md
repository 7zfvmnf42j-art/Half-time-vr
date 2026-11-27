# HALFTIME VR — Godot edition

This folder is the new Godot 4 project. The existing Unity project and website files are left in place.

## Current prototype
- Godot 4 project configured for the Mobile renderer.
- OpenXR startup for a compatible standalone VR headset.
- Basic 3D football pitch, goals, centre circle, scoreboard and a physics football.
- XR origin, headset camera and left/right controller tracking.
- Simple controller-to-ball contact impulse prototype.

## Important
This is an early source prototype, not a finished game or a tested Quest APK. It still needs testing on a Quest headset, proper controller input/action maps, goal detection and match rules, player models/animations, menus and locker, multiplayer, performance tuning, Android export templates and a signed Android build.

Open this folder as a project in Godot 4. Use a Godot version with OpenXR support and configure Android export for Meta Quest before creating an APK.

## Android build
The GitHub Actions workflow builds a debug-signed Android APK for Quest-compatible testing and publishes it as a workflow artifact.
