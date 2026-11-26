# HALFTIME VR — Unity project

This is the native Unity project track. The existing browser build remains at the repository root as a backup.

## Current foundation
- Unity 6 project structure
- OpenXR and XR Origin packages
- Full-size 105 m × 68 m pitch generated in code
- Goals, penalty areas and centre markings
- Match ball with Rigidbody physics
- 11 players per side, including a goalkeeper for each team
- Basic controller-trigger ball interaction and player positioning

## Open the starter scene
1. Open this folder in Unity Hub using Unity 6.0.
2. Let Package Manager finish resolving packages.
3. In Unity, choose **HALFTIME VR → Build Starter Match Scene**.
4. Open `Assets/Scenes/HalftimeMatch.unity` and press Play.

## Quest setup still required
Configure Android Build Support, OpenXR, the Meta Quest feature group, and the Android player settings in Unity before building an APK. The starter scene and scripts have not yet been built or tested on a physical Quest headset. This is the first playable foundation, not a finished 11v11 game.
