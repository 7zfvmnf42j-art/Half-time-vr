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

The Locker Room navigation now includes a Store page with a simple player mannequin preview. Selecting a kit, boots or cap updates the preview before the user presses the buy button. The buy button is deliberately a preview-only placeholder until a real currency, inventory and purchase system is implemented.

The Store page includes a selfie capture button. On Android/Quest builds it captures the current game view and attempts to save the PNG to the device media library under `Pictures/HALFTIME VR` using Android MediaStore. The Quest gallery/file-browser behaviour still needs to be tested on a physical headset. In the Unity Editor or other platforms, the button reports that headset saving is only available on Quest/mobile.


## APK build status

The existing GitHub Actions workflow named `Build HALFTIME VR Quest APK` packages the hosted browser/PWA edition with Bubblewrap. It does **not** compile the Unity project in `UnityProject/`, so its APK will not contain Unity-only features such as this Store page. A native Unity Quest APK still requires a configured Unity Android build environment and a valid Unity license/build configuration. No native Unity APK is claimed until that build completes successfully and is tested on a headset.

The Store mannequin is currently a 2D in-menu preview. It is not yet wired to a persistent 3D match-player avatar or an inventory. The selfie button captures the current game view and attempts to publish the PNG to the Quest media library; physical Quest gallery verification remains outstanding.
