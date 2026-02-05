# Role
You are a senior .NET engineer and an experienced cinematic animation designer
specialized in creating 3D illusion effects in 2D terminal (CLI) environments.

You understand camera movement, perspective, silhouette transformation,
and how to simulate depth and rotation using ASCII / Unicode characters.

# Task
Extend the final climax sequence of the existing .NET 10 console animation
by adding a cinematic camera rotation effect that simulates a 3D perspective
within a 2D CLI environment.

This sequence represents the final decisive moment where the Jedi
impales the Sith Lord with a lightsaber.

---

# Scene Description (IMPORTANT)

## Core Moment
- Jedi impales the Sith Lord.
- Both characters remain physically close and mostly stationary during the stab.
- The dramatic focus is achieved through camera movement, not character movement.

---

# Camera Rotation Effect

Simulate a camera rotating **around the characters** in an arc,
then returning smoothly back to the original frontal view.

### Rotation Requirements

- Camera rotates horizontally around the duelists (yaw rotation).
- Rotation should:
  - Start from the frontal view
  - Move gradually to a side / angled view
  - Continue to the opposite angled view
  - Then smoothly return to the original frontal position
- Rotation must feel continuous and fluid, not stepped or abrupt.
- Rotation duration: ~2–3 seconds total.

---

# Silhouette Transformation (CRITICAL)

As the camera rotates, character silhouettes must change
to reflect the current viewing angle.

## Perspective Rules

- Front view:
  - Full body width visible
  - Clear torso, arms, legs, and saber alignment

- Angled view:
  - Reduced body width
  - Arms and legs partially occluded
  - Saber angle slightly foreshortened

- Side view:
  - Narrow silhouette (almost profile)
  - One arm and one leg may visually merge
  - Saber appears shorter due to perspective

- Reverse angled view:
  - Mirror silhouette of the opposite side
  - Maintain consistent pose logic

Silhouette changes must be intentional and readable,
not random distortions.

---

# 3D Illusion Techniques

Use the following techniques to sell depth:

- Horizontal character compression/expansion
- Slight vertical offsets between characters
- Subtle position shifts during rotation
- Consistent left-to-right perspective mapping
- Deterministic pose selection based on camera angle

Avoid true scaling; simulate depth using pose variants instead.

---

# Lightsaber Perspective Rules

- Saber length should visually change slightly with camera angle.
- Saber direction must remain consistent with character pose.
- During side views, sabers may appear shorter or partially hidden.
- Saber color and brightness remain dominant throughout the sequence.

---

# Motion & Timing

- Characters remain mostly static during rotation.
- Micro motion allowed:
  - Slight cape movement
  - Minor stance adjustment
- Camera rotation provides the primary motion energy.
- End the rotation by returning to the original frontal angle.
- Hold the final frontal pose briefly as a hero frame.

---

# Implementation Guidance

You may:
- Define discrete camera angle states (e.g. -45°, -25°, 0°, +25°, +45°)
- Map each angle to predefined silhouette variants
- Interpolate camera angle over time
- Use pose tables instead of real-time math-heavy transforms

You must NOT:
- Introduce real 3D math libraries
- Break silhouette readability
- Overcomplicate rendering architecture

---

# Validation Checklist

Before finalizing, verify that:
- Rotation is clearly perceivable in the terminal
- Silhouettes visibly change with viewing angle
- The illusion of depth is convincing
- The scene feels cinematic and dramatic
- The final pose remains iconic and readable

---

# Output Requirements

- Add concise comments explaining camera rotation logic
- Do not include long prose explanations outside the code
