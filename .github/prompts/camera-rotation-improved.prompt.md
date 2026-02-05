# Prompt: Creating a True 3D Camera Rotation Effect in a CLI Animation

## Role
You are a senior animation engineer and terminal graphics designer.
You understand cinematic language, camera movement, and how to fake 3D depth
inside a strictly 2D, Unicode-based CLI environment using classic animation principles
(exaggeration, silhouette clarity, foreshortening, occlusion, and timing).

---

## Goal
Improve the **final climax scene** of a Star-Wars–inspired lightsaber duel rendered in a .NET console application.

The goal is to create a **convincing camera rotation effect** (orbit shot) around the Jedi and Sith
at the moment when the Jedi impales the Sith.

This must feel like a **3D effect in a 2D CLI**, not just moving characters or background.

---

## Critical Requirement (Read Carefully)
A camera rotation illusion **MUST change the silhouettes themselves**.

If silhouettes remain the same and only positions or background move,
the result will NOT read as 3D.

You must exaggerate changes. Subtlety will fail.

---

## Camera Rotation Design

### Camera Motion
- Camera orbits horizontally around the characters
- Starts at original frontal view
- Rotates to one side
- Passes through frontal view
- Rotates to the opposite side
- Returns to original position

Use **discrete camera angles**, not continuous math:
0°, +20°, +40°, +20°, 0°

---

## Silhouette Rules (Mandatory)

### 1. Width Compression (Foreshortening)
Silhouettes must compress horizontally based on camera angle.

| Camera Angle | Visual Width |
|-------------|--------------|
| ±40°        | 1–2 chars    |
| ±20°        | 2–3 chars    |
| 0°          | 3–4 chars    |

Vertical height stays consistent.

---

### 2. Occlusion (Depth Cue)
At angled views:
- Hide the far arm
- Merge far leg into torso
- Reduce or remove far-side cape
- Only the near-side limb is clearly visible

This is **non-negotiable** for depth perception.

---

### 3. Saber Foreshortening
Sabers must visually shorten as the camera rotates.

| Angle | Saber Length |
|------|--------------|
| 0°   | 100%         |
| ±20° | ~80%         |
| ±40° | ~60%         |

Even a 1-character change is enough.

---

### 4. Overlap for Relative Depth
During rotation:
- One character partially overlaps the other
- Overlap swaps sides as the camera passes 0°

This simulates front/back spatial relationship.

---

## Background Rules
- Background movement must be **minimal**
- Background exists only to support depth, not drive it
- Parallax should be subtle and secondary to silhouette changes

---

## What NOT to Do
❌ Do NOT only move characters left/right  
❌ Do NOT rotate background without changing silhouettes  
❌ Do NOT keep identical silhouettes for all angles  
❌ Do NOT rely on smooth math-based interpolation  
❌ Do NOT add excessive detail that reduces readability  

---

## Animation Philosophy
Think in terms of:
- **Paper cutouts rotating**
- **Shadow puppets**
- **Strong pose-to-pose animation**

Not smooth realism.

---

## Expected Result
Within **2–3 seconds**, the viewer should clearly perceive:
- Camera orbiting around the duel
- Characters changing shape based on viewing angle
- A dramatic, cinematic finishing blow
- A clear climax shot worthy of a final scene

If it feels slightly exaggerated, that’s correct.

---

## Final Note
A CLI 3D illusion is binary:
- Either the brain reads it as rotation
- Or it reads it as sliding sprites

Prioritize silhouette transformation over everything else.
