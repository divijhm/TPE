# FreeEdit — SceneOps Demo

Interactive product demo for **FreeEdit for Game Worlds**: an intent-to-safe-edits layer that converts natural language into reviewable 3D scene changes.

Built for the Technology Product & Entrepreneurship (Team DAMN) hackathon pitch.

## Quick Start

```bash
npm install
npm run dev
```

Open `http://localhost:5173` in your browser.

## Where to Place GLB Models (Optional)

The app works fully with procedural fallback geometry. To use GLB models:

```
public/models/alley_before.glb   # Gray/clay alley scene
public/models/alley_after.glb    # Neon cyberpunk alley scene
```

If GLBs are absent, the app renders a procedural alley using primitive geometry (boxes, cylinders, spheres).

## Demo Script — 3 Use Cases

### UC2: Scene Dressing (Hero Demo)

1. Click the **"Scene Dressing"** tab (default)
2. Click the **"Cyberpunk alley"** golden chip
3. Click **"Generate Edit"**
4. Review the 12 proposed changes — note the "Blocking" badge on `Wooden_Crate_Lg`
5. See ghost objects appear in the 3D viewport (cyan = OK, red = blocking)
6. See the walkway zone highlighted on the ground
7. Toggle checkboxes to accept/reject individual ops
8. Click **"Accept Changes"** — scene transforms to neon cyberpunk with bloom effects

### UC3: Selection Edit

1. Click the **"Selection Edit"** tab
2. Click on `Crate_Wood_01` in the 3D viewport (it highlights with an outline)
3. Shift+click `Crate_Wood_02` to multi-select
4. Click the **"Move 2m left"** golden chip
5. Click **"Generate Edit"** → see MOVE operations
6. Click **"Accept Changes"** → crates move left
7. Click **"Undo Last"** → crates return to original position

### UC1: Asset Variants

1. Click the **"Asset Variants"** tab
2. Click the **"Crate variants"** golden chip
3. Click **"Generate Edit"** → see 4 variant cards
4. Click **"Save"** on a variant to add it to the library
5. Click **"Place"** to add the variant to the 3D scene

### Reset

Click **"Reset Demo"** at the bottom of the panel to restore everything to the initial state.

## Key Features

- **Deterministic Planner Stub** — No AI/API calls. Keyword matching produces consistent results.
- **Interactive 3D Viewport** — Orbit, zoom, click to select objects
- **Ghost Preview** — Translucent pulsing objects show proposed ADD placements
- **Walkway Constraint** — Validates that props don't block the walkway zone
- **Before/After Transformation** — Clay to neon cyberpunk with bloom post-processing
- **Undo Support** — Single-step undo for selection edit operations
- **Production Readiness Panel** — Click the gear icon to see simulated validation status

## Tech Stack

- React 18 + Vite + TypeScript
- Tailwind CSS (dark glassmorphism theme)
- react-three-fiber + @react-three/drei (Three.js)
- @react-three/postprocessing (Bloom + Outline)
- framer-motion (animations)
- Zustand (state management)
- lucide-react (icons)

## Architecture

```
src/
  types/       — TypeScript types for operations, plans, scene objects
  store/       — Zustand store with 4 slices (scene, plan, ui, history)
  planner/     — Deterministic planner stub with keyword rules + golden prompts
  engine/      — Scene ops engine (preview, apply, undo)
  scene/       — React Three Fiber 3D components
  components/  — UI components (layout, panel, UC-specific, shared)
  data/        — Procedural scene data, prefabs, fixtures
  utils/       — AABB math, ID generation
```

## No Real AI

This is a demo/prototype. The `generatePlan()` function in `src/planner/generatePlan.ts` uses keyword matching to produce deterministic results. The "golden prompts" (clickable chips) return curated plans for reliable demos.
