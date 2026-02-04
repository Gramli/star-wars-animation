using System;
using System.Collections.Generic;

namespace StarWarsAnimation;

public class DuelSimulation
{
    public const float LogicWidth = 80f;
    public const float LogicHeight = 25f;

    public List<Vec2> Stars { get; private set; } = new();
    public List<Spark> Sparks { get; private set; } = new();
    public List<LightningBolt> LightningBolts { get; private set; } = new();
    public Actor Jedi { get; private set; }
    public Actor Sith { get; private set; }

    public bool FlashScreen { get; private set; }
    public bool ShakeScreen { get; private set; }
    public float ZoomLevel { get; private set; } = 1.0f;
    public Vec2 CameraFocus { get; private set; } = new Vec2(40, 12);
    public bool IsFinished => _phase == Phase.Exit;
    
    public List<Vec2> ScorchMarks { get; private set; } = new(); // Scars/Damage
    public List<DebrisChunk> DebrisChunks { get; private set; } = new();

    private float _time;
    private Random _rng = new(123);
    private Phase _phase = Phase.Establishment;

    private enum Phase { Establishment, FirstExchange, Escalation, WallDestruction, ForceSequence, SaberActionSequence, Climax, Resolution, FadeOut, Exit }

    public void Initialize()
    {
        for (int i = 0; i < 60; i++)
            Stars.Add(new Vec2((float)(_rng.NextDouble() * LogicWidth), (float)(_rng.NextDouble() * LogicHeight)));

        Jedi = new Actor { FX = 20, FY = 15, Color = Palette.Blue, FacingRight = true };
        Sith = new Actor { FX = 60, FY = 15, Color = Palette.Red, FacingRight = false };
        Sith.CapeTail = new Vec2(62, 15);
    }

    public void Update(float dt)
    {
        _time += dt;
        SavePrevPos(Jedi);
        SavePrevPos(Sith);

        switch (_phase)
        {
            case Phase.Establishment: UpdateEstablishment(); break;
            case Phase.FirstExchange: UpdateFirstExchange(); break;
            case Phase.Escalation: UpdateEscalation(); break;
            case Phase.WallDestruction: UpdateWallDestruction(); break;
            case Phase.ForceSequence: UpdateForceSequence(); break;
            case Phase.SaberActionSequence: UpdateSaberActionSequence(); break;
            case Phase.Climax: UpdateClimax(); break;
            case Phase.Resolution: UpdateResolution(); break;
            case Phase.FadeOut: UpdateFadeOut(); break;
        }

        SimulateCape(Sith, dt);
        UpdateDebris(dt);
        UpdateSparks(dt);
        UpdateLightning(dt);
    }

    private void UpdateLightning(float dt)
    {
        for (int i = LightningBolts.Count - 1; i >= 0; i--)
        {
            LightningBolts[i].Life -= dt;
            if (LightningBolts[i].Life <= 0) LightningBolts.RemoveAt(i);
        }
    }

    private void SavePrevPos(Actor a) { a.PrevFX = a.FX; a.PrevFY = a.FY; }

    private void UpdateEstablishment()
    {
        if (_time < 0.5f) { Jedi.PoseIndex = 0; Sith.PoseIndex = 0; }
        else if (_time < 1.0f) { Jedi.PoseIndex = 12; }
        else if (_time < 1.5f) { SetSaber(Jedi, true, 3); Jedi.PoseIndex = 0; }
        
        if (_time > 1.2f && _time < 1.7f) { Sith.PoseIndex = 12; }
        else if (_time > 1.7f && _time < 2.0f) { SetSaber(Sith, true, 3); Sith.PoseIndex = 0; }
        
        if (_time > 4.0f) _phase = Phase.FirstExchange;
    }

    private void UpdateFirstExchange()
    {
        // Smooth Approach (Walk)
        if (InRange(4.5f, 5.5f)) 
        { 
            float t = (_time - 4.5f) / 1.0f;
            Jedi.FX = Lerp(20, 32, t);
            Sith.FX = Lerp(60, 48, t);
            
            // Walk Animation (Swap legs every 0.15s)
            int walkFrame = (int)(_time * 6) % 2;
            Jedi.PoseIndex = 13 + walkFrame; 
            Sith.PoseIndex = 13 + walkFrame;
        }
        else if (InRange(5.5f, 5.8f)) 
        { 
            Jedi.FX = 32; Sith.FX = 48;
            Jedi.PoseIndex = 0; Sith.PoseIndex = 0; 
        }

        if (InRange(5.8f, 6.0f)) { SetPoses(5, 2); ZoomLevel = 1.2f; }
        else if (InRange(6.0f, 6.2f)) 
        { 
            SetPoses(1, 1); 
            SpawnSparks(40, 12, 3); 
          ShakeScreen = true;
            ZoomLevel = 1.4f; // Zoom in on impact
        } 
        else if (_time >= 6.2f) { SetPoses(0, 0); ShakeScreen = false; ZoomLevel = 1.0f; }

        // Camera Tracking
        CameraFocus = new Vec2((Jedi.FX + Sith.FX)/2, 12);

        if (_time > 8.0f) _phase = Phase.Escalation;
    }

    private void UpdateEscalation()
    {
        float t = _time - 8.0f; 

        // Beat 1: Fast Clash
        if (t > 0.2f && t < 0.5f) { SetPoses(5, 5); }
        else if (t >= 0.5f && t < 0.7f) 
        { 
            Jedi.FX = 35; Sith.FX = 45; 
            SetPoses(1, 1); 
        }
        else if (t >= 0.7f && t < 1.0f) { SpawnSparks(40, 13, 1); }

        // Beat 2: Riposte
        if (t > 1.2f && t < 1.5f) { SetPoses(2, 5); }
        else if (t >= 1.5f && t < 1.7f) 
        { 
            Jedi.FX = 33; Sith.FX = 47; 
            SetPoses(2, 1); 
        }

        // Beat 3: Heavy Clash
        if (t > 2.2f && t < 2.5f) { SetPoses(5, 5); ZoomLevel = 1.3f; }
        else if (t >= 2.5f && t < 2.7f) 
        { 
            Jedi.FX = 38; Sith.FX = 42; 
            SetPoses(1, 1); 
            SpawnSparks(40, 10, 5);
            ShakeScreen = true; 
            ZoomLevel = 1.5f; 
            
            // Permanent Scorch Mark on Floor
            if (t > 2.5f && t < 2.55f) ScorchMarks.Add(new Vec2(40, 16));
        }
        else if (t >= 2.7f && t < 3.2f)
        { 
            SetPoses(3, 3); // Brief Lock
            Jedi.FX = 36; Sith.FX = 44; 
            ShakeScreen = false;
            ZoomLevel = 1.0f;
        }

        // --- NEW EXTENDED SEQUENCE ---

        // Beat 4: Disengage & Circle (3.2 - 4.2)
        else if (t >= 3.2f && t < 4.2f)
        {
             ShakeScreen = false; ZoomLevel = 1.0f;
             float p = (t - 3.2f);
             Jedi.FX = Lerp(36, 28, p);
             Sith.FX = Lerp(44, 52, p);
             SetPoses(2, 2); // Guarding
        }

        // Beat 5: The Flurry (4.2 - 6.0)
        else if (t >= 4.2f && t < 4.5f) { SetPoses(5, 5); } // Wind up
        else if (t >= 4.5f && t < 4.7f) // High Hit
        {
             Jedi.FX = 38; Sith.FX = 42;
             SetPoses(1, 2); // Attack/Guard
             if (t < 4.55f) { SpawnSparks(40, 10, 2); ShakeScreen = true; }
        }
        else if (t >= 4.7f && t < 4.9f) // Low Hit
        {
             SetPoses(11, 1); // Crouch-Attack / Attack
             if (t < 4.75f) { SpawnSparks(40, 14, 2); }
        }
        else if (t >= 4.9f && t < 5.1f) // Mid Hit
        {
             SetPoses(1, 1);
             if (t < 4.95f) { SpawnSparks(40, 12, 2); ShakeScreen = true; }
        }
        else if (t >= 5.1f && t < 5.5f) { SetPoses(2, 2); ShakeScreen = false; } // Reset

        // Beat 6: Jump Dodge (5.5 - 7.0)
        else if (t >= 5.5f && t < 6.5f)
        {
             float jp = (t - 5.5f);
             // Sith charges
             Sith.PoseIndex = 10; 
             Sith.FX = Lerp(42, 30, jp); 
             
             // Jedi Jumps over
             Jedi.PoseIndex = 8;
             Jedi.FY = 15 - (float)Math.Sin(jp * Math.PI) * 9;
             Jedi.FX = Lerp(38, 50, jp); // Swap sides temporary
             
             // Mid-air intercept
             if (jp > 0.4f && jp < 0.6f) 
             { 
                 if (jp < 0.45f) SpawnSparks(35, 12, 3);
             }
        }
        else if (t >= 6.5f && t < 7.0f) // Land & Face
        {
             Jedi.FY = 15; Jedi.FX = 50; Sith.FX = 30;
             Jedi.FacingRight = false; Sith.FacingRight = true;
             SetPoses(11, 11);
        }

        // Beat 7: Reverse Clash (7.0 - 8.0)
        else if (t >= 7.0f && t < 7.3f) { SetPoses(5, 5); }
        else if (t >= 7.3f && t < 8.0f)
        {
             Jedi.FX = 42; Sith.FX = 38;
             SetPoses(1, 1);
             if (t < 7.35f) { SpawnSparks(40, 12, 5); ShakeScreen = true; ZoomLevel = 1.4f; }
        }

        // Beat 8: Force Push Reset (8.0 - 9.0) - Get back to correct sides
        else if (t >= 8.0f && t < 9.0f)
        {
             ShakeScreen = false; ZoomLevel = 1.0f;
             float rp = (t - 8.0f);
             
             Jedi.FX = Lerp(42, 25, rp);
             Sith.FX = Lerp(38, 55, rp);
             
             if (rp > 0.5f) { Jedi.FacingRight = true; Sith.FacingRight = false; }
             SetPoses(8, 8); // Jump back
        }

        CameraFocus = new Vec2((Jedi.FX + Sith.FX)/2, 12);

        if (_time > 17.0f) _phase = Phase.WallDestruction;
    }

    private void UpdateWallDestruction()
    {
        float t = _time - 17.0f;
        
        // 0.0 - 1.0: Preparation
        if (t < 1.0f)
        {
            SetPoses(2, 6); // Jedi Guards, Sith Pulls
            Sith.FacingRight = false; // Face left towards Jedi
            Jedi.FacingRight = true;
            
            // Wall Cracking Effect
            if (_rng.NextDouble() > 0.7) SpawnSparks(78, (int)(5 + _rng.NextDouble() * 10), 1);
            if (t > 0.5f) ShakeScreen = true;
        }
        // 1.0 - 5.0: The Barrage
        else if (t < 5.0f)
        {
            ShakeScreen = false;
            
            // Spawn Debris
            if (_rng.NextDouble() > 0.80) // Slightly more frequent
            {
                // Fix spawn height to be strictly above floor (Floor is ~15)
                float spawnY = (float)(2 + _rng.NextDouble() * 10); // 2 to 12
                char[] debrisChars = { '▞', '▟', '▙', '▀', '▄', '■', '●' };
                
                // Calculate velocity to hit Jedi
                float targetX = Jedi.FX;
                float targetY = Jedi.FY - 2; // Aim for chest
                float timeToHit = 1.0f + (float)_rng.NextDouble() * 0.5f; // Randomize speed slightly
                
                float vx = (targetX - 78) / timeToHit;
                // vy = (dy - 0.5*g*t^2) / t
                float vy = ((targetY - spawnY) - (0.5f * 10f * timeToHit * timeToHit)) / timeToHit;

                DebrisChunks.Add(new DebrisChunk 
                { 
                    Pos = new Vec2(78, spawnY),
                    Vel = new Vec2(vx, vy),
                    Active = true,
                    Char = debrisChars[_rng.Next(debrisChars.Length)]
                });
                
                // Explosion at wall source
                SpawnSparks(78, (int)spawnY, 3); 
            }

            // Jedi Defense Logic
            // Find closest debris
            float closestDist = 999f;
            Vec2 target = new Vec2(0,0);
            foreach (var d in DebrisChunks)
            {
                if (!d.Active) continue;
                float dist = d.Pos.X - Jedi.FX;
                // Look ahead for debris coming towards us
                if (dist > -2.0f && dist < closestDist)
                {
                    closestDist = dist;
                    target = d.Pos;
                }
            }

            // Expanded reaction range
            if (closestDist < 12.0f) // Incoming!
            {
                if (closestDist < 6.0f) 
                {
                    // Swing!
                    SetPoses(1, 6); // Attack
                    
                    // Visual flair: different attack poses based on height
                    if (target.Y < Jedi.FY - 3) Jedi.PoseIndex = 1; // High Swing
                    else Jedi.PoseIndex = 11; // Low/Mid Swing
                    
                    // Hold the pose slightly longer visually? 
                    // No, frame rate is high enough.
                }
                else
                {
                    SetPoses(5, 6); // Anticipate (Wind up)
                }
            }
            else
            {
                SetPoses(2, 6); // Guard
            }
        }
        // 5.0 - 6.0: Cooldown
        else if (t < 6.0f)
        {
            ShakeScreen = false;
            SetPoses(0, 0);
        }
        else
        {
            _phase = Phase.ForceSequence;
        }
    }

    private void UpdateForceSequence()
    {
        float t = _time - 23.0f; // Adjusted for WallDestruction (+6s)

        if (t < 0.5f) SetPoses(0, 6); // Sith prepares
        else if (t < 2.5f) // Lightning Phase (2 seconds)
        {
             SetPoses(2, 6); // Jedi Blocks (2), Sith Casts (6)
             
             // Generate Lightning
             if (_rng.NextDouble() > 0.3)
             {
                 var bolt = new LightningBolt { Life = 0.1f };
                 GenerateLightning(bolt, new Vec2(Sith.FX - 2, Sith.FY - 3), new Vec2(Jedi.FX + 1, Jedi.FY - 3));
                 LightningBolts.Add(bolt);
             }

             // Jedi slides back slightly
             float progress = (t - 0.5f); 
             Jedi.FX = 35 - (5 * progress);
             
             // Sparks at impact
             if (_rng.NextDouble() > 0.5) SpawnSparks((int)Jedi.FX + 2, (int)Jedi.FY - 3, 1);
        }
        else if (t < 3.5f) // Big Push / Jump Back
        {
             SetPoses(7, 6);
             float progress = (t - 2.5f); 
             Jedi.FX = 25 - (15 * progress); // Fast slide back
        }
        else if (t < 4.5f) // Jump Sequence
        {
            SetPoses(8, 0); 
            float jumpProgress = (t - 3.5f); 
            Jedi.FX = 10 + (28 * jumpProgress);
            float height = 8.0f; 
            Jedi.FY = 15 - (height * 4 * jumpProgress * (1 - jumpProgress));
        }
        else if (t < 4.8f)
        {
            Jedi.FY = 15;
            SetPoses(11, 5);
        }
        else _phase = Phase.SaberActionSequence;
    }

    private void GenerateLightning(LightningBolt bolt, Vec2 start, Vec2 end)
    {
        bolt.Points.Add(start);
        Vec2 current = start;
        Vec2 dir = new Vec2(end.X - start.X, end.Y - start.Y);
        float dist = (float)Math.Sqrt(dir.X*dir.X + dir.Y*dir.Y);
        int segments = (int)(dist / 2);
        
        for (int i = 1; i < segments; i++)
        {
            float t = (float)i / segments;
            float jitter = (float)(_rng.NextDouble() * 2.0 - 1.0);
            Vec2 p = new Vec2(
                start.X + (end.X - start.X) * t,
                start.Y + (end.Y - start.Y) * t + jitter
            );
            bolt.Points.Add(p);
        }
        bolt.Points.Add(end);
    }

    private void UpdateSaberActionSequence()
    {
        float t = _time - 28.0f; // Adjusted: 17 + 6 (Wall) + 5 (Force) = 28s

        if (t < 0.2f) { SetPoses(11, 0); }
        else if (t < 1.0f)
        {
            SetPoses(8, 0);
            float p = (t - 0.2f) / 0.8f;
            Jedi.FX = 38 + (10 * p); 
            Jedi.FY = 15 - (5.0f * 4 * p * (1 - p)); 
        }
        else if (t < 1.5f) { Jedi.FY = 15; Jedi.FX = 48; SetPoses(1, 2); } 
        else if (t < 2.0f) { SetPoses(5, 1); } 
        else if (t < 3.0f)
        {
            SetPoses(7, 3);
            float p = (t - 2.0f);
            Jedi.FX = 48 - (30 * p); 
            Sith.FX = 60 - (10 * p); 
        }
        else if (t < 3.2f) { SetPoses(11, 0); }
        else if (t < 4.5f)
        {
            SetPoses(8, 0);
            float p = (t - 3.2f) / 1.3f;
            Jedi.FX = 18 + (50 * p); 
            Jedi.FY = 15 - (12.0f * 4 * p * (1 - p)); 
            Jedi.FacingRight = p <= 0.5f;
        }
        else if (t < 5.5f)
        {
            Jedi.FX = 60; Jedi.FY = 8;
            Sith.FacingRight = true; 
            SetPoses(9, 6);
        }
        else if (t < 6.0f)
        {
            SetPoses(9, 1); 
            if (t > 5.8f) SpawnSparks(60, 8, 2); 
        }
        else if (t < 7.0f)
        {
            float p = (t - 6.0f);
            Jedi.FY = 15; 
            SetPoses(10, 5);
            Jedi.FX = 68 - (20 * p); 
        }
        else if (t < 7.5f) SetPoses(1, 5);
        else if (t < 8.0f)
        {
             Jedi.FacingRight = true;
             Sith.FacingRight = false;
             Jedi.FX = 38; Sith.FX = 42;
             SetPoses(0, 0);
        }
        else _phase = Phase.Climax;
    }

    private void UpdateClimax()
    {
        Jedi.FX = 38; Sith.FX = 42;
        SetPoses(3, 3);

        if (_time > 30.0f && _time < 30.1f) FlashScreen = true; 
        else FlashScreen = false;

        if (_time > 30.1f && _time < 33.0f && _rng.NextDouble() > 0.5)
            SpawnSparks(40, 11, 2);

        if (_time > 34.0f) _phase = Phase.Resolution;
    }

    private void UpdateResolution()
    {
        FlashScreen = false;
        ShakeScreen = false;
        ZoomLevel = 1.0f;
        
        SetPoses(0, 4); 
        SetSaber(Sith, false, 0);
        if (_time > 37.0f) _phase = Phase.FadeOut;
    }

    private void UpdateFadeOut()
    {
        if (_time > 38.0f) _phase = Phase.Exit;
    }

    private bool InRange(float min, float max) => _time >= min && _time < max;
    private float Lerp(float a, float b, float t) => a + (b - a) * Math.Clamp(t, 0, 1);
    
    private void SetPoses(int jediPose, int sithPose)
    {
        Jedi.PoseIndex = jediPose;
        Sith.PoseIndex = sithPose;
    }

    private void SetSaber(Actor a, bool active, int length)
    {
        a.SaberActive = active;
        a.SaberLength = length;
    }

    private void SimulateCape(Actor a, float dt)
    {
        float anchorX = a.FX - (a.FacingRight ? 1.0f : -1.0f); 
        float anchorY = a.FY - 3.0f;
        float velX = (a.FX - a.PrevFX) / dt;
        float velY = (a.FY - a.PrevFY) / dt;
        
        // Default hanging position (Gravity)
        float targetX = anchorX - (a.FacingRight ? 1.5f : -1.5f);
        float targetY = anchorY + 4.0f;

        // Physics: Drag
        targetX -= velX * 0.15f; // Drag against movement
        targetY -= velY * 0.15f; // Drag against jump

        // Force Action Reaction (Wind from power)
        if (a.PoseIndex == 6) // Casting Force
        {
             targetX -= (a.FacingRight ? 4.0f : -4.0f); // Blows back violently
             targetY -= 2.0f; // Lift
        }
        else if (velY < -5.0f) // Jumping up
        {
             targetY += 1.0f; // Pull down
        }

        // Smooth Damping
        a.CapeTail.X += (targetX - a.CapeTail.X) * 8.0f * dt;
        a.CapeTail.Y += (targetY - a.CapeTail.Y) * 8.0f * dt;
    }

    private void UpdateDebris(float dt)
    {
        for (int i = 0; i < DebrisChunks.Count; i++)
        {
            var d = DebrisChunks[i];
            if (!d.Active) continue;

            d.Pos.X += d.Vel.X * dt;
            d.Pos.Y += d.Vel.Y * dt;
            
            // Gravity on debris
            d.Vel.Y += 10.0f * dt;

            // Collision with Jedi Saber
            // Saber Hitbox: Jedi FX/FY.
            // When Attacking (Pose 1 or 11), blade covers area in front.
            // Let's be generous with hit detection to ensure "Cool" factor.
            if (Jedi.PoseIndex == 1 || Jedi.PoseIndex == 11) 
            {
                float dx = d.Pos.X - Jedi.FX;
                float dy = d.Pos.Y - Jedi.FY;
                
                // Hitbox: 0 to 6 in front, -5 to +5 height relative to Jedi center
                if (dx > -1.0f && dx < 7.0f && Math.Abs(dy) < 6.0f)
                {
                    d.Active = false;
                    SpawnSparks((int)d.Pos.X, (int)d.Pos.Y, 8); // MASSIVE sparks
                    ShakeScreen = true;
                }
            }

            // Floor collision
            if (d.Pos.Y > 15) 
            {
                d.Active = false;
                d.Pos.Y = 16;
                if (d.Pos.X > 0 && d.Pos.X < LogicWidth) // Only mark if on screen
                {
                    ScorchMarks.Add(d.Pos); 
                    SpawnSparks((int)d.Pos.X, (int)d.Pos.Y, 2);
                }
            }

            DebrisChunks[i] = d;
        }

        // Cleanup
        if (DebrisChunks.Count > 0 && DebrisChunks[0].Pos.X < -10) DebrisChunks.RemoveAt(0);
    }

    private void UpdateSparks(float dt)
    {
        for (int i = Sparks.Count - 1; i >= 0; i--)
        {
            var s = Sparks[i];
            s.Pos.X += s.Vel.X * dt * 20;
            s.Pos.Y += s.Vel.Y * dt * 10;
            s.Vel.Y += 20f * dt; 
            s.Life -= dt;
            Sparks[i] = s;

            if (s.Life <= 0) Sparks.RemoveAt(i);
        }
    }

    private void SpawnSparks(int x, int y, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (float)(_rng.NextDouble() * Math.PI * 2);
            float speed = (float)(_rng.NextDouble() * 1.5 + 0.5);
            Sparks.Add(new Spark
            {
                Pos = new Vec2(x, y),
                Vel = new Vec2((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed - 1.0f),
                Life = (float)(_rng.NextDouble() * 0.3 + 0.1)
            });
        }
    }
}
