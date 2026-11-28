import { DurableObject } from "cloudflare:workers";

const MAX_BODY = 18000;
const allowedStatuses = new Set(["pending", "approved", "changes requested", "declined"]);
const json = (data, status = 200) => new Response(JSON.stringify(data), { status, headers: { "content-type": "application/json; charset=utf-8", "access-control-allow-origin": "*", "access-control-allow-methods": "GET,POST,PATCH,OPTIONS", "access-control-allow-headers": "content-type,authorization", "cache-control": "no-store" } });
const clean = (v, max) => typeof v === "string" ? v.trim().slice(0, max) : "";

export class LogoRequestStore extends DurableObject {
  async fetch(request) {
    const url = new URL(request.url);
    const parts = url.pathname.split("/").filter(Boolean);
    if (request.method === "OPTIONS") return json({});
    if (request.method === "POST" && parts.join("/") === "api/requests") {
      const raw = await request.text();
      if (raw.length > MAX_BODY) return json({ error: "Request is too large." }, 413);
      let body; try { body = JSON.parse(raw); } catch { return json({ error: "Invalid request." }, 400); }
      const item = {
        id: "HVR-" + crypto.randomUUID().replace(/-/g, "").slice(0, 8).toUpperCase(),
        team: clean(body.team, 50), discord: clean(body.discord, 50), style: clean(body.style, 60),
        colours: clean(body.colours, 80), badgeText: clean(body.badgeText, 60),
        description: clean(body.description, 1200), reference: clean(body.reference, 500),
        status: "pending", note: "", createdAt: new Date().toISOString()
      };
      if (!item.team || !item.discord || !item.style || !item.colours || !item.description) return json({ error: "Please complete all required fields." }, 400);
      if (item.reference && !/^https:\/\//i.test(item.reference)) return json({ error: "Reference image must be an HTTPS link." }, 400);
      await this.ctx.storage.put("request:" + item.id, item);
      const keys = await this.ctx.storage.list({ prefix: "request:" });
      const ids = [...keys.keys()].sort().reverse().slice(0, 150);
      await this.ctx.storage.put("request_index", ids);
      if (this.env.DISCORD_WEBHOOK_URL) {
        const embed = { title: "New logo request • " + item.team, color: 0x438cff, fields: [
          { name: "Request ID", value: item.id, inline: true }, { name: "Discord", value: item.discord, inline: true },
          { name: "Colours", value: item.colours, inline: true }, { name: "Style", value: item.style, inline: true },
          { name: "Badge text", value: item.badgeText || "Not specified" }, { name: "Description", value: item.description }
        ], timestamp: item.createdAt };
        if (item.reference) embed.description = "Reference image: " + item.reference;
        try {
          const sent = await fetch(this.env.DISCORD_WEBHOOK_URL, { method: "POST", headers: { "content-type": "application/json" }, body: JSON.stringify({ username: "HALFTIME VR Logo Desk", embeds: [embed] }) });
          if (!sent.ok) console.log("Discord webhook returned", sent.status);
        } catch (e) { console.log("Discord notification failed", String(e)); }
      }
      return json({ id: item.id, status: item.status }, 201);
    }
    if (request.method === "GET" && parts.length === 3 && parts[0] === "api" && parts[1] === "requests") {
      const item = await this.ctx.storage.get("request:" + parts[2].toUpperCase());
      return item ? json({ id: item.id, team: item.team, status: item.status, note: item.note }) : json({ error: "Request not found." }, 404);
    }
    if (url.pathname === "/api/admin/requests" && request.method === "GET") {
      if (!this.authorised(request)) return json({ error: "Review key is not valid." }, 401);
      const ids = await this.ctx.storage.get("request_index") || [];
      const requests = [];
      for (const key of ids) { const item = await this.ctx.storage.get(key); if (item) requests.push(item); }
      return json({ requests });
    }
    if (parts.length === 4 && parts[0] === "api" && parts[1] === "admin" && parts[2] === "requests" && request.method === "PATCH") {
      if (!this.authorised(request)) return json({ error: "Review key is not valid." }, 401);
      let body; try { body = await request.json(); } catch { return json({ error: "Invalid update." }, 400); }
      const id = parts[3].toUpperCase(), item = await this.ctx.storage.get("request:" + id);
      if (!item) return json({ error: "Request not found." }, 404);
      if (!allowedStatuses.has(body.status)) return json({ error: "Invalid status." }, 400);
      item.status = body.status; item.note = clean(body.note, 500); item.reviewedAt = new Date().toISOString();
      await this.ctx.storage.put("request:" + id, item);
      return json({ id, status: item.status, note: item.note });
    }
    return json({ error: "Not found." }, 404);
  }

  authorised(request) {
    const supplied = request.headers.get("authorization") || "";
    return !!this.env.ADMIN_TOKEN && supplied === "Bearer " + this.env.ADMIN_TOKEN;
  }
}

export default {
  async fetch(request, env) {
    const id = env.LOGO_REQUESTS.idFromName("halftime-logo-requests");
    return env.LOGO_REQUESTS.get(id).fetch(request);
  }
};