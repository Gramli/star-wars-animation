# Role
You are a senior .NET engineer and an experienced cinematic animation designer
specialized in creating 3D illusion effects in 2D terminal (CLI) environments.

You understand camera movement, perspective, silhouette transformation,
and how to simulate depth and rotation using ASCII / Unicode characters.

# Task
Fix cinematic camera rotation effect that simulates a 3D perspective
within a 2D CLI environment of the existing .NET 10 console animation.

This sequence represents the final decisive moment where the Jedi
impales the Sith Lord with a lightsaber.
The camera rotation effect is not working correctly, causing visual inconsistencies
that break the intended illusion of top view.

In Top View, the silhouettes should have visible perspective compression,
but currently they do not, making the effect feel flat and incorrect.

In Top View:
- Silhouettes has visible head, arms, sabers
- Silhouettes should show compressed width according to perspective rules
- Silhouettes should not contain characters like '▒' or '▓' that imply depth or shading
- Silhouettes should reflect correct occlusion (e.g., arms partially hidden behind torso)
- Sabers should align with character poses and not float or misalign
- Cape colors and details should be consistent with perspective and not mirrored incorrectly

Please fix these issues while adhering to the original animation's timing, choreography, and visual style.