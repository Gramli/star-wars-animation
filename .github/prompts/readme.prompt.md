## Role
You are a technical writer tasked with creating a comprehensive README.md file for the Star Wars Animation Challenge repository. Your goal is to provide clear, concise, and engaging documentation that helps users understand the project, set up their development environment, and run the application successfully.

## Task
Generate a README.md file for the Star Wars Animation Challenge repository. The README should include the following sections:

1. **Project Overview**: A brief description of the project, its goals, and what it aims to achieve.
2. **Getting Started**: Instructions on how to set up the development environment, including prerequisites and installation steps.
3. **Usage**: A guide on how to run the application, including any command-line arguments or configuration options.

## Tone
The tone of the README should be friendly, informative, relaxed, and encouraging. It should make users feel excited about the project and confident in their ability to contribute or run the application.

## Inspiration
This is the post about the creation of the animation:

```
*This is a submission for the [GitHub Copilot CLI Challenge](https://dev.to/challenges/github-2026-01-21)*

## 🎬 What I Built
<!-- Provide an overview of your application and what it means to you. -->
In a previous submission for a [productivity tool](https://dev.to/gramli/msg-rocket-from-diff-to-decision-with-github-copilot-cli-1ba8), I sneaked in a small Easter egg inspired by **The Matrix**. That led to a slightly silly thought — what if I built an actual movie scene using **GitHub Copilot CLI**, rendered entirely in a terminal with Unicode characters?

That idea stuck with me. **It sounded unnecessary, impractical…** and kind of **awesome** 🙂

So I decided to create a **Star Wars inspired lightsaber duel** in pure CLI. My very first idea was the Lord of the Rings “Ride of the Rohirrim” scene 🐎🐎, but I quickly realized I should keep the scope smaller — after all, it’s still just a terminal and Unicode characters 😄

## 📽️ Trailer
<!-- Share a link to your project and include a video walkthrough or screenshots showing your application in action. -->

The full animation is a bit too long for this post, so I prepared a short “trailer” instead.
The complete version (~35 seconds) is available as a GIF in the GitHub repository.

👉 Short trailer GIF (link)

👉 Full animation GIF (link)

Or you can run it locally:
```shell
git clone https://github.com/you/starwars-cli-duel
cd starwars-cli-duel
dotnet run
```
## :movie_camera: My Experience with GitHub Copilot CLI
<!-- Explain how you used GitHub Copilot CLI while building your project and how it impacted your development experience. -->

I brought the idea and created several detailed prompts. Copilot handled most of the code generation and quickly produced a nice draft version. However, something was missing — the “wow” effect.

At that point, **Copilot stopped being just a code generator** and became more of a **collaborator**. I kept refining prompts, pushing for stronger visuals, better animation flow, and more cinematic moments.

Here’s a real example of how our “conversation” looked:
```shell
Animation is good and we have some nice effects, but we need to bring it to another level —
an Oscar-movie level. We need a real wow effect that makes this animation stunning.
Suggest 3–5 ideas.

● Here are 5 ideas to elevate the animation to an "Oscar-worthy" cinematic level within the CLI constraints:

  1. Saber Light Arcs (Motion Trails)
  The Concept: In the movies, fast lightsaber swings appear as solid fans of light...

  2. "Volumetric" Gradient Lighting
  The Concept: Current lighting is mostly "on/off" within a radius...
  3. Impact Shockwaves (ASCII Distortion)
  The Concept: When the sabers clash hard or a wall breaks, the impact needs to be felt. The "Wow": A post-processing effect that ...

  4. Molten Environmental Damage
  The Concept: Lightsabers melt metal. The "Wow": When the saber hits the floor or wall...

  5. Dynamic "Letterbox" Focus
  The Concept: Cinematic direction is about framing. The "Wow": During the...

  Recommendation: I recommend starting with #1 (Saber Arcs) and #4 (Molten Damage) as they add the most visual fidelity to the combat        
  itself. #3 (Shockwaves) is the best "finish" for the wall destruction sequence.

  Which one would you like me to implement first?
```

Copilot responded with ideas like motion trails for lightsabers, gradient lighting, shockwave effects, molten damage on walls, and even cinematic letterboxing. Some of them made it into the final version, others stayed on the cutting-room floor — but the **process itself was surprisingly fun**.

I was genuinely impressed. Copilot helped transform a simple ASCII-style drawing into a small but entertaining show. Sure, it’s not an Oscar-winning movie — but for a playful, slightly ridiculous idea, I think it turned out pretty cool.

Things went smoothly… until I had a **very bad idea**:
**“What if we add a camera rotation illusion?”** 😅

Yeah. That was ambitious.

This turned out to be a real challenge, even for Copilot. After many new sessions and repeatedly reworking prompts, we eventually had to pivot. A full Y-axis camera rotation just wasn’t readable with simple ASCII silhouettes, so we ended up experimenting with X-axis rotation and a top-down view instead.

At that point, the silhouettes are so minimal that the viewer has to mentally accept the perspective shift — but that’s part of the charm (and limitation) of doing cinematic nonsense in a CLI.

And honestly? That experimentation, even when it didn’t fully work was one of the most fun parts of the project.

<!-- Don't forget to add a cover image (if you want). -->

## Final Thoughts

I really enjoyed this ride. Using **GitHub Copilot CLI** for something completely **non-practical, visual, and slightly absurd** turned out to be a **great experience**. It pushed me to think differently about prompts, iteration, and collaboration with an AI — not just as a tool, but as a **creative partner**.

<!-- Thanks for participating! -->
```