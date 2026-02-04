using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace StarWarsAnimation;

// --- Core Data Structures ---

struct Vec2
{
    public float X, Y;
    public Vec2(float x, float y) { X = x; Y = y; }
}

struct Spark
{
    public Vec2 Pos;
    public Vec2 Vel;
    public float Life;
}

class Actor
{
    public int X, Y;
    public string Color = "";
    public bool SaberActive;
    public int SaberLength;
    public int PoseIndex;
    public bool FacingRight;
}

// --- Main Program ---

class Program
{
    // Configuration
    const int TargetFps = 20;
    const float FrameTime = 1.0f / TargetFps;

    // Palette
    const string ClrReset = "\u001b[0m";
    const string ClrRed = "\u001b[91m";
    const string ClrBlue = "\u001b[94m";
    const string ClrWhite = "\u001b[97m";
    const string ClrYellow = "\u001b[93m";
    const string ClrDim = "\u001b[90m";

    // Global State
    static TerminalRenderer _renderer = new();
    static Random _rng = new(123);
    static List<Vec2> _stars = new();
    static List<Spark> _sparks = new();
    static Actor _jedi = new();
    static Actor _sith = new();
    
    // Timeline
    enum Phase { Establishment, FirstExchange, Escalation, ForceSequence, SaberActionSequence, Climax, Resolution, FadeOut, Exit }
    static Phase _phase = Phase.Establishment;
    static float _time = 0f;
    static bool _flashScreen = false;

    static void Main(string[] args)
    {
        Console.Title = "Duel of the CLI";
        InitializeScene();

        var stopwatch = Stopwatch.StartNew();
        var prevTime = stopwatch.Elapsed.TotalSeconds;

        try
        {
            while (_phase != Phase.Exit)
            {
                var now = stopwatch.Elapsed.TotalSeconds;
                var dt = (float)(now - prevTime);

                if (dt >= FrameTime)
                {
                    prevTime = now;
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

    static void InitializeScene()
    {
        // Stars
        for (int i = 0; i < 40; i++)
            _stars.Add(new Vec2(_rng.Next(TerminalRenderer.Width), _rng.Next(TerminalRenderer.Height)));

        // Actors
        _jedi = new Actor { X = 20, Y = 15, Color = ClrBlue, FacingRight = true };
        _sith = new Actor { X = 60, Y = 15, Color = ClrRed, FacingRight = false };
    }

    // --- Update Logic ---

    static void Update(float dt)
    {
        _time += dt;

        switch (_phase)
        {
            case Phase.Establishment:       UpdateEstablishment(); break;
            case Phase.FirstExchange:       UpdateFirstExchange(); break;
            case Phase.Escalation:          UpdateEscalation(); break;
            case Phase.ForceSequence:       UpdateForceSequence(); break;
            case Phase.SaberActionSequence: UpdateSaberActionSequence(); break;
            case Phase.Climax:              UpdateClimax(); break;
            case Phase.Resolution:          UpdateResolution(); break;
            case Phase.FadeOut:             UpdateFadeOut(); break;
        }

        UpdateSparks(dt);
    }

    static void UpdateEstablishment()
    {
        if (_time < 1.0f) return;
        
        if (_time < 1.5f) { SetSaber(_jedi, true, 3); _jedi.PoseIndex = 0; }
        else if (_time < 2.0f) { SetSaber(_sith, true, 3); _sith.PoseIndex = 0; }
        
        if (_time > 4.0f) _phase = Phase.FirstExchange;
    }

    static void UpdateFirstExchange()
    {
        // Approach
        if (InRange(4.5f, 5.0f)) { _jedi.X = 25; _sith.X = 55; }
        if (InRange(5.5f, 6.0f)) { _jedi.X = 30; _sith.X = 50; }

        // Exchange
        if (InRange(5.8f, 6.0f)) { SetPoses(5, 2); } // Anticipate
        else if (InRange(6.0f, 6.2f)) // Clash
        { 
            SetPoses(1, 1); 
            SpawnSparks(40, 12, 3); 
        } 
        else if (_time >= 6.2f) { SetPoses(0, 0); } // Recover

        if (_time > 8.0f) _phase = Phase.Escalation;
    }

    static void UpdateEscalation()
    {
        float t = _time - 8.0f; // Local phase time

        // Beat 1: Fast Clash
        if (t > 0.3f && t < 0.5f) { SetPoses(5, 5); }
        else if (t >= 0.5f && t < 0.7f) 
        { 
            _jedi.X = 32; _sith.X = 48; 
            SetPoses(1, 1); 
        }
        else if (t >= 0.7f && t < 1.0f) { SpawnSparks(40, 13, 1); }

        // Beat 2: Riposte
        if (t > 1.3f && t < 1.5f) { SetPoses(2, 5); }
        else if (t >= 1.5f && t < 1.7f) 
        { 
            _jedi.X = 28; _sith.X = 52; 
            SetPoses(2, 1); 
        }

        // Beat 3: Heavy Clash
        if (t > 2.3f && t < 2.5f) { SetPoses(5, 5); }
        else if (t >= 2.5f && t < 2.7f) 
        { 
            _jedi.X = 38; _sith.X = 42; 
            SetPoses(1, 1); 
            SpawnSparks(40, 10, 5); 
        }
        else if (t >= 2.7f) 
        { 
            _jedi.X = 35; _sith.X = 45; 
            SetPoses(0, 0); 
        }

        if (_time > 13.0f) _phase = Phase.ForceSequence;
    }

    static void UpdateForceSequence()
    {
        float t = _time - 13.0f; // Local time starts at 0

        // 1. Sith Prepare (0.0 - 0.5)
        if (t < 0.5f) 
        {
            SetPoses(0, 6); // Jedi Idle, Sith Force Pose
        }
        // 2. Force Push (0.5 - 1.5)
        else if (t < 1.5f)
        {
             SetPoses(7, 6); // Jedi Stagger, Sith Force
             // Slide Jedi back from 35 to ~10 (25 chars)
             float progress = (t - 0.5f); // 0 to 1
             _jedi.X = 35 - (int)(25 * progress);
        }
        // 3. Stagger/Pause (1.5 - 2.5)
        else if (t < 2.5f)
        {
            SetPoses(7, 0); // Jedi Stagger, Sith drops arm
        }
        // 4. Jump Back (2.5 - 3.5)
        else if (t < 3.5f)
        {
            SetPoses(8, 0); // Jedi Jump, Sith Idle
            // Parabolic arc for Jump
            float jumpProgress = (t - 2.5f); // 0 to 1
            
            // Move X from ~10 back to 38 (Target for climax)
            _jedi.X = 10 + (int)(28 * jumpProgress);
            
            // Arc Y (Base is 15)
            // 4 * height * x * (1-x)
            float height = 8.0f; 
            _jedi.Y = 15 - (int)(height * 4 * jumpProgress * (1 - jumpProgress));
        }
        // 5. Land (3.5 - 4.0)
        else if (t < 4.0f)
        {
            _jedi.Y = 15;
            SetPoses(5, 5); // Anticipation for Climax
        }
        else
        {
            _phase = Phase.SaberActionSequence;
        }
    }

    static void UpdateSaberActionSequence()
    {
        float t = _time - 17.0f; // Start at 17s

        // 1. Jedi Jump Toward Sith (0.0 - 1.0)
        if (t < 1.0f)
        {
            SetPoses(8, 0);
            float p = t / 1.0f;
            _jedi.X = 38 + (int)(10 * p); // 38 -> 48
            _jedi.Y = 15 - (int)(5.0f * 4 * p * (1 - p)); // Hop
        }
        // 2. Parry and Dodge (1.0 - 2.0)
        else if (t < 1.5f) { _jedi.Y = 15; _jedi.X = 48; SetPoses(1, 2); } // Jedi Atk, Sith Guard
        else if (t < 2.0f) { SetPoses(5, 1); } // Jedi Dodge, Sith Atk
        
        // 3. Sith Pushes Jedi (2.0 - 3.0)
        else if (t < 3.0f)
        {
            SetPoses(7, 3); // Jedi Stagger, Sith Lock/Push
            float p = (t - 2.0f);
            _jedi.X = 48 - (int)(30 * p); // Push left to ~18
            _sith.X = 60 - (int)(10 * p); // Sith advances to 50
        }
        // 4. Jedi Jump Over Sith (3.0 - 4.5)
        else if (t < 4.5f)
        {
            SetPoses(8, 0);
            float p = (t - 3.0f) / 1.5f;
            _jedi.X = 18 + (int)(50 * p); // 18 -> 68 (Right side)
            _jedi.Y = 15 - (int)(12.0f * 4 * p * (1 - p)); // High jump
            _jedi.FacingRight = false; // Turn mid air?
            if (p > 0.5f) _jedi.FacingRight = false;
        }
        // 5. Sith Stops Jedi Mid-air (4.5 - 5.5)
        else if (t < 5.5f)
        {
            // Freeze Jedi mid-air
            _jedi.X = 60; // Suspended above/near Sith
            _jedi.Y = 8;
            _sith.FacingRight = true; // Face Jedi
            SetPoses(9, 6); // Jedi Suspended, Sith Force
        }
        // 6. Sith Slashes (5.5 - 6.0)
        else if (t < 6.0f)
        {
            SetPoses(9, 1); // Jedi Suspended, Sith Slash
            if (t > 5.8f) SpawnSparks(60, 8, 2); // Hit?
        }
        // 7. Jedi Rebounds/Attacks (6.0 - 7.0)
        else if (t < 7.0f)
        {
            float p = (t - 6.0f);
            _jedi.Y = 15; // Landed
            SetPoses(10, 5); // Jedi Dash, Sith Anticipate
            _jedi.X = 68 - (int)(20 * p); // Dash Left
        }
        // 8. Sith Dodges (7.0 - 7.5)
        else if (t < 7.5f)
        {
             SetPoses(1, 5); // Jedi Atk, Sith Lean back
        }
        // 9. Finish / Reset (7.5 - 8.0)
        else if (t < 8.0f)
        {
             _jedi.FacingRight = true;
             _sith.FacingRight = false;
             _jedi.X = 38; _sith.X = 42;
             SetPoses(0, 0);
        }
        else
        {
            _phase = Phase.Climax;
        }
    }

    static void UpdateClimax()
    {
        // Now starts at 17 + 8 = 25s
        
        _jedi.X = 38; _sith.X = 42;
        SetPoses(3, 3); // Lock

        // Flash Impact
        if (_time > 25.0f && _time < 25.1f) { _flashScreen = true; }
        else { _flashScreen = false; }

        // Lock Sparks
        if (_time > 25.1f && _time < 28.0f && _rng.NextDouble() > 0.5)
            SpawnSparks(40, 11, 2);

        if (_time > 29.0f) _phase = Phase.Resolution;
    }

    static void UpdateResolution()
    {
        _flashScreen = false;
        SetPoses(0, 4); // Jedi Stand, Sith Fall
        SetSaber(_sith, false, 0);

        if (_time > 32.0f) _phase = Phase.FadeOut;
    }

    static void UpdateFadeOut()
    {
        if (_time > 33.0f) _phase = Phase.Exit;
    }

    // --- Helpers ---

    static bool InRange(float min, float max) => _time >= min && _time < max;
    
    static void SetPoses(int jediPose, int sithPose)
    {
        _jedi.PoseIndex = jediPose;
        _sith.PoseIndex = sithPose;
    }

    static void SetSaber(Actor a, bool active, int length)
    {
        a.SaberActive = active;
        a.SaberLength = length;
    }

    static void UpdateSparks(float dt)
    {
        for (int i = _sparks.Count - 1; i >= 0; i--)
        {
            var s = _sparks[i];
            s.Pos.X += s.Vel.X * dt * 20;
            s.Pos.Y += s.Vel.Y * dt * 10;
            s.Vel.Y += 20f * dt; // Gravity
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
                Pos = new Vec2(x, y),
                Vel = new Vec2((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed - 1.0f),
                Life = (float)(_rng.NextDouble() * 0.3 + 0.1)
            });
        }
    }

    // --- Rendering ---

    static void Render()
    {
        _renderer.Clear();

        if (_flashScreen)
        {
            _renderer.Fill('█', ClrWhite);
        }
        else
        {
            RenderScene();
        }

        _renderer.Present();
    }

    static void RenderScene()
    {
        foreach (var s in _stars)
            _renderer.Draw((int)s.X, (int)s.Y, '.', ClrDim);

        DrawActor(_jedi);
        DrawActor(_sith);

        foreach (var s in _sparks)
            _renderer.Draw((int)s.Pos.X, (int)s.Pos.Y, '*', ClrYellow);
    }

    static void DrawActor(Actor actor)
    {
        int x = actor.X;
        int y = actor.Y;
        int dir = actor.FacingRight ? 1 : -1;
        
        // Pose Definitions (Relative offsets)
        // [Index] -> (Head, TorsoTop, TorsoBot, LegL, LegR, ArmL, ArmR, Hand, Blade)
        // This keeps logic separated from data
        
        // Default (Idle)
        var head = (dx: 0, dy: -4, c: 'O');
        var torsoT = (dx: 0, dy: -3, c: '|');
        var torsoB = (dx: 0, dy: -2, c: '|');
        var legL = (dx: -1, dy: -1, c: '/');
        var legR = (dx: 1, dy: -1, c: '\\');
        var armL = (dx: -1, dy: -3, c: '/');
        var armR = (dx: 1, dy: -3, c: '\\');
        var hand = (dx: 1, dy: -3);
        var blade = (dx: 0, dy: -1, c: '|');

        switch (actor.PoseIndex)
        {
            case 1: // Attack
                head = (2, -4, 'O'); torsoT = (1, -3, '|');
                legL = (-2, -1, '/'); legR = (2, -1, '\\');
                armL = (-1, -3, '\\'); armR = (2, -4, '/');
                hand = (2, -4); blade = (1, -1, '/');
                break;
            case 2: // Guard
                head = (-1, -4, 'O'); torsoT = (-1, -3, '|');
                legL = (-2, -1, '/'); legR = (1, -1, '\\');
                armR = (0, -3, '|');
                hand = (0, -3); blade = (-1, -1, '\\');
                break;
            case 3: // Lock
                head = (1, -4, 'O'); torsoT = (1, -3, '|');
                legL = (-2, -1, '/'); legR = (2, -1, '\\');
                armR = (1, -3, '-');
                hand = (1, -3); blade = (1, 0, '-');
                break;
            case 4: // Kneel
                head = (1, -2, 'o'); torsoT = (0, -1, '_'); torsoB = (0, 0, ' ');
                legL = (0, -1, '_'); legR = (2, -1, '_');
                armL = (0,0,' '); armR = (0,0,' '); // Hide arms
                break;
            case 5: // Anticipation
                head = (-1, -4, 'O'); torsoT = (-1, -3, '|');
                armR = (-2, -4, '\\');
                hand = (-2, -4); blade = (-1, -1, '\\');
                break;
            case 6: // Force Push (Sith)
                // ArmL raised forward (Force), ArmR (Saber) down/back
                head = (0, -4, 'O'); torsoT = (0, -3, '|');
                armL = (2, -3, '-'); // Force Hand
                armR = (-1, -2, '\\'); // Saber Hand Low
                hand = (-1, -2); blade = (1, 1, '\\'); // Pointing down/forward
                break;
            case 7: // Stagger (Jedi)
                head = (-2, -4, 'O'); torsoT = (-1, -3, '\\');
                legL = (-2, -1, '/'); legR = (1, -1, '/'); // Leaning back
                armL = (-2, -3, '/'); armR = (0, -3, '\\');
                hand = (0, -3); blade = (-1, -1, '\\'); // Guarding loosely
                break;
            case 8: // Jump (Jedi)
                head = (1, -4, 'O'); torsoT = (0, -3, '|');
                legL = (-1, -2, '-'); legR = (1, -2, '-'); // Tucked legs
                armL = (-1, -3, '/'); armR = (1, -3, '\\');
                hand = (1, -3); blade = (-1, -1, '/');
                break;
            case 9: // Suspended (Jedi)
                head = (0, -4, 'O'); torsoT = (0, -3, '|');
                legL = (-1, -1, '|'); legR = (1, -1, '|'); // Dangling
                armL = (-2, -3, '\\'); armR = (2, -3, '/'); // Flailing
                hand = (2, -3); blade = (0, -1, '|');
                break;
            case 10: // Dash/Rebound (Jedi)
                head = (3, -3, 'O'); torsoT = (2, -2, '-'); torsoB = (0, -2, '-'); // Horizontal
                legL = (-2, -2, '='); legR = (-1, -2, '='); // Flying
                armL = (0, -2, '-'); armR = (3, -2, '-'); // Thrusting
                hand = (3, -2); blade = (1, 0, '-');
                break;
        }

        if (actor.PoseIndex == 4) // Kneel special draw
        {
             _renderer.Draw(x + (head.dx * dir), y + head.dy, head.c, ClrWhite);
             _renderer.Draw(x, y - 1, '_', ClrWhite);
             return;
        }

        // Draw Body
        DrawPart(x, y, dir, legL);
        DrawPart(x, y, dir, legR);
        DrawPart(x, y, dir, torsoB);
        DrawPart(x, y, dir, torsoT);
        DrawPart(x, y, dir, armL);
        if (actor.PoseIndex != 3) DrawPart(x, y, dir, armR);

        _renderer.Draw(x + (head.dx * dir), y + head.dy, head.c, ClrWhite);

        // Draw Saber
        if (actor.SaberActive)
        {
            int sx = x + (hand.dx * dir);
            int sy = y + hand.dy;
            
            // Flip blade char
            char bChar = blade.c;
            if (dir == -1)
            {
                if (bChar == '/') bChar = '\\';
                else if (bChar == '\\') bChar = '/';
            }

            for (int k = 1; k <= actor.SaberLength; k++)
            {
                _renderer.Draw(sx + (blade.dx * dir * k), sy + (blade.dy * k), bChar, actor.Color);
            }
        }
    }

    static void DrawPart(int cx, int cy, int dir, (int dx, int dy, char c) part)
    {
        _renderer.Draw(cx + (part.dx * dir), cy + part.dy, part.c, ClrWhite);
    }
}
