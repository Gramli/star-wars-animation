using StarWarsAnimation.Core;
using StarWarsAnimation.Entities;

namespace StarWarsAnimation.Simulation
{
    public class DuelSimulation
    {
        public const float LogicWidth = Constants.LogicWidth;
        public const float LogicHeight = Constants.LogicHeight;

        public List<Vec2> Stars { get; private set; } = new();
        public ParticleManager Particles { get; private set; } = new();
        
        // Expose lists for Renderer via Manager
        public List<Spark> Sparks => Particles.Sparks;
        public List<Spark> Smoke => Particles.Smoke;
        public List<LightningBolt> LightningBolts => Particles.LightningBolts;
        public List<ScorchMark> ScorchMarks => Particles.ScorchMarks;

        public Actor Jedi { get; private set; }
        public Actor Sith { get; private set; }

        public bool FlashScreen { get; private set; }
        public bool ShakeScreen { get; private set; }
        public float ZoomLevel { get; private set; } = 1.0f;
        public float CameraAngle { get; private set; } = 0.0f; // 0 = Side view, +/- values = rotation
        public Vec2 CameraFocus { get; private set; } = new Vec2(40, 12);
        public bool IsFinished => _phase == Phase.Exit;
        public float TimeScale { get; set; } = 1.0f;
        public bool IsDarkness { get; private set; } = false;
        
        public bool WallDamaged { get; private set; }
        public List<DebrisChunk> DebrisChunks { get; private set; } = new();
        public string Subtitle { get; private set; } = "";
        
        public string CrawlText = 
    @"EPISODE VII
    THE CLI AWAKENS

    It is a time of coding challenges.
    A lone JEDI KNIGHT defends the
    terminal from the dark side.

    A SITH LORD, master of bugs,
    seeks to crash the system.

    As pixels clash and cursors
    flash, the fate of the
    console hangs in the balance...";

        public float Time => _time; // Expose for renderer
        public Phase CurrentPhase => _phase; // Expose for renderer

        private float _time;
        private float _phaseTimer;
        private Random _rng = new(123);
        private Phase _phase = Phase.OpeningCrawl;
        private int _blitzCount = 0;

        public enum Phase 
        { 
            OpeningCrawl,       // Star Wars style scrolling text
            Establishment,      // Intro scene, characters enter
            FirstExchange,      // Initial contact and first strike
            Escalation,         // Fight intensifies
            WallDestruction,    // Sith breaks the wall
            ForceSequence,      // Force push/lightning sequence
            Blackout,           // Lights go out
            SpeedBlitz,         // Fast teleporting attacks in dark
            RestoreLights,      // Lights come back on
            SaberActionSequence,// Main choreography
            Climax,             // Final struggle and kill
            Resolution,         // Aftermath
            FadeOut,            // Fade to black
            Exit                // End program
        }

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
            dt *= TimeScale;
            _time += dt;
            _phaseTimer += dt;
            
            Jedi.SaveState();
            Sith.SaveState();

            switch (_phase)
            {
                case Phase.OpeningCrawl: UpdateOpeningCrawl(); break;
                case Phase.Establishment: UpdateEstablishment(); break;
                case Phase.FirstExchange: UpdateFirstExchange(); break;
                case Phase.Escalation: UpdateEscalation(); break;
                case Phase.WallDestruction: UpdateWallDestruction(); break;
                case Phase.ForceSequence: UpdateForceSequence(); break;
                case Phase.Blackout: UpdateBlackout(); break;
                case Phase.SpeedBlitz: UpdateSpeedBlitz(dt); break;
                case Phase.RestoreLights: UpdateRestoreLights(); break;
                case Phase.SaberActionSequence: UpdateSaberActionSequence(); break;
                case Phase.Climax: UpdateClimax(); break;
                case Phase.Resolution: UpdateResolution(); break;
                case Phase.FadeOut: UpdateFadeOut(); break;
            }

            Sith.UpdateCape(dt);
            UpdateDebris(dt);
            Particles.Update(dt);
        }
        
        public void SkipPhase()
        {
            if (_phase < Phase.Exit)
            {
                SetPhase(_phase + 1);
                
                // Special reset logic for certain phases if needed
                if (_phase == Phase.SpeedBlitz) _blitzCount = 0;
                if (_phase == Phase.Resolution) 
                {
                    Jedi.SaberActive = false;
                    Sith.SaberActive = false;
                }
            }
        }

        private void SetPhase(Phase p)
        {
            _phase = p;
            _phaseTimer = 0;
        }

        private void UpdateOpeningCrawl()
        {
            // Crawl lasts longer to ensure text clears screen
            // Text is ~30 units high. Screen 25. Need to scroll ~55 units.
            // Speed is 2.5f. Time needed = 55 / 2.5 = 22s.
            if (_phaseTimer > 22.0f) 
            {
                _time = 0; 
                SetPhase(Phase.Establishment);
            }
        }

        private void UpdateEstablishment()
        {
            if (_time < 0.5f) { Jedi.PoseIndex = 0; Sith.PoseIndex = 0; Subtitle = ""; }
            else if (_time < 1.0f) { Jedi.PoseIndex = 12; }
            else if (_time < 1.5f) { SetSaber(Jedi, true, 3); Jedi.PoseIndex = 0; }
            else if (_time < 3.0f) { Subtitle = "JEDI: Surrender! The system is secure."; }
            
            if (_time > 1.2f && _time < 1.7f) { Sith.PoseIndex = 12; }
            else if (_time > 1.7f && _time < 2.0f) { SetSaber(Sith, true, 3); Sith.PoseIndex = 0; }
            else if (_time > 3.0f && _time < 4.5f) { Subtitle = "SITH: Peace is a lie... there is only Bugs."; }
            
            if (_time > 4.5f) { Subtitle = ""; _phase = Phase.FirstExchange; }
        }

        private void UpdateFirstExchange()
        {
            float t = _phaseTimer;
            
            // Smooth Approach (Walk)
            if (t < 1.0f) 
            { 
                float p = t / 1.0f;
                Jedi.FX = Lerp(20, 32, p);
                Sith.FX = Lerp(60, 48, p);
                
                // Walk Animation (Swap legs every 0.15s)
                int walkFrame = (int)(t * 6) % 2;
                Jedi.PoseIndex = 13 + walkFrame; 
                Sith.PoseIndex = 13 + walkFrame;
            }
            else if (t < 1.3f) 
            { 
                // Idle wait
                Jedi.FX = 32; Sith.FX = 48;
                Jedi.PoseIndex = 0; Sith.PoseIndex = 0; 
            }
            else if (t >= 1.3f && t < 1.5f) 
            { 
                // Windup - Move closer for the hit
                Jedi.FX = 38; Sith.FX = 42;
                SetPoses(5, 2); 
                ZoomLevel = 1.2f; 
            }
            else if (t >= 1.5f && t < 1.7f) 
            { 
                // The Hit
                Jedi.FX = 38; Sith.FX = 42;
                SetPoses(1, 1); 
                
                // Spark only on first frame of hit
                if (t >= 1.5f && t < 1.55f) 
                {
                    Particles.SpawnSparks(40, 12, 3); 
                    ShakeScreen = true;
                }
                ZoomLevel = 1.4f; 
            }
            else if (t >= 1.7f) 
            { 
                // Disengage
                Jedi.FX = 32; Sith.FX = 48;
                SetPoses(0, 0); 
                ShakeScreen = false; 
                ZoomLevel = 1.0f; 
            }

            // Camera Tracking
            CameraFocus = new Vec2((Jedi.FX + Sith.FX)/2, 12);

            if (t > 3.0f) SetPhase(Phase.Escalation);
        }

        private void UpdateEscalation()
        {
            float t = _phaseTimer; 

            if (TimeWindow(t, 0.2f, 1.0f)) Beat1_FastClash(t);
            else if (TimeWindow(t, 1.2f, 1.7f)) Beat2_Riposte(t);
            else if (TimeWindow(t, 2.2f, 3.2f)) Beat3_HeavyClash(t);
            else if (TimeWindow(t, 3.2f, 4.2f)) Beat4_Disengage(t);
            else if (TimeWindow(t, 4.2f, 5.5f)) Beat5_TheFlurry(t);
            else if (TimeWindow(t, 5.5f, 7.0f)) Beat6_JumpDodge(t);
            else if (TimeWindow(t, 7.0f, 8.0f)) Beat7_ReverseClash(t);
            else if (TimeWindow(t, 8.0f, 9.0f)) Beat8_Reset(t);

            CameraFocus = new Vec2((Jedi.FX + Sith.FX)/2, 12);

            if (t > 9.0f) SetPhase(Phase.WallDestruction);
        }

        private void UpdateWallDestruction()
        {
            float t = _phaseTimer;
            
            // 0.0 - 1.5: Preparation
            if (t < 1.5f)
            {
                SetPoses(2, 6); // Jedi Guards, Sith Pulls
                Sith.FacingRight = false; // Face left towards Jedi
                Jedi.FacingRight = true;
                
                // Shout: 0.2 - 1.5
                if (t > 0.2f) Subtitle = "SITH: BREAK!";
                
                // Wall Cracking Effect
                if (_rng.NextDouble() > 0.7) Particles.SpawnSparks(78, (int)(5 + _rng.NextDouble() * 10), 1);
                if (t > 0.5f) ShakeScreen = true;
            }
            // 1.5 - 5.5: The Barrage
            else if (t < 5.5f)
            {
                Subtitle = "";
                ShakeScreen = false;
                WallDamaged = true;
                
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
                    Particles.SpawnSparks(78, (int)spawnY, 3); 
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
            // 5.5 - 6.5: Cooldown
            else if (t < 6.5f)
            {
                ShakeScreen = false;
                SetPoses(0, 0);
            }
            else
            {
                SetPhase(Phase.ForceSequence);
            }
        }

        private void UpdateForceSequence()
        {
            float t = _phaseTimer;

            if (t < 0.5f) SetPoses(0, 6); // Sith prepares
            else if (t < 2.5f) // Lightning Phase (2 seconds)
            {
                 SetPoses(2, 6); // Jedi Blocks (2), Sith Casts (6)
                 
                 // Generate Lightning
                 if (_rng.NextDouble() > 0.3)
                 {
                     var bolt = new LightningBolt { Life = 0.1f };
                     GenerateLightning(bolt, new Vec2(Sith.FX - 2, Sith.FY - 3), new Vec2(Jedi.FX + 1, Jedi.FY - 3));
                     Particles.AddLightning(bolt);
                 }

                 // Jedi slides back slightly
                 float progress = (t - 0.5f); 
                 Jedi.FX = 35 - (5 * progress);
                 
                 // Sparks at impact
                 if (_rng.NextDouble() > 0.5) Particles.SpawnSparks((int)Jedi.FX + 2, (int)Jedi.FY - 3, 1);
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
            else SetPhase(Phase.Blackout);
        }

        private void UpdateBlackout()
        {
            float t = _phaseTimer;
            
            if (t < 0.5f) { /* Wait */ }
            else if (t < 1.5f) 
            { 
                if (!IsDarkness)
                {
                     IsDarkness = true; 
                     // Sith Roar/Power up logic?
                     Sith.PoseIndex = 6; // Cast/Power
                }
            }
            else
            {
                SetPhase(Phase.SpeedBlitz);
                _blitzCount = 0;
            }
        }

        private void UpdateSpeedBlitz(float dt)
        {
            float t = _phaseTimer;
            // Slower pace: 0.3s per clash (was 0.15s)
            int currentStep = (int)(t / 0.35f);
            
            if (currentStep > _blitzCount)
            {
                _blitzCount = currentStep;
                
                if (_blitzCount > 12) // End after 12 hits (~4.2s)
                {
                    SetPhase(Phase.RestoreLights);
                    return;
                }

                // Structured Teleport Logic - Keep somewhat centered but dynamic
                float cx = 40 + (float)(_rng.NextDouble() * 30 - 15);
                float cy = 12 + (float)(_rng.NextDouble() * 8 - 4);
                
                // Occasionally switch sides
                bool jediLeft = _rng.NextDouble() > 0.3;
                
                Jedi.FX = cx + (jediLeft ? -2 : 2);
                Sith.FX = cx + (jediLeft ? 2 : -2);
                Jedi.FY = cy;
                Sith.FY = cy;
                Jedi.FacingRight = jediLeft;
                Sith.FacingRight = !jediLeft;

                // Structured Clash Poses (Attack/Block pairs)
                // 0: High Attack / High Block
                // 1: Low Attack / Low Block
                // 2: Mid Clash (Pose 5)
                // 3: Saber Lock (Pose 3) - Guaranteed Cross!
                
                int clashType = _rng.Next(0, 4);
                
                // Force at least 2 crosses (locks) in the sequence
                // Blitz runs 0..12. Let's force it on step 4 and 9
                if (_blitzCount == 4 || _blitzCount == 9) clashType = 3;

                if (clashType == 0) 
                {
                    // High
                    Jedi.PoseIndex = jediLeft ? 1 : 2; // Attack / Guard
                    Sith.PoseIndex = jediLeft ? 2 : 1;
                }
                else if (clashType == 1)
                {
                    // Low
                    Jedi.PoseIndex = jediLeft ? 11 : 5; // Crouch Attack / Guard
                    Sith.PoseIndex = jediLeft ? 5 : 11;
                }
                else if (clashType == 2)
                {
                    // Mid Clash
                    Jedi.PoseIndex = 5;
                    Sith.PoseIndex = 5;
                }
                else
                {
                    // Saber Lock (Crossing) - Ensure close proximity
                    Jedi.FX = cx - 1.5f; // Closer than usual (was +/- 2)
                    Sith.FX = cx + 1.5f;
                    
                    Jedi.PoseIndex = 3; // Lock Pose
                    Sith.PoseIndex = 3; // Lock Pose
                    
                    // Extra sparks for the cross
                    Particles.SpawnSparks((int)cx, (int)cy, 8);
                }
                
                if (clashType != 3) // Normal spark logic for non-locks
                {
                    Particles.SpawnSparks((int)cx, (int)cy, 3);
                }
                ShakeScreen = true;
            }
            else
            {
                ShakeScreen = false;
            }
        }

        private void UpdateRestoreLights()
        {
            IsDarkness = false;
            ShakeScreen = false;
            
            // Reset positions for next sequence
            Jedi.FX = 20; Sith.FX = 60;
            Jedi.FY = 15; Sith.FY = 15;
            SetPoses(2, 2);
            
            SetPhase(Phase.SaberActionSequence);
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
            float t = _phaseTimer;

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
                if (t > 5.8f) Particles.SpawnSparks(60, 8, 2); 
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
            else SetPhase(Phase.Climax);
        }

        private void UpdateClimax()
        {
            float t = _phaseTimer;

            // 0.0 - 1.5: The final struggle (Lock)
            if (t < 1.5f)
            {
                 TimeScale = 1.0f;
                 Jedi.FX = 38; Sith.FX = 42;
                 SetPoses(3, 3); // Lock
                 ShakeScreen = true; // Struggle shake
                 ZoomLevel = 1.2f;
                 if (_rng.NextDouble() > 0.6) Particles.SpawnSparks(40, 11, 1);
            }
            // SLOW MOTION MOMENT
            else if (t < 3.2f) // Even shorter duration (1.7s)
            {
                 TimeScale = 0.3f; 
                 ZoomLevel = 1.3f; 
                 ShakeScreen = false; 
                 
                 // Spin logic: 0 to 1.0 progress
                 float spinProgress = (t - 1.5f) / 1.7f;
                 
                 // Rotation Arc: 0 -> Top-Down (90 deg) -> 0
                 // PI/2 = 1.57 rad.
                 float maxAngle = 1.4f; // Nearly 90 degrees
                 CameraAngle = (float)Math.Sin(spinProgress * Math.PI) * maxAngle;
                 
                 // Display Camera Angle in Subtitle
                 int deg = (int)(CameraAngle * (180.0f / Math.PI));
                 Subtitle = $"CAM: {deg}°";
            }
            // 1.5: THE KILL STROKE
            else if (t < 6.2f) // Shifted timeline
            {
                 CameraAngle = 0.0f; 
                 TimeScale = 1.0f; 
                 Jedi.FX = 39; Sith.FX = 42;
                 SetPoses(10, 7); 
                 
                 // Subtitle: 
                 if (t > 3.5f && t < 6.0f) Subtitle = "JEDI: It is finished.";
                 else Subtitle = "";
                 
                 // Visuals
                 ShakeScreen = true;
                 if (t < 3.8f) Particles.SpawnSparks(41, 11, 8); 
                 
                 // Flash only on the frame of impact
                 if (t >= 3.2f && t < 3.3f) FlashScreen = true; 
                 else FlashScreen = false;
            }
            // 2.5 - 4.0: The fall
            else if (t < 7.5f)
            {
                 Subtitle = "";
                 FlashScreen = false;
                 ShakeScreen = false;
                 ZoomLevel = 1.0f;
                 
                 Jedi.PoseIndex = 2; // Guard/Stand back
                 Jedi.FX = 34; // Step back
                 
                 Sith.PoseIndex = 4; // Kneel/Collapse
                 Sith.SaberActive = false; // Drop saber
            }
            else
            {
                 SetPhase(Phase.Resolution);
            }
        }

        private void UpdateResolution()
        {
            float t = _phaseTimer;

            FlashScreen = false;
            ShakeScreen = false;
            ZoomLevel = 1.0f;
            TimeScale = 1.0f;
            
            SetPoses(0, 4); 
            SetSaber(Sith, false, 0);
            // Turn off Jedi saber after a moment
            if (t > 2.0f) 
            {
                SetSaber(Jedi, false, 0);
                Subtitle = "May the Source be with you.";
            }
            
            if (t > 4.0f) SetPhase(Phase.FadeOut);
        }

        private void UpdateFadeOut()
        {
            if (_phaseTimer > 4.0f) SetPhase(Phase.Exit);
        }

        private void Beat1_FastClash(float t)
        {
            if (t < 0.5f) { SetPoses(5, 5); }
            else if (t < 0.7f) 
            { 
                Jedi.FX = 35; Sith.FX = 45; 
                SetPoses(1, 1); 
            }
            else if (t < 1.0f) { Particles.SpawnSparks(40, 13, 1); }
        }

        private void Beat2_Riposte(float t)
        {
            if (t < 1.5f) { SetPoses(2, 5); }
            else if (t < 1.7f) 
            { 
                Jedi.FX = 33; Sith.FX = 47; 
                SetPoses(2, 1); 
            }
        }

        private void Beat3_HeavyClash(float t)
        {
            if (t < 2.5f) { SetPoses(5, 5); ZoomLevel = 1.3f; }
            else if (t < 2.7f) 
            { 
                Jedi.FX = 38; Sith.FX = 42; 
                SetPoses(1, 1); 
                Particles.SpawnSparks(40, 10, 5);
                ShakeScreen = true; 
                ZoomLevel = 1.5f; 
                
                // Permanent Scorch Mark on Floor
                if (t > 2.5f && t < 2.55f) Particles.AddScorchMark(new Vec2(40, 16));
            }
            else if (t < 3.2f) 
            { 
                SetPoses(3, 3); // Brief Lock
                Jedi.FX = 36; Sith.FX = 44; 
                ShakeScreen = false;
                ZoomLevel = 1.0f;
            }
        }

        private void Beat4_Disengage(float t)
        {
            ShakeScreen = false; ZoomLevel = 1.0f;
            float p = (t - 3.2f);
            Jedi.FX = Lerp(36, 28, p);
            Sith.FX = Lerp(44, 52, p);
            SetPoses(2, 2); // Guarding
        }

        private void Beat5_TheFlurry(float t)
        {
            if (t < 4.5f) { SetPoses(5, 5); } // Wind up
            else if (t < 4.7f) // High Hit
            {
                 Jedi.FX = 38; Sith.FX = 42;
                 SetPoses(1, 2); // Attack/Guard
                 if (t < 4.55f) { Particles.SpawnSparks(40, 10, 2); ShakeScreen = true; }
            }
            else if (t < 4.9f) // Low Hit
            {
                 SetPoses(11, 1); // Crouch-Attack / Attack
                 if (t < 4.75f) { Particles.SpawnSparks(40, 14, 2); }
            }
            else if (t < 5.1f) // Mid Hit
            {
                 SetPoses(1, 1);
                 if (t < 4.95f) { Particles.SpawnSparks(40, 12, 2); ShakeScreen = true; }
            }
            else if (t < 5.5f) { SetPoses(2, 2); ShakeScreen = false; } // Reset
        }

        private void Beat6_JumpDodge(float t)
        {
            if (t < 6.5f)
            {
                 float jp = (t - 5.5f);
                 Sith.PoseIndex = 10; 
                 Sith.FX = Lerp(42, 30, jp); 
                 
                 Jedi.PoseIndex = 8;
                 Jedi.FY = 15 - (float)Math.Sin(jp * Math.PI) * 9;
                 Jedi.FX = Lerp(38, 50, jp); 
                 
                 if (jp > 0.4f && jp < 0.6f && jp < 0.45f) Particles.SpawnSparks(35, 12, 3);
            }
            else // 6.5 - 7.0 Land & Face
            {
                 Jedi.FY = 15; Jedi.FX = 50; Sith.FX = 30;
                 Jedi.FacingRight = false; Sith.FacingRight = true;
                 SetPoses(11, 11);
            }
        }

        private void Beat7_ReverseClash(float t)
        {
            if (t < 7.3f) { SetPoses(5, 5); }
            else 
            {
                 Jedi.FX = 42; Sith.FX = 38;
                 SetPoses(1, 1);
                 if (t < 7.35f) { Particles.SpawnSparks(40, 12, 5); ShakeScreen = true; ZoomLevel = 1.4f; }
            }
        }

        private void Beat8_Reset(float t)
        {
             ShakeScreen = false; ZoomLevel = 1.0f;
             float rp = (t - 8.0f);
             
             Jedi.FX = Lerp(42, 25, rp);
             Sith.FX = Lerp(38, 55, rp);
             
             if (rp > 0.5f) { Jedi.FacingRight = true; Sith.FacingRight = false; }
             SetPoses(8, 8); // Jump back
        }

        private bool TimeWindow(float t, float start, float end) => t >= start && t < end;
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
                        Particles.SpawnSparks((int)d.Pos.X, (int)d.Pos.Y, 8); // MASSIVE sparks
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
                        Particles.AddScorchMark(d.Pos); 
                        Particles.SpawnSparks((int)d.Pos.X, (int)d.Pos.Y, 2);
                        Particles.SpawnSmoke((int)d.Pos.X, (int)d.Pos.Y, 3);
                    }
                }

                DebrisChunks[i] = d;
            }

            // Cleanup
            if (DebrisChunks.Count > 0 && DebrisChunks[0].Pos.X < -10) DebrisChunks.RemoveAt(0);
        }
    }
}
