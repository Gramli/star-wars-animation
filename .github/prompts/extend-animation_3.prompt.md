# Task
Improve the existing animation by making transitions between choreography phases smoother and by increasing overall motion dynamism.

Additionally, enhance the Sith Lord character design by adding a helmet and a flowing cape that reacts to movement and Force actions.

---

# Transition Improvements

Transitions between animation phases must feel natural and continuous instead of segmented or abrupt.

## Transition Requirements

- Avoid sudden pose changes between sequences.
- Introduce short transitional poses (1–3 frames) that bridge major actions.
- Use anticipation and follow-through to connect actions smoothly.
- Gradually adjust stance, saber angle, and body orientation when switching phases.
- Movement velocity should accelerate and decelerate naturally instead of changing instantly.
- Characters should maintain motion momentum when entering a new sequence.

---

# Motion Dynamism Improvements

Animation should feel more energetic, fluid, and cinematic.

## Motion Rules

- Increase variation in attack speed and recovery speed.
- Add torso rotation during strikes and dodges.
- Introduce stance shifts and weight transfers during movement.
- Allow slight vertical motion (micro jumps, stance compression, landing recoil).
- Use brief pauses only for dramatic emphasis, not as default spacing.

---

# Sith Lord Visual Enhancement

## Helmet Design

- Sith Lord must wear a recognizable helmet silhouette.
- Helmet should:
  - Extend slightly beyond the head shape
  - Maintain readability during fast motion
  - Preserve strong silhouette contrast
  - Avoid excessive detail that reduces clarity

Helmet silhouette should remain recognizable even in monochrome rendering.

---

## Cape Animation

Sith Lord must wear a cape that dynamically reacts to movement.

### Cape Motion Rules

Cape movement must be physically suggestive and cinematic:

- Cape trails behind Sith during forward movement
- Cape lifts upward during jumps or Force actions
- Cape settles naturally during still poses
- Cape reacts with slight delay relative to body movement
- Cape must never obscure saber blades or arm readability
- Cape motion should use small, consistent character patterns

### Cape Visual Constraints

- Cape must remain part of silhouette readability
- Use subtle wave or trailing shapes
- Avoid excessive flickering or noisy animation
- Keep cape brightness slightly lower than main body silhouette

---

# Silhouette Preservation

Despite added helmet and cape details:

- Head, torso, arms, and legs must remain clearly identifiable.
- Saber arm must remain visually dominant.
- Character poses must remain readable during fast motion.
- Additional details must enhance, not clutter, the silhouette.

---

# Integration Requirements

- Preserve original choreography and timing structure.
- Integrate enhancements into existing ForceSequence and surrounding scenes.
- Maintain consistent frame rate (~20 FPS).
- Maintain deterministic animation behavior.
- Avoid introducing performance spikes or heavy per-frame calculations.

---

# Implementation Guidance

You may:

- Introduce intermediate pose states for smoother transitions
- Add cape offset calculations based on movement direction and speed
- Extend character rendering logic to include helmet and cape layers

You must NOT:

- Rewrite scene architecture unnecessarily
- Introduce external libraries
- Change the core choreography timing significantly

---

# Validation Checklist

Before finalizing implementation, verify:

- Transitions between sequences appear fluid and natural
- Motion shows visible anticipation and follow-through
- Sith helmet remains recognizable in all major poses
- Cape motion reacts logically to movement and Force actions
- Silhouettes remain readable at small terminal sizes
