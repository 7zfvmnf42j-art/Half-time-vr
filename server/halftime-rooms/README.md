# HALFTIME VR online rooms

This is the first online room relay for HALFTIME VR. It uses Cloudflare Workers + SQLite-backed Durable Objects and WebSockets.

## What it does
- One room per 4–8 character room code
- Up to 22 connected players in a room (11 per team)
- Relays player, ball and match-event messages
- No game accounts or passwords required for this prototype

This is a networking prototype, not a finished authoritative game server. It is configured for 22 connections per room, but 22-player gameplay has not yet been tested and requires the Quest client to be connected and team assignment/gameplay synchronization to be implemented. The server currently relays allowed JSON messages; it does not validate movement, own the ball physics, prevent cheating, or provide matchmaking. Those protections are needed before public competitive play.

## Deploy (free tier, subject to Cloudflare limits)
1. Create a Cloudflare account and a Workers API token with permission to edit Workers.
2. In Cloudflare, find your account ID.
3. In the GitHub repository settings, add Actions secrets named `CLOUDFLARE_API_TOKEN` and `CLOUDFLARE_ACCOUNT_ID`. Never put either secret in source code or send it in chat.
4. Run the **Deploy HALFTIME VR room server** workflow from the Actions tab.
5. The workflow prints the deployed Worker URL. The room endpoint is `wss://YOUR-WORKER.workers.dev/room/ROOMCODE`.

The Worker is not deployed just by adding these files. Deployment requires the account secrets above. Free usage has daily limits and may stop accepting work after those limits are reached.

## Message protocol
Clients connect using WebSockets to `/room/ROOMCODE`.

The server sends `welcome`, `player_joined`, `player_left`, `pong`, and `error` messages. Clients may send JSON messages with type `player_state`, `ball_state`, `match_event`, or `ping`. The server relays gameplay data to the other players in that room.
