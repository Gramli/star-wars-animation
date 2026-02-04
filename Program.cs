using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace StarWarsAnimation;

class Program
{
    // Screen dimensions
    const int Width = 80;
    const int Height = 25; // Standard terminal height
    const int TargetFps = 20;
    const int FrameTimeMs = 1000 / TargetFps;

    // Colors (ANSI)
    const string ColorReset = "\u001b[0m";
    const string ColorBlack = "\u001b[30m";
    const string ColorRed = "\u001b[91m";   // Bright Red for Sith
    const string ColorBlue = "\u001b[94m";  // Bright Blue for Jedi
    const string ColorWhite = "\u001b[97m"; // Bright White for Core/Flash
    const string ColorYellow = "\u001b[93m"; // Sparks
    const string ColorDarkGray = "\u001b[90m"; // Background stars
    const string BgBlack = "\u001b[40m";

    // Characters
    struct Pixel
    {
        public char Char;
        public string Color;
    }

    static Pixel[,] _buffer = new Pixel[Height, Width];
    static Pixel[,] _prevBuffer = new Pixel[Height, Width];
    static List<(int x, int y)> _stars = new();
    static Random _rng = new Random(123); // Deterministic seed

    // Animation State
    enum Phase { Establishment, FirstExchange, Escalation, Climax, Resolution, FadeOut, Exit }
    static Phase _currentPhase = Phase.Establishment;
    static float _time = 0f;

    // Actors
    class Actor
    {
        public int X, Y;
        public string Color;
        public bool SaberActive;
        public int SaberLength; // 0 to 3
        public int PoseIndex;
        public bool FacingRight;
    }

    static Actor _jedi = new Actor { X = 20, Y = 15, Color = ColorBlue, FacingRight = true };
    static Actor _sith = new Actor { X = 60, Y = 15, Color = ColorRed, FacingRight = false };

    // Effects
    struct Spark { public float X, Y; public float VX, VY; public float Life; }
    static List<Spark> _sparks = new();
    static bool _flashScreen = false;

    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = false;
        Console.Title = "Duel of the CLI";

        // Initialize Stars
        for (int i = 0; i < 40; i++)
        {
            _stars.Add((_rng.Next(Width), _rng.Next(Height)));
        }

        Stopwatch sw = Stopwatch.StartNew();
        long previousTime = sw.ElapsedMilliseconds;

        try
        {
            while (_currentPhase != Phase.Exit)
            {
                long currentTime = sw.ElapsedMilliseconds;
                long deltaTimeMs = currentTime - previousTime;

                if (deltaTimeMs >= FrameTimeMs)
                {
                    float dt = deltaTimeMs / 1000.0f;
                    previousTime = currentTime;

                    Update(dt);
                    Render();
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

    static void Update(float dt)
    {
        _time += dt;

        // Choreography Timeline
        // 0-4s: Establishment
        // 4-8s: First Exchange
        // 8-13s: Escalation
        // 13-17s: Climax
        // 17-20s: Resolution
        
        switch (_currentPhase)
        {
            case Phase.Establishment:
                if (_time < 1.0f) { /* Wait */ }
                else if (_time < 1.5f) { _jedi.SaberActive = true; _jedi.SaberLength = 3; } // Ignite
                else if (_time < 2.0f) { _sith.SaberActive = true; _sith.SaberLength = 3; } // Ignite
                
                if (_time > 4.0f) _currentPhase = Phase.FirstExchange;
                break;

            case Phase.FirstExchange:
                // Move closer
                if (_time > 4.5f && _time < 5.0f) { _jedi.X = 25; _sith.X = 55; }
                if (_time > 5.5f && _time < 6.0f) { _jedi.X = 30; _sith.X = 50; }
                
                // First Clash
                if (_time > 6.0f && _time < 6.2f) { _jedi.PoseIndex = 1; _sith.PoseIndex = 1; SpawnSparks(40, 12, 3); }
                else if (_time > 6.2f) { _jedi.PoseIndex = 0; _sith.PoseIndex = 0; } // Reset

                if (_time > 8.0f) _currentPhase = Phase.Escalation;
                break;

            case Phase.Escalation:
                // Faster beats
                float localTime = _time - 8.0f;
                // Beat 1
                if (localTime > 0.5f && localTime < 0.7f) { _jedi.X = 32; _sith.X = 48; _jedi.PoseIndex = 2; _sith.PoseIndex = 2; }
                else if (localTime > 0.7f && localTime < 1.0f) { SpawnSparks(40, 13, 1); } // Clash
                
                // Beat 2
                if (localTime > 1.5f && localTime < 1.7f) { _jedi.X = 28; _sith.X = 52; _jedi.PoseIndex = 1; _sith.PoseIndex = 0; }
                
                // Beat 3 (Big move)
                if (localTime > 2.5f && localTime < 2.7f) { _jedi.X = 38; _sith.X = 42; _jedi.PoseIndex = 2; _sith.PoseIndex = 2; SpawnSparks(40, 10, 5); }
                else if (localTime > 2.7f) { _jedi.X = 35; _sith.X = 45; _jedi.PoseIndex = 0; _sith.PoseIndex = 0; }

                if (_time > 13.0f) _currentPhase = Phase.Climax;
                break;

            case Phase.Climax:
                // Lock sabers
                _jedi.X = 38; _sith.X = 42;
                _jedi.PoseIndex = 3; _sith.PoseIndex = 3;
                
                // Flash at impact
                if (_time > 13.0f && _time < 13.1f) { _flashScreen = true; }
                else { _flashScreen = false; }

                // Intense sparks during lock
                if (_time > 13.1f && _time < 15.0f)
                {
                    if (_rng.NextDouble() > 0.5) SpawnSparks(40, 11, 2);
                }

                if (_time > 16.0f) _currentPhase = Phase.Resolution;
                break;

            case Phase.Resolution:
                _flashScreen = false;
                _jedi.PoseIndex = 0; // Stand tall
                _sith.PoseIndex = 4; // Kneel/Fall
                _sith.SaberActive = false; // Drop saber

                if (_time > 19.0f) _currentPhase = Phase.FadeOut;
                break;

            case Phase.FadeOut:
                // Fade logic handled in render by darkening everything or just exit
                if (_time > 20.0f) _currentPhase = Phase.Exit;
                break;
        }

        // Update Sparks
        for (int i = _sparks.Count - 1; i >= 0; i--)
        {
            var s = _sparks[i];
            s.X += s.VX * dt * 20; // Scale speed
            s.Y += s.VY * dt * 10; // Gravity/Motion
            s.VY += 20f * dt; // Gravity
            s.Life -= dt;
            _sparks[i] = s;
            if (s.Life <= 0) _sparks.RemoveAt(i);
        }
    }

    static void SpawnSparks(int x, int y, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (float)(_rng.NextDouble() * Math.PI * 2);
            float speed = (float)(_rng.NextDouble() * 1.5 + 0.5);
            _sparks.Add(new Spark
            {
                X = x, Y = y,
                VX = (float)Math.Cos(angle) * speed,
                VY = (float)Math.Sin(angle) * speed - 1.0f, // Upward bias
                Life = (float)(_rng.NextDouble() * 0.3 + 0.1)
            });
        }
    }

    static void Render()
    {
        // 1. Clear Buffer
        ClearBuffer();

        // 2. Draw Background (Stars)
        if (!_flashScreen)
        {
            foreach (var (sx, sy) in _stars)
            {
                if (sx >= 0 && sx < Width && sy >= 0 && sy < Height)
                    DrawChar(sx, sy, '.', ColorDarkGray);
            }
        }

        // 3. Draw Characters
        if (!_flashScreen)
        {
            DrawActor(_jedi);
            DrawActor(_sith);
        }

        // 4. Draw Effects
        foreach (var s in _sparks)
        {
            if (s.X >= 0 && s.X < Width && s.Y >= 0 && s.Y < Height)
                DrawChar((int)s.X, (int)s.Y, '*', ColorYellow);
        }

        if (_flashScreen)
        {
            FillBuffer('█', ColorWhite);
        }

        // 5. Output to Console (Double Buffer optimization)
        DrawBufferToConsole();
    }

    static void DrawActor(Actor actor)
    {
        int x = actor.X;
        int y = actor.Y; // Feet position
        string c = ColorReset; // Body color (silhouette is usually standard or dark, but prompt says readable silhouette)
        // Actually prompt says "identifiable even if rendered in a single color". 
        // We will use standard color for body, specific color for saber.
        
        // Simple Body
        // Head
        DrawChar(x, y - 4, 'O', ColorWhite); 
        // Torso
        DrawChar(x, y - 3, '|', ColorWhite);
        DrawChar(x, y - 2, '|', ColorWhite);
        
        // Pose Logic
        if (actor.PoseIndex == 4) // Kneel/Fall
        {
            // Override for fallen state
             DrawChar(x, y - 2, ' ', ColorReset); // Clear standing torso
             DrawChar(x, y - 3, ' ', ColorReset);
             DrawChar(x, y - 4, ' ', ColorReset);

             // Lowered head
             DrawChar(x + (actor.FacingRight ? 1 : -1), y - 1, 'o', ColorWhite);
             // Body
             DrawChar(x, y-1, '_', ColorWhite);
             return; 
        }

        // Arms
        if (actor.PoseIndex == 0) // Idle
        {
            DrawChar(x - 1, y - 3, '/', ColorWhite);
            DrawChar(x + 1, y - 3, '\\', ColorWhite);
        }
        else if (actor.PoseIndex == 1) // Attack High
        {
            if (actor.FacingRight) DrawChar(x + 1, y - 3, '/', ColorWhite);
            else DrawChar(x - 1, y - 3, '\\', ColorWhite);
        }
        else if (actor.PoseIndex == 2) // Parry/Guard
        {
            if (actor.FacingRight) DrawChar(x + 1, y - 3, '-', ColorWhite);
            else DrawChar(x - 1, y - 3, '-', ColorWhite);
        }
        else if (actor.PoseIndex == 3) // Lock
        {
             if (actor.FacingRight) DrawChar(x + 1, y - 3, '>', ColorWhite);
             else DrawChar(x - 1, y - 3, '<', ColorWhite);
        }

        // Legs
        DrawChar(x - 1, y - 1, '/', ColorWhite);
        DrawChar(x + 1, y - 1, '\\', ColorWhite);


        // Saber
        if (actor.SaberActive)
        {
            int sx = x + (actor.FacingRight ? 2 : -2);
            int sy = y - 3; // Hand height

            // Draw Handle
            // DrawChar(sx - (actor.FacingRight ? 1 : -1), sy, '-', ColorWhite);

            // Blade direction based on pose
            int dx = 0, dy = 0;
            char bladeChar = '|';

            if (actor.PoseIndex == 0) { dx = 0; dy = -1; bladeChar = '|'; } // Up
            if (actor.PoseIndex == 1) { dx = (actor.FacingRight ? 1 : -1); dy = -1; bladeChar = '/'; if(!actor.FacingRight) bladeChar = '\\'; } // Diagonal Strike
            if (actor.PoseIndex == 2) { dx = (actor.FacingRight ? 1 : -1); dy = -1; bladeChar = '/'; if(!actor.FacingRight) bladeChar = '\\'; } 
            if (actor.PoseIndex == 3) { dx = (actor.FacingRight ? 1 : -1); dy = 0; bladeChar = '-'; } // Horizontal Lock

            for (int k = 1; k <= actor.SaberLength; k++)
            {
                DrawChar(sx + (dx * k), sy + (dy * k), bladeChar, actor.Color);
            }
        }
    }

    static void ClearBuffer()
    {
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                _buffer[y, x].Char = ' ';
                _buffer[y, x].Color = ColorBlack;
            }
    }

    static void FillBuffer(char c, string color)
    {
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                _buffer[y, x].Char = c;
                _buffer[y, x].Color = color;
            }
    }

    static void DrawChar(int x, int y, char c, string color)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            _buffer[y, x].Char = c;
            _buffer[y, x].Color = color;
        }
    }

    static void DrawBufferToConsole()
    {
        StringBuilder sb = new StringBuilder();
        // Reset cursor to top-left
        sb.Append("\u001b[H"); 

        string lastColor = "";

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var p = _buffer[y, x];
                var prev = _prevBuffer[y, x];

                // Optimization: In a real terminal, we could skip overwriting if identical.
                // But for simplicity and to prevent tearing/color bleed issues with ANSI, 
                // we'll just redraw the whole buffer line by line or character by character.
                // However, building a huge string is faster than Console.Write per char.
                
                if (p.Color != lastColor)
                {
                    sb.Append(p.Color);
                    lastColor = p.Color;
                }
                sb.Append(p.Char);

                // Update prev buffer
                _prevBuffer[y, x] = p;
            }
            sb.Append("\n");
        }
        sb.Append(ColorReset); // Reset at end of frame
        Console.Write(sb.ToString());
    }
}
