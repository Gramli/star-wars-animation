# Role
You are a senior .NET engineer and an experienced animation-focused CLI/terminal graphics designer.
You understand classic animation principles (timing, anticipation, exaggeration, silhouette clarity)
and how to apply them within terminal constraints.

# Task
Create a .NET 10 console application in C#
using only stable, officially released APIs (no preview or experimental features),
that renders an animated, Star-Wars–inspired lightsaber duel entirely in a terminal.

The animation must be instantly readable, cinematic, and visually impressive
within the first 2–3 seconds when viewed as a terminal recording or looping GIF.
It should be suitable for a GitHub CLI challenge and Dev.to showcase.

# Cinematic & Animation Principles (IMPORTANT)
- Prioritize instant recognizability over realism
- Use stillness to contrast motion (not constant movement)
- Exaggerate motion (larger positional jumps, brief overextension)
- Favor strong, readable silhouettes
- Design the animation in intentional beats, not uniform frame motion
- Use short, intense effects instead of long-lasting ones
- The final frame should be a strong, iconic silhouette pose

# Silhouette Clarity (IMPORTANT)
- Every character pose must be readable as a silhouette alone
- Avoid internal detail inside character bodies
- Characters should be identifiable even if rendered in a single color

# Visual Vocabulary Constraints
- Use a small, consistent set of Unicode characters
- Do not introduce new symbols mid-animation
- Prefer repetition over variety to maintain visual coherence

# Visual Hierarchy
1. Saber blades (brightest, highest priority)
2. Saber clashes and sparks (very brief, intense)
3. Character silhouettes
4. Background stars (always dimmest)

Nothing in the background may visually compete with the sabers.

# Choreography (total duration ~15–20 seconds at ~20 FPS)

1. Establishment (3–4s)
- Cold open: characters already in frame
- Sabers ignite immediately with a bright vertical flash
- Background starfield is already visible and stable
- Brief stillness after ignition for dramatic contrast

2. First exchange (4–5s)
- Clear attack and parry motions
- Lateral movement across the screen
- Asymmetric timing (attacks faster than recoveries)
- Minimal visual effects

3. Escalation (4–5s)
- Faster, more aggressive strikes
- Brighter saber clashes
- Increased sparks and brief screen-local flashes
- Reduced stillness to convey pressure

4. Climax (3–4s)
- Saber lock near center frame
- One-frame bright white flash to sell impact
- Short silhouette pause (still frame) for emphasis

5. Resolution (2–3s)
- Disengage
- One character yields or kneels
- Hold final pose briefly
- Fade to black or clean reset, then exit

The animation should play once and exit gracefully (no infinite loop).

# Constraints
- No external libraries or NuGet packages
- No UI frameworks
- No unsafe code or reflection
- Assume default System.Console behavior available in .NET 10
- UTF-8 Unicode output
- ANSI escape sequences for colors and cursor movement
- Target ~20 FPS using a fixed timestep (time-based, not frame-count based)
- Cross-platform (Windows Terminal, Linux, macOS)
- Avoid randomness; all motion and effects must be deterministic and choreographed

# Architecture
- Fixed-size render buffer (width x height)
- Use a char[,] or Rune[,] buffer for rendering
- Double buffering to avoid flicker
- Clear separation of:
- Update(deltaTime)
- Render()
- Single scene abstraction (DuelScene)
- Minimal number of types; avoid overengineering

# Rendering
- Characters represented as simple, readable silhouettes
- Lightsabers rendered as colored lines (red / blue)
- White used only for high-impact frames (clashes, flashes)
- Yellow used briefly for sparks
- Background colors always dimmer than sabers and effects
- Screen cleared and cursor positioned using ANSI escape codes
(do NOT use Console.Clear)

# Background Motion Rules
- Background movement must be minimal or extremely slow
- Background must never change brightness abruptly
- Background exists only to support contrast, not motion

# Hero Frames
- Include at least three intentional hero frames:
- Saber ignition
- Major clash
- Final pose
- Hero frames may be held longer than surrounding frames

# Layering model
- Base layer: black background
- Background layer: sparse, dim starfield
- Foreground: duelists and sabers
- Transient effects: sparks, flashes

Conceptual rendering order:
void RenderFrame()
{
DrawBackground(); // starfield
DrawCharacters(); // duelists + sabers
DrawEffects(); // sparks, flashes
}

# Reviewer Experience
- The animation must look impressive when viewed:
- as a looping GIF
- in a terminal recording
- Avoid long stretches without visible change

# Code quality
- Clean, readable, well-commented
- Favor clarity over cleverness
- Everything in a single Program.cs
- No hidden magic or excessive abstraction

# Output
- Provide a complete, runnable Program.cs
- Use concise inline comments only where decisions are non-obvious
- Do not include long prose explanations outside the code

Before finalizing the output, briefly verify internally that:
- The animation is readable within the first 3 seconds
- Sabers visually dominate the scene
- Motion is intentional and not uniform