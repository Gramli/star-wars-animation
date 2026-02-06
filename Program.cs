using StarWarsAnimation.Rendering;
using StarWarsAnimation.Simulation;
using System.Diagnostics;

namespace StarWarsAnimation;

class Program
{
    const int TargetFps = 20;
    const float FrameTime = 1.0f / TargetFps;

    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Title = "🎬 Duel of the CLI";
        
        var simulation = new DuelSimulation();
        simulation.Initialize();

        var renderer = new SceneRenderer();

        var stopwatch = Stopwatch.StartNew();
        var prevTime = stopwatch.Elapsed.TotalSeconds;
        double accumulator = 0.0;

        try
        {
            while (!simulation.IsFinished)
            {
                var now = stopwatch.Elapsed.TotalSeconds;
                var frameTime = now - prevTime;
                prevTime = now;
                
                accumulator += frameTime;

                // Input handling
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.Spacebar)
                    {
                        simulation.SkipPhase();
                    }
                }

                bool updated = false;
                while (accumulator >= FrameTime)
                {
                    simulation.Update(FrameTime);
                    accumulator -= FrameTime;
                    updated = true;
                }

                if (updated)
                {
                    renderer.Render(simulation);
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }
        finally
        {
            Console.ResetColor();
            Console.Clear();
            Console.CursorVisible = true;
        }
    }
}