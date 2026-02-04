# Task
Improve the animation background system with two major goals:

1. Make the rendering area dynamically adapt to the user's console window size.
2. Replace the simple starfield-only background with a clear and recognizable spaceship interior environment, while still showing outer space through windows or viewports.

The background must support cinematic immersion without reducing character silhouette clarity or animation readability.

---

# Dynamic Console Size Requirement

The animation must adapt to the console size at application startup.

## Rules

- Detect console width and height when the program starts.
- Use this detected size as the fixed render resolution for the entire animation session.
- Do NOT resize dynamically during animation playback.
- All animation layout, positioning, and choreography must scale relative to this initial console size.

## Layout Guidance

Divide screen space logically:

- Foreground combat area must remain centered.
- Background elements must scale proportionally with screen size.
- Maintain safe margins around screen edges to avoid clipping silhouettes or sabers.

---

# Spaceship Interior Environment

Replace the empty space background with a recognizable spaceship interior while maintaining visibility of outer space.

## Environment Goals

The viewer must immediately understand that:
- The duel takes place inside a spacecraft.
- Space is visible outside the ship through windows or structural openings.

---

# Spaceship Visual Elements

## Structural Interior Elements

Add subtle environment components such as:

- Floor plating or deck panels
- Wall structures or corridor framing
- Window frames or viewport borders
- Optional control panel or structural columns near screen edges

These elements must remain static or move extremely slowly.

---

## Space Visibility

Space must remain visible through:

- Large observation windows
- Side viewport panels
- Rear hangar-style openings

Starfield should only appear outside ship boundaries.

---

# Visual Hierarchy Rules

Background must never compete with the duel.

Priority order:

1. Sabers
2. Clash effects
3. Character silhouettes
4. Spaceship structure
5. Outer space stars

---

# Background Brightness Constraints

- Interior spaceship structure must use dim or medium intensity characters.
- Starfield must remain the faintest visual element.
- Avoid high-contrast background shapes near combat center.

---

# Parallax and Depth (Optional but Recommended)

Add subtle depth illusion:

- Interior ship structures remain static
- Outer space starfield may move extremely slowly
- Window frames help visually separate layers

Movement must be minimal to avoid distraction.

---

# Terminal Rendering Constraints

Use consistent, monospace-safe Unicode characters.

## Suggested Character Vocabulary

Spaceship structure:
- `#`, `|`, `-`, `=`, `+`, `║`, `═`, `╬`, `╣`, `╠`, `╩`, `╦`

Floor plating:
- `_`, `=`, `-`, `.`

Window frames:
- `[]`, `||`, `==`, `╔`, `╗`, `╚`, `╝`

Starfield:
- `.`, `·`, `*` (very sparse and dim)

---

# Composition Guidelines

- Place large viewport windows behind or around combat center.
- Add structural elements primarily near screen edges.
- Avoid placing heavy visual elements directly behind characters.

---

# Performance Requirements

- Background must be generated using deterministic patterns.
- Avoid per-frame random generation.
- Cache background layout where possible.
- Maintain ~20 FPS target.

---

# Integration Constraints

- Preserve existing duel choreography and character positioning logic.
- Background rendering must remain a separate layer from characters and effects.
- Maintain double buffering architecture.

---

# Implementation Guidance

You may:

- Create a background layout generator using console dimensions.
- Use proportional positioning (percent-based coordinates).
- Add simple environment layering (interior structure vs outer space).

You must NOT:

- Introduce external dependencies
- Overcomplicate rendering architecture
- Add excessive background animation

---

# Validation Checklist

Before final output, verify:

- Animation renders correctly on small and large console windows.
- Viewers can instantly recognize spaceship interior environment.
- Background does not reduce silhouette readability.
- Space outside ship remains visible and believable.
