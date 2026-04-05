# FreeEdit — Landing Website

Premium black-themed landing site for **FreeEdit**, the AI-powered game world editor that lets you edit a Unity/Unreal level like editing a sentence.

---

## Pages

| Path | Description |
|---|---|
| `index.html` | Main cinematic landing page |
| `api-key.html` | Elegant API key connection page |

---

## Completed Features

### `index.html` — Main Landing
- **Cinematic hero section** — full-viewport dark hero with parallax game-world background, animated typing prompt, before/after comparison frame, and animated badge
- **Tagline strip** — 4-step workflow strip (Type → AI parses → Scene updates → Ship)
- **How It Works** — 3-card grid with icon cards that animate in on scroll
- **Live Prompt Showcase** — split-panel showing a real FreeEdit prompt window with live operation log + game scene image
- **Demo section** — 3-tab before/after carousel (Cyberpunk Alley / Gothic Fortress / Editor View) with command bar
- **Pipeline section** — architecture diagram image + 4-step pipeline cards with highlighted "FreeEdit Layer"
- **Production section** — 2-column layout: feature list + animated checklist UI with progress bars
- **CTA section** — centered, glowing CTA with gradient text
- **Footer** — minimal, linked to API key page

### `api-key.html` — API Key Page
- **Split layout** — form on left, live visual panel on right
- **Animated ambient background** — floating gradient glows + particle canvas + CSS grid pattern
- **API Key form** — password input with show/hide toggle, real-time validation feedback, status dot, and color-coded states (idle / validating / success / error)
- **Connect animation** — loading spinner → success state with connected status + stats row (worlds synced, engines, pipeline)
- **Floating prompt widget** — animated typing prompt in the corner
- **Log terminal** — live "connection log" in the visual panel
- **Security features** — encrypted note, never stored server-side, revoke anytime

### Shared Features
- **Scroll-triggered animations** — IntersectionObserver-based fade-up + reveal-up on all content blocks
- **Animated typing effect** — rotating prompt commands at the hero level
- **Subtle cursor trail** — translucent cyan dots follow the cursor
- **Nav scroll state** — transparent → frosted glass on scroll
- **Demo tab transitions** — animated scene-in on tab change
- **Production bar animation** — progress bars animate in on scroll
- **Hero parallax** — background image translates on scroll
- **Page enter fade** — smooth opacity fade on load
- **8 AI-generated images** — all purpose-built for this product

---

## AI Images Used

| File | Purpose |
|---|---|
| `images/hero-bg.jpg` | Cinematic cyberpunk city — hero background |
| `images/hero-comparison.jpg` | Before/after game world edit (industrial → cyberpunk) |
| `images/product-demo.jpg` | Game engine editor with AI prompt panel |
| `images/gothic-transform.jpg` | Village → Gothic horror before/after |
| `images/pipeline.jpg` | Abstract pipeline flow diagram |
| `images/prompt-window.jpg` | Floating prompt / chat UI |
| `images/checklist-ui.jpg` | Production readiness dashboard |
| `images/api-key-ui.jpg` | API connection interface |

---

## Design System

| Token | Value |
|---|---|
| Background | `#000000` / `#0a0a0a` / `#111111` |
| Accent Cyan | `#00e5ff` |
| Accent Purple | `#9b59ff` |
| Accent Magenta | `#ff2d7e` |
| Success Green | `#00ff88` |
| Font | Inter (variable) |
| Mono Font | JetBrains Mono |
| Border radius | 8px – 24px (contextual) |
| Animation easing | `cubic-bezier(0.16, 1, 0.3, 1)` |

---

## Next Steps

- Add video loop of the actual FreeEdit product in the hero
- Connect API key validation to real backend endpoint
- Add email capture / waitlist form
- Add mobile hamburger menu
- Add Unity/Unreal plugin download CTA section
- Add testimonials/social proof section
