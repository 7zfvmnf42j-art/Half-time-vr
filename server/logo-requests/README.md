# HALFTIME VR Logo Requests

A small Cloudflare Worker + Durable Object API for the public logo-request website.

## Required GitHub Actions secrets

Add these under repository Settings → Secrets and variables → Actions:

- `CLOUDFLARE_API_TOKEN`: token allowed to deploy Workers.
- `CLOUDFLARE_ACCOUNT_ID`: your Cloudflare account ID.
- `DISCORD_WEBHOOK_URL`: webhook URL for the Discord channel where new logo requests should appear.
- `LOGO_REQUESTS_ADMIN_TOKEN`: a long, private review key used by the website's staff review panel.

Never put the webhook or review key in the website source code or share them in chat.

## Deploy

Run the **Deploy HALFTIME VR logo request service** workflow in the GitHub Actions tab. It prints the Worker URL. Open the logo request website at:

`https://7zfvmnf42j-art.github.io/Half-time-vr/logo-requests/`

The first visit shows a one-time service URL setup field. Paste the Worker URL from the deployment output and save it on the device used for the website.

The website can be used without Discord OAuth: submissions are posted by the configured Discord webhook. Reviewers use the private review key in the Staff review section to approve, decline, or request changes. This is a first version; it does not sign users into Discord or prove that a submitted Discord username belongs to the sender.
