using System;
using System.Diagnostics;
using System.Threading;

namespace StarWarsAnimation;

class Program
{
    const int TargetFps = 20;
    const float FrameTime = 1.0f / TargetFps;

    static void Main(string[] args)
    {
        Console.Title = "Duel of the CLI";
        
        var simulation = new DuelSimulation();
        simulation.Initialize();

        var renderer = new SceneRenderer();

        var stopwatch = Stopwatch.StartNew();
        var prevTime = stopwatch.Elapsed.TotalSeconds;

        try
        {
            while (!simulation.IsFinished)
            {
                var now = stopwatch.Elapsed.TotalSeconds;
                var dt = (float)(now - prevTime);

                if (dt >= FrameTime)
                {
                    prevTime = now;
                    simulation.Update(dt);
                    renderer.Render(simulation);
                }
                else
                {
                    Thread.Sleep(5);
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