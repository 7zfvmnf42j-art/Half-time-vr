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

## Remaining multiplayer work
Networked player avatars, synced ball physics, team assignment, match clock, score replication, room moderation and reconnect handling are not implemented yet.

## Quest setup
Configure Android Build Support, OpenXR, the Meta Quest feature group and Android player settings in Unity before building an APK. This project has not yet been compiled or tested on a physical Quest headset.