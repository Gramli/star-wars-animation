using System;
using System.Collections.Generic;

namespace StarWarsAnimation;

public class DuelSimulation
{
    public const float LogicWidth = 80f;
    public const float LogicHeight = 25f;

    public List<Vec2> Stars { get; private set; } = new();
    public List<Spark> Sparks { get; private set; } = new();
    public Actor Jedi { get; private set; }
    public Actor Sith { get; private set; }

    public bool FlashScreen { get; private set; }
    public bool IsFinished => _phase == Phase.Exit;

    private float _time;
    private Random _rng = new(123);
    private Phase _phase = Phase.Establishment;

    private enum Phase { Establishment, FirstExchange, Escalation, ForceSequence, SaberActionSequence, Climax, Resolution, FadeOut, Exit }

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
            case Phase.ForceSequence: UpdateForceSequence(); break;
            case Phase.SaberActionSequence: UpdateSaberActionSequence(); break;
            case Phase.Climax: UpdateClimax(); break;
            case Phase.Resolution: UpdateResolution(); break;
            case Phase.FadeOut: UpdateFadeOut(); break;
        }

        SimulateCape(Sith, dt);
        UpdateSparks(dt);
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
        // Smooth Approach with small hop/step
        if (InRange(4.5f, 4.7f)) { Jedi.PoseIndex = 11; Sith.PoseIndex = 11; } // Crouch/Prep
        else if (InRange(4.7f, 5.0f)) 
        { 
            Jedi.FX = Lerp(20, 32, (5.0f - 4.7f)); // Move closer (was 25)
            Jedi.FX = 32; Sith.FX = 48; // Set close engagement distance (was 25, 55)
            Jedi.PoseIndex = 0; Sith.PoseIndex = 0;
        }
        
        if (InRange(5.4f, 5.6f)) { Jedi.PoseIndex = 11; Sith.PoseIndex = 11; }
        else if (InRange(5.6f, 6.0f)) { Jedi.FX = 35; Sith.FX = 45; } // Even closer for clash (was 30, 50)

        if (InRange(5.8f, 6.0f)) { SetPoses(5, 2); }
        else if (InRange(6.0f, 6.2f)) 
        { 
            SetPoses(1, 1); 
            SpawnSparks(40, 12, 3); 
        } 
        else if (_time >= 6.2f) { SetPoses(0, 0); }

        if (_time > 8.0f) _phase = Phase.Escalation;
    }

    private void UpdateEscalation()
    {
        float t = _time - 8.0f; 

        // Beat 1: Fast Clash
        if (t > 0.2f && t < 0.5f) { SetPoses(5, 5); }
        else if (t >= 0.5f && t < 0.7f) 
        { 
            Jedi.FX = 35; Sith.FX = 45; // Close distance (was 32, 48)
            SetPoses(1, 1); 
        }
        else if (t >= 0.7f && t < 1.0f) { SpawnSparks(40, 13, 1); }

        // Beat 2: Riposte
        if (t > 1.2f && t < 1.5f) { SetPoses(2, 5); }
        else if (t >= 1.5f && t < 1.7f) 
        { 
            Jedi.FX = 33; Sith.FX = 47; // Close distance (was 28, 52)
            SetPoses(2, 1); 
        }

        // Beat 3: Heavy Clash
        if (t > 2.2f && t < 2.5f) { SetPoses(5, 5); }
        else if (t >= 2.5f && t < 2.7f) 
        { 
            Jedi.FX = 38; Sith.FX = 42; // Very close (kept same, was good)
            SetPoses(1, 1); 
            SpawnSparks(40, 10, 5); 
        }
        else if (t >= 2.7f) 
        { 
            Jedi.FX = 36; Sith.FX = 44; 
            SetPoses(0, 0); 
        }

        if (_time > 13.0f) _phase = Phase.ForceSequence;
    }

    private void UpdateForceSequence()
    {
        float t = _time - 13.0f; 

        if (t < 0.5f) SetPoses(0, 6);
        else if (t < 1.5f)
        {
             SetPoses(7, 6);
             float progress = (t - 0.5f); 
             Jedi.FX = 35 - (25 * progress);
        }
        else if (t < 2.5f) SetPoses(7, 0); 
        else if (t < 2.7f) SetPoses(11, 0);
        else if (t < 3.7f)
        {
            SetPoses(8, 0); 
            float jumpProgress = (t - 2.7f); 
            Jedi.FX = 10 + (28 * jumpProgress);
            float height = 8.0f; 
            Jedi.FY = 15 - (height * 4 * jumpProgress * (1 - jumpProgress));
        }
        else if (t < 4.0f)
        {
            Jedi.FY = 15;
            SetPoses(11, 5);
        }
        else _phase = Phase.SaberActionSequence;
    }

    private void UpdateSaberActionSequence()
    {
        float t = _time - 17.0f; 

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

        if (_time > 25.0f && _time < 25.1f) FlashScreen = true;
        else FlashScreen = false;

        if (_time > 25.1f && _time < 28.0f && _rng.NextDouble() > 0.5)
            SpawnSparks(40, 11, 2);

        if (_time > 29.0f) _phase = Phase.Resolution;
    }

    private void UpdateResolution()
    {
        FlashScreen = false;
        SetPoses(0, 4); 
        SetSaber(Sith, false, 0);
        if (_time > 32.0f) _phase = Phase.FadeOut;
    }

    private void UpdateFadeOut()
    {
        if (_time > 33.0f) _phase = Phase.Exit;
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
        float targetX = anchorX - (a.FacingRight ? 2.0f : -2.0f);
        float targetY = anchorY + 3.0f;

        targetX -= velX * 0.1f;
        targetY -= velY * 0.1f;

        if (a.PoseIndex == 6 || a.PoseIndex == 9 || velY < -5.0f) 
        {
             targetY -= 2.0f;
             targetX -= (a.FacingRight ? 3.0f : -3.0f);
        }

        a.CapeTail.X += (targetX - a.CapeTail.X) * 5.0f * dt;
        a.CapeTail.Y += (targetY - a.CapeTail.Y) * 5.0f * dt;
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
