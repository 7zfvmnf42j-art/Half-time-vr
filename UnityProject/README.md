# HALFTIME VR — Unity project

This is the native Unity project track. The existing browser build remains at the repository root as a backup.

## Current foundation
- Unity 6 project structure with OpenXR
- Full-size 105 m × 68 m pitch generated in code
- Goals, penalty areas, centre markings, ball physics and starter player positioning

## Public online lobbies
- Uses Unity Authentication for anonymous player sign-in.
- Uses Unity Lobby to publish and browse public rooms.
- Uses Unity Relay with DTLS for internet connections without sharing IP addresses.
- Public rooms are discoverable in the in-game list; players can refresh and join an open room.
- Players can also join with a room code.
- Current room capacity is 8 players.
- Hosts create a public room and receive a shareable room code.
- The host sends Lobby heartbeats while the room is active.

## Unity Gaming Services setup required
1. Open the project in Unity 6 and sign in to a Unity account.
2. Link the project to a Unity Cloud project in Project Settings → Services.
3. Enable Authentication, Lobby and Relay for that Cloud project.
4. Resolve packages in Package Manager and check the Console for compile errors.
5. Build and test with two clients before distributing a Quest APK.

The code uses anonymous authentication for the prototype. A production release should add account recovery, moderation, reporting and privacy controls.

## Match gameplay foundation
- Local five-minute match clock and headset-facing score display.
- Goal sensors inside both goal frames, score updates and kickoff resets.
- The ball is tagged for goal detection and uses a continuous dynamic Rigidbody.
- This is still a gameplay prototype: no full referee/offside/foul system, player locomotion, passing/possession AI, or network score/ball synchronisation.

## Remaining multiplayer work
Networked player avatars, synced ball physics, team assignment, match clock, score replication, room moderation and reconnect handling are not implemented yet.

## Quest setup
Configure Android Build Support, OpenXR, the Meta Quest feature group and Android player settings in Unity before building an APK. This project has not yet been compiled or tested on a physical Quest headset.

## Store preview and headset selfies

The Locker Room navigation now includes a Store page with a simple player mannequin preview. Selecting a kit, boots or cap updates the preview before the user chooses Equip Item. The selected kit is saved locally and applied to the home-side prototype players in the next generated match. The buy button is deliberately a preview-only placeholder until a real currency, inventory and purchase system is implemented.

The Store page includes a selfie capture button. On Android/Quest builds it captures the current game view and attempts to save the PNG to the device media library under `Pictures/HALFTIME VR` using Android MediaStore. The Quest gallery/file-browser behaviour still needs to be tested on a physical headset. In the Unity Editor or other platforms, the button reports that headset saving is only available on Quest/mobile.


## APK build status

The existing GitHub Actions workflow named `Build HALFTIME VR Quest APK` packages the hosted browser/PWA edition with Bubblewrap. It does **not** compile the Unity project in `UnityProject/`, so its APK will not contain Unity-only features such as this Store page. A native Unity Quest APK still requires a configured Unity Android build environment and a valid Unity license/build configuration. No native Unity APK is claimed until that build completes successfully and is tested on a headset.

The Store mannequin is currently a 2D in-menu preview. It is not yet wired to a persistent 3D match-player avatar or an inventory. The selfie button captures the current game view and attempts to publish the PNG to the Quest media library; physical Quest gallery verification remains outstanding.


## Quest menu input and compatibility fixes

- The generated XR camera now has a gaze pointer. Aim the centre of the headset view at a menu button and press the right controller trigger (or primary button) to click it.
- The EventSystem no longer creates a legacy Standalone Input Module that conflicts with the project's newer XR input path.
- Runtime UI text now requests Unity's LegacyRuntime built-in font instead of the old Arial resource name.
- Players pause movement during the goal reset delay so they do not immediately run away from their kickoff positions.
- The local equipment choice is stored with PlayerPrefs and survives app restarts on the same headset.

These changes have been committed but have not been compiled in Unity or verified on a Quest 3S. The gaze pointer is a basic fallback interaction, not a full controller-ray system. The player preview is still a simple in-menu mannequin, and only the selected home-kit colour is applied to the generated match players; boots and cap are not yet 3D match cosmetics.


## Latest development pass

- Fixed a malformed line in the editor scene setup that would have stopped the C# file compiling.
- Added direct XR runtime head-pose tracking for the generated Quest camera instead of relying on an unconfigured Input System pose driver.
- Connected KICK OFF and PRACTICE to hide the menu and enter the generated match; the menu canvas now has the main camera assigned for world-space UI raycasting.
- Added the missing Store Equip Item handler. Home/away/black shirt choices are saved and applied to the home-side prototype players. Boots and cap choices are saved as locker selections but are not yet visible on the match players.
- Performed a source-level check of the Unity C# files for balanced braces and accidental literal escaped newlines. This is not a Unity compile, runtime test, or Quest hardware test.

## Remaining release blockers

- The repository does not contain a generated HalftimeMatch.unity scene asset yet; the editor setup script must run inside Unity to create it.
- OpenXR Android loader/build-target configuration and a licensed Unity Android build environment still need to be configured.
- No native Unity APK has been built. The existing Bubblewrap workflow packages the browser/PWA edition and must not be mistaken for the Unity Quest game.
- Multiplayer Lobby/Relay code is present, but networked avatars, replicated ball/game state, and a complete synced match are not implemented.
- The generated players are simple capsules with basic ball-following behaviour; full football AI, player rigging, hand/foot interaction, and proper VR locomotion are outstanding.
- The Store preview remains a 2D UI mannequin. Selfie gallery saving and all headset interactions require Quest 3S testing.

## Latest controller gameplay update

- Ball kicking now accepts the right or left Quest controller trigger.
- A kick is only attempted when the tracked controller is within the configured reach of the ball.
- Controller position and rotation are converted from XR tracking space into world space before distance and kick-direction checks.
- The change is committed in the Unity project, but still needs a Unity compile and Quest 3S play test. Controller pose availability and kick direction can vary by headset/runtime and must be verified on-device.


## Match and practice flow

- The match clock remains paused while the main menu is open and starts only when KICK OFF is selected.
- PRACTICE enters a separate untimed practice state; goal detection and kickoff resets remain available.
- The match scoreboard shows READY before kickoff and PRACTICE in practice mode.
- These are source-level changes only; Unity compilation and Quest 3S runtime testing are still required.
