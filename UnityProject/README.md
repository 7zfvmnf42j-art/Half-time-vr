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

## Online rooms
- Uses Unity Authentication for anonymous player sign-in.
- Uses Unity Lobby to create private rooms and issue shareable room codes.
- Uses Unity Relay with DTLS to connect players over the internet without asking them to share IP addresses or configure port forwarding.
- Current room capacity is 8 players.
- The multiplayer panel has CREATE ROOM, JOIN ROOM and DISCONNECT controls.
- Host room codes are displayed in the menu status text and can be shared with friends.
- This is the connection/session layer only. Networked player avatars, synced ball physics, team assignment, match clock and score replication still need implementing.

## Unity Gaming Services setup required
1. Open the project in Unity 6 and sign in to a Unity account.
2. Link the project to a Unity Cloud project in Project Settings → Services.
3. Enable Authentication, Lobby and Relay for that Cloud project.
4. Resolve packages in Package Manager and check the Console for compile errors.
5. Build and test with two clients before distributing a Quest APK.

The code uses anonymous authentication for the initial prototype. A production release should add account recovery, moderation and privacy controls.

## Quest setup still required
Configure Android Build Support, OpenXR, the Meta Quest feature group, and Android player settings in Unity before building an APK. The starter scene and scripts have not yet been built or tested on a physical Quest headset. This is a multiplayer connection foundation, not a finished 11v11 game.