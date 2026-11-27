import { DurableObject } from "cloudflare:workers";

const ROOM_CODE = /^[A-Z0-9]{4,8}$/;
const MAX_PLAYERS = 22;
const MAX_MESSAGE_LENGTH = 4096;

export class GameRoom extends DurableObject {
  constructor(ctx, env) {
    super(ctx, env);
    this.ctx = ctx;
  }

  async fetch(request) {
    if (request.headers.get("Upgrade")?.toLowerCase() !== "websocket") {
      return new Response("WebSocket connection required", { status: 426 });
    }

    const sockets = this.ctx.getWebSockets();
    if (sockets.length >= MAX_PLAYERS) {
      return new Response("Room is full", { status: 409 });
    }

    const pair = new WebSocketPair();
    const [client, server] = Object.values(pair);
    this.ctx.acceptWebSocket(server);
    server.serializeAttachment({ playerId: crypto.randomUUID() });

    server.send(JSON.stringify({
      type: "welcome",
      playerId: server.deserializeAttachment().playerId,
      players: this.ctx.getWebSockets().length,
      maxPlayers: MAX_PLAYERS
    }));
    this.broadcast({ type: "player_joined", players: this.ctx.getWebSockets().length }, server);

    return new Response(null, { status: 101, webSocket: client });
  }

  webSocketMessage(sender, message) {
    if (typeof message !== "string" || message.length > MAX_MESSAGE_LENGTH) {
      sender.close(1009, "Message too large");
      return;
    }

    let data;
    try {
      data = JSON.parse(message);
    } catch {
      sender.send(JSON.stringify({ type: "error", message: "Send valid JSON" }));
      return;
    }

    // This first version relays match updates; game authority and anti-cheat
    // validation must be added before public competitive matches.
    if (!data || !["player_state", "ball_state", "match_event", "ping"].includes(data.type)) {
      sender.send(JSON.stringify({ type: "error", message: "Unsupported message type" }));
      return;
    }

    this.broadcast({
      type: data.type,
      playerId: sender.deserializeAttachment()?.playerId ?? null,
      data: data.data ?? null
    }, sender);
    if (data.type === "ping") {
      sender.send(JSON.stringify({ type: "pong" }));
    }
  }

  webSocketClose(sender) {
    try { sender.close(); } catch {}
    this.broadcast({ type: "player_left", players: this.ctx.getWebSockets().length });
  }

  webSocketError(sender) {
    try { sender.close(1011, "Connection error"); } catch {}
  }

  broadcast(payload, except = null) {
    const encoded = JSON.stringify(payload);
    for (const socket of this.ctx.getWebSockets()) {
      if (socket !== except) {
        try { socket.send(encoded); } catch {}
      }
    }
  }
}

export default {
  async fetch(request, env) {
    const url = new URL(request.url);
    const match = url.pathname.match(/^\/room\/([A-Za-z0-9]{4,8})$/);
    if (!match) {
      return new Response("HALFTIME VR room server. Connect to /room/ROOMCODE.", {
        headers: { "content-type": "text/plain; charset=utf-8" }
      });
    }
    if (request.headers.get("Upgrade")?.toLowerCase() !== "websocket") {
      return new Response("Use a WebSocket connection.", { status: 426 });
    }

    const roomCode = match[1].toUpperCase();
    const id = env.GAME_ROOMS.idFromName(roomCode);
    return env.GAME_ROOMS.get(id).fetch(request);
  }
};
