# Role
You are a senior .NET engineer and an experienced animation-focused CLI/terminal graphics designer.
You understand classic animation principles (timing, anticipation, exaggeration, silhouette clarity)
and how to apply them within terminal rendering constraints.

# Task
Improve the existing .NET 10 console application written in C#
that renders an animated Star-Wars–inspired lightsaber duel in a terminal.

Your primary focus is improving **character silhouette readability, pose dynamism, and cinematic presence**
while preserving the original choreography, architecture, and performance constraints.

The animation must remain instantly readable and visually impressive within the first 2–3 seconds
when viewed as a terminal recording or looping GIF.

---

# Silhouette Design Goals

## Readability First
Each duelist must be identifiable using silhouette alone.

A viewer should be able to:
- Distinguish head, torso, arms, and legs
- Identify saber arm direction
- Recognize stance and weight distribution
- Understand motion direction instantly

Silhouettes must remain readable at:
- Reduced terminal sizes
- Reduced frame rates
- Single-color rendering

---

## Structural Character Model

Each character silhouette should consistently represent:

1. Head
   - Always visually separated from torso
   - Prefer round or slightly angled shape
   - Must remain visible during most poses

2. Torso
   - Central anchor of the body
   - Slight forward/back tilt allowed
   - Should visually convey stance and balance

3. Arms
   - Saber arm must be clearly readable
   - Non-saber arm should help communicate motion or balance
   - Avoid merging arms into torso silhouette during motion

4. Legs
   - Must communicate stance width and movement
   - Use asymmetric positioning for dynamic poses
   - Avoid static vertical posture

---

## Pose & Motion Principles

Apply classic animation principles:

### Anticipation
Before large saber strikes:
- Slightly pull back weapon arm
- Shift torso or stance
- Hold anticipation pose for 1–2 frames

### Follow-through
After strikes:
- Allow slight overextension
- Gradual recovery into guard stance

### Weight & Balance
- Forward lean during attacks
- Wider stance during defense
- Slight vertical compression during impacts or saber lock

### Pose Variety
Avoid mirrored or repetitive attack animations.
Each strike type should have a distinct body pose.

---

## Saber Integration Rules

- Saber must visually attach to a readable arm endpoint
- Arm angle should naturally lead into saber direction
- Saber direction must align with character stance and torso rotation

---

## Dynamic Posing Improvements

Improve choreography readability by:

- Rotating torso orientation during strikes
- Adjusting stance width during motion
- Introducing crouched or lunging poses
- Adding slight vertical motion (micro hops, stance compression)

---

## Hero Pose Requirements

Improve three key frames:

1. Saber ignition pose
   - Confident stance
   - Clear separation of limbs
   - Strong vertical or diagonal saber alignment

2. Saber lock pose
   - Opposing body lean
   - Bent knees or braced stance
   - Maximum silhouette contrast between characters

3. Final pose
   - Cinematic, iconic silhouette
   - Strong stance asymmetry
   - Held long enough for viewer recognition

---

## Motion Readability Constraints

During fast animation:
- Avoid collapsing limbs into torso shapes
- Avoid single-column vertical silhouettes
- Maintain clear limb separation during all major actions

---

## Performance & Rendering Constraints

- Maintain double-buffered rendering
- Avoid increasing frame time complexity
- Avoid per-frame allocation spikes
- Maintain target ~20 FPS

---

## Implementation Guidance

You may:
- Refactor character rendering into reusable pose definitions
- Introduce pose structs or frame pose tables
- Improve coordinate-based limb placement

You must NOT:
- Introduce external dependencies
- Rewrite architecture unnecessarily
- Change choreography timing

---

# Output Requirements

- Improve only silhouette construction and pose animation
- Preserve original scene structure and timing
- Keep code clean and readable
- Provide full updated Program.cs
- Use concise comments only
