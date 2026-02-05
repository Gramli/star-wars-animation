using System;
using System.Collections.Generic;
using StarWarsAnimation.Core;
using StarWarsAnimation.Entities;
using StarWarsAnimation.Simulation;

namespace StarWarsAnimation.Rendering
{
    public class SceneRenderer
    {
        private const float LogicWidth = Constants.LogicWidth;
        private const float LogicHeight = Constants.LogicHeight;

        private readonly TerminalRenderer _term;
        private readonly float _scaleX;
        private readonly float _scaleY;
        private char[,] _bgChars;
        private string[,] _bgColors;

        public int Width => _term.Width;
        public int Height => _term.Height;

        public SceneRenderer()
        {
            _term = new TerminalRenderer();
            _scaleX = _term.Width / LogicWidth;
            _scaleY = _term.Height / LogicHeight;
            InitBackground();
        }

        private void InitBackground()
        {
            int w = _term.Width;
            int h = _term.Height;
            _bgChars = new char[h, w];
            _bgColors = new string[h, w];

            int floorY = (int)(15 * _scaleY);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    char c = ' ';
                    string color = Palette.Dim;

                    // Floor (Futuristic)
                    if (y == floorY) c = '━'; 
                    else if (y == floorY + 1) c = '▍'; 
                    // Under ground: Empty 
                    else if (y > floorY + 1) c = ' ';
                    
                    // Ceiling
                    else if (y == 0) c = '━';
                    else if (y == 1) c = '▖'; 

                    // Background Details
                    else
                    {
                        float px = (float)x / w;
                        
                        // Main Columns (Futuristic Hex/Block style)
                        if (px < 0.1f || px > 0.9f)
                        {
                            if (x % 4 == 0) c = '▐';
                            else if (y % 5 == 0) c = '≡';
                            else c = ' ';
                        }
                        // Window Frames (Angled)
                        else if ((px >= 0.1f && px < 0.12f))
                        {
                             c = '╱'; // Angled frame left
                        }
                        else if ((px <= 0.9f && px > 0.88f))
                        {
                            c = '╲'; // Angled frame right
                        }
                    }

                    _bgChars[y, x] = c;
                    _bgColors[y, x] = color;
                }
            }
        }

        public void Render(DuelSimulation sim)
        {
            _term.Clear();
            if (sim.FlashScreen)
            {
                _term.Fill('█', Palette.White);
            }
            else if (sim.CurrentPhase == DuelSimulation.Phase.OpeningCrawl)
            {
                DrawOpeningCrawl(sim);
            }
            else
            {
                RenderScene(sim);
                
                // UI Overlay
                if (!string.IsNullOrEmpty(sim.Subtitle))
                {
                    DrawSubtitle(sim.Subtitle);
                }
            }
            _term.Present();
        }
        
        private void DrawOpeningCrawl(DuelSimulation sim)
        {
            // Star Field Background
            foreach (var s in sim.Stars)
            {
                 _term.Draw((int)(s.X * _scaleX), (int)(s.Y * _scaleY), '.', Palette.Dim);
            }
            
            // Perspective Text
            string[] lines = sim.CrawlText.Replace("\r", "").Split('\n');
            float scrollY = sim.Time * 2.5f; // Speed match Update logic
            float startY = _term.Height;
            
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                float worldY = startY + (i * 2) - scrollY; 
                
                if (worldY > _term.Height || worldY < 0) continue;
                
                int screenY = (int)worldY;
                int centerX = _term.Width / 2;
                int len = line.Length;
                int drawX = centerX - (len / 2);
                if (drawX < 0) drawX = 0; // Safety bound
                _term.DrawString(drawX, screenY, line, Palette.Yellow);
            }
        }

        private void DrawSubtitle(string text)
        {
            int cx = _term.Width / 2;
            int y = _term.Height - 3;
            int len = text.Length;
            int x = cx - (len / 2);
            
            // Draw black bar behind
            for(int i=-2; i<len+2; i++) _term.Draw(x+i, y, ' ', "\u001b[40m"); 
            
            _term.DrawString(x, y, text, Palette.Cyan);
        }

        private void RenderScene(DuelSimulation sim)
        {
            int floorY = (int)(15 * _scaleY);
            
            // Camera Math
            float zoom = sim.ZoomLevel;
            float angle = sim.CameraAngle; // Rotation Angle (radians)
            float camX = sim.CameraFocus.X;
            float camY = sim.CameraFocus.Y;
            
            // Screen Shake Jitter
            int shakeX = 0, shakeY = 0;
            if (sim.ShakeScreen)
            {
                var rng = new Random();
                shakeX = rng.Next(-1, 2);
                shakeY = rng.Next(-1, 2);
            }

            // Transform function: World -> Screen with Zoom & Pan & Rotation (Skew)
            (int sx, int sy) Transform(float wx, float wy)
            {
                // 1. Center relative to camera focus
                float relX = (wx - camX);
                float relY = (wy - camY);

                // 2. Rotate Effect (PITCH - Top Down)
                // As angle increases (Pitch -> 90), Y-distances compress.
                // X-distances stay roughly same (or get Perspective?).
                
                float yScale = (float)Math.Cos(angle); 
                
                // Apply scale to Y (Height compression)
                float rotatedY = relY * yScale;
                float rotatedX = relX; // No yaw rotation
                
                // 3. Zoom
                float zoomedX = rotatedX * zoom;
                float zoomedY = rotatedY * zoom; 
                
                // 4. Project back
                int screenX = (int)((zoomedX + LogicWidth/2) * _scaleX) + shakeX;
                int screenY = (int)((zoomedY + LogicHeight/2) * _scaleY) + shakeY;
                return (screenX, screenY);
            }

            // Actors - Transform Calculation
            var jediPos = Transform(sim.Jedi.FX, sim.Jedi.FY);
            var sithPos = Transform(sim.Sith.FX, sim.Sith.FY);

            // Stars (Parallax - move opposite to camera rotation)
            // If Pitching Up/Down, Stars should move vertically? 
            // Let's keep them static for stability or move slightly down.
            
            // Background - Shift based on Angle
            // If Pitching Up (looking down), Wall should slide UP/Away?
            // Actually, user said "walls disappears".
            
            // Only draw Wall Background if angle is low
            if (angle < 0.8f)
            {
                for (int y = 0; y < _term.Height; y++)
                {
                    for (int x = 0; x < _term.Width; x++)
                    {
                        int drawX = x + shakeX;
                        int drawY = y + shakeY;
                        
                        char c = _bgChars[y, x];
                        if (c != ' ') _term.Draw(drawX, drawY, c, _bgColors[y, x]);
                    }
                }
            }
            
            // Draw Floor Grid (Visible when Top Down)
            if (angle > 0.2f)
            {
                // Transform the center of the arena (assuming world 0,0 or logic center)
                // LogicWidth/2 is the center X usually.
                var centerPos = Transform(LogicWidth / 2, 15); // 15 is floor level
                int cx = centerPos.sx;
                int cy = centerPos.sy;
                
                float intensity = (angle - 0.2f) / 0.8f; // 0 to 1
                if (intensity > 1) intensity = 1;
                
                string floorColor = intensity > 0.5f ? Palette.White : Palette.Dim;
                
                // Radial floor pattern - Denser for better lighting
                for (int r = 1; r < 24; r++)
                {
                     // Ellipse aspect ratio
                     int rx = (int)(r * 1.5 * zoom * _scaleX);
                     int ry = (int)(r * 1.5 * zoom * _scaleY * (float)Math.Cos(angle)); 
                     
                     // Draw Ellipse points
                     int step = r < 10 ? 15 : 10;
                     for (int deg=0; deg<360; deg+=step)
                     {
                         double rad = deg * Math.PI / 180.0;
                         int px = cx + (int)(Math.Cos(rad) * rx);
                         int py = cy + (int)(Math.Sin(rad) * ry);
                         
                         char c = '.';
                         if (r % 5 == 0) c = '+'; // Grid markers
                         
                         _term.Draw(px, py, c, floorColor);
                     }
                }
                
                // Apply Floor Lighting from Sabers (Glow on the ground)
                if (intensity > 0.3f)
                {
                    if (sim.Jedi.SaberActive) 
                    {
                        // Light at feet
                        _term.ApplyLighting(jediPos.sx, jediPos.sy, 16, Palette.Blue);
                        // Stronger core
                        _term.ApplyLighting(jediPos.sx, jediPos.sy, 8, Palette.Cyan);
                    }
                    
                    if (sim.Sith.SaberActive) 
                    {
                        _term.ApplyLighting(sithPos.sx, sithPos.sy, 16, Palette.Red);
                         // Stronger core
                        _term.ApplyLighting(sithPos.sx, sithPos.sy, 8, Palette.Red);
                    }
                }
            }

            // Wall Damage (Hole) - Shifted with Background
            if (sim.WallDamaged && angle < 0.5f)
            {
                // DrawWallDamage(shakeX, shakeY);
                 // Hole at right wall (approx x=74..80, y=4..14)
                // We draw "void" (spaces) and "rubble edges"
                
                for (int y = 4; y <= 14; y++)
                {
                    for (int x = 74; x < 80; x++)
                    {
                        int sx = (int)(x * _scaleX) + shakeX;
                        int sy = (int)(y * _scaleY) + shakeY;

                        // Center of the hole is empty/dark
                        bool isCenter = (x > 75 && y > 5 && y < 13);
                        
                        if (isCenter)
                        {
                            // Erase background
                            _term.Draw(sx, sy, ' ', Palette.Dim);
                            // Add some depth details occasionally
                            if ((x+y)%3 == 0) _term.Draw(sx, sy, '░', Palette.Dim); 
                        }
                        else
                        {
                            // Edges - jagged
                            char c = (x+y)%2 == 0 ? '▙' : '▟';
                            if (y == 4 || y == 14) c = '▄';
                            _term.Draw(sx, sy, c, Palette.Dim);
                        }
                    }
                }
            }

            // Permanent Debris (Scorch marks) - Transformed
            foreach (var d in sim.ScorchMarks)
            {
                var pos = Transform(d.Pos.X, d.Pos.Y);
                
                // Molten cooling logic
                char c = '░';
                string color = Palette.Dim;
                
                if (d.Age < 0.3f) 
                {
                    c = '▓'; 
                    color = Palette.White; 
                    _term.ApplyLighting(pos.sx, pos.sy, 4, Palette.Yellow); 
                }
                else if (d.Age < 1.5f)
                {
                    c = '▒';
                    color = Palette.Red; 
                    _term.ApplyLighting(pos.sx, pos.sy, 2, Palette.Red);
                }
                
                _term.Draw(pos.sx, pos.sy, c, color); 
            }

            // Flying Debris Chunks - Transformed
            foreach (var chunk in sim.DebrisChunks)
            {
                if (!chunk.Active) continue;
                var pos = Transform(chunk.Pos.X, chunk.Pos.Y);
                _term.Draw(pos.sx, pos.sy, chunk.Char, Palette.White);
            }

            // Actors - Transform Calculation (Already done above)
            // var jediPos = Transform(sim.Jedi.FX, sim.Jedi.FY);
            // var sithPos = Transform(sim.Sith.FX, sim.Sith.FY);
            
            // Calculate offsets from default projection to feed into DrawActor
            int jediOx = jediPos.sx - (int)(sim.Jedi.FX * _scaleX);
            int jediOy = jediPos.sy - (int)(sim.Jedi.FY * _scaleY);
            int sithOx = sithPos.sx - (int)(sim.Sith.FX * _scaleX);
            int sithOy = sithPos.sy - (int)(sim.Sith.FY * _scaleY);
            
            // Calculate Height Scale (Vertical Foreshortening)
            float heightScale = (float)Math.Cos(angle);
            
            int floorScreenY = (int)(15 * _scaleY) + shakeY;

            // Reflections (Only if not top down)
            if (angle < 0.5f)
            {
                DrawActor(sim.Jedi, false, true, floorScreenY, jediOx, jediOy, false, 1.0f, heightScale);
                DrawActor(sim.Sith, true, true, floorScreenY, sithOx, sithOy, false, 1.0f, heightScale);
            }

            // Motion Blur (Draw previous position faintly)
            if (Math.Abs(sim.Jedi.FX - sim.Jedi.PrevFX) > 0.5f)
            {
                 var prevJ = Transform(sim.Jedi.PrevFX, sim.Jedi.PrevFY);
                 int pjOx = prevJ.sx - (int)(sim.Jedi.PrevFX * _scaleX);
                 int pjOy = prevJ.sy - (int)(sim.Jedi.PrevFY * _scaleY);
                 DrawActor(sim.Jedi, false, false, floorScreenY, pjOx, pjOy, true, 1.0f, heightScale);
            }
            if (Math.Abs(sim.Sith.FX - sim.Sith.PrevFX) > 0.5f)
            {
                 var prevS = Transform(sim.Sith.PrevFX, sim.Sith.PrevFY);
                 int psOx = prevS.sx - (int)(sim.Sith.PrevFX * _scaleX);
                 int psOy = prevS.sy - (int)(sim.Sith.PrevFY * _scaleY);
                 DrawActor(sim.Sith, true, false, floorScreenY, psOx, psOy, true, 1.0f, heightScale);
            }
            
            // Draw Shadows (Visible when Top Down)
            if (angle > 0.3f)
            {
                // Draw simple circular shadows at foot position
                int jx = jediPos.sx;
                int sx = sithPos.sx;
                int fy = floorScreenY; 
                
                // Jedi Shadow
                _term.Draw(jx, fy, '●', "\u001b[30;1m"); // Dark Gray
                _term.Draw(jx-1, fy, '(', "\u001b[30;1m");
                _term.Draw(jx+1, fy, ')', "\u001b[30;1m");
                
                // Sith Shadow
                _term.Draw(sx, fy, '●', "\u001b[30;1m"); 
                _term.Draw(sx-1, fy, '(', "\u001b[30;1m");
                _term.Draw(sx+1, fy, ')', "\u001b[30;1m");
                
                // Apply darkness around shadows to make them pop against glow
                var shadows = new List<(int x, int y, int r)>();
                shadows.Add((jx, fy, 2));
                shadows.Add((sx, fy, 2));
                _term.ApplyDarkness(shadows);
            }

            // Actors
            var jediTip = DrawActor(sim.Jedi, false, false, floorScreenY, jediOx, jediOy, false, 1.0f, heightScale);
            var sithTip = DrawActor(sim.Sith, true, false, floorScreenY, sithOx, sithOy, false, 1.0f, heightScale);

            // Dynamic Lighting
            if (sim.Jedi.SaberActive && jediTip.HasValue)
                _term.ApplyLighting(jediTip.Value.x, jediTip.Value.y, 12, Palette.Blue);
            
            if (sim.Sith.SaberActive && sithTip.HasValue)
                _term.ApplyLighting(sithTip.Value.x, sithTip.Value.y, 12, Palette.Red);
                
            // Redraw Actors (Pass 2 - Overlay on top of lighting with clean colors)
            DrawActor(sim.Jedi, false, false, floorScreenY, jediOx, jediOy, false, 1.0f, heightScale);
            DrawActor(sim.Sith, true, false, floorScreenY, sithOx, sithOy, false, 1.0f, heightScale);

            // Saber Motion Trails (Arcs)
            if (sim.Jedi.SaberActive) DrawSaberArc(sim.Jedi, jediTip, jediOx, jediOy, Palette.Blue);
            if (sim.Sith.SaberActive) DrawSaberArc(sim.Sith, sithTip, sithOx, sithOy, Palette.Red);

            // Force Lightning
            foreach (var bolt in sim.LightningBolts)
            {
                for (int i = 0; i < bolt.Points.Count - 1; i++)
                {
                    var p1 = bolt.Points[i];
                    var p2 = bolt.Points[i+1];
                    
                    var t1 = Transform(p1.X, p1.Y);
                    var t2 = Transform(p2.X, p2.Y);
                    
                    DrawLine(t1.sx, t1.sy, t2.sx, t2.sy, '+', Palette.White);
                    _term.ApplyLighting(t1.sx, t1.sy, 6, Palette.Cyan);
                }
            }
                    

            // Sparks
            foreach (var s in sim.Sparks)
            {
                var pos = Transform(s.Pos.X, s.Pos.Y);
                _term.Draw(pos.sx, pos.sy, '*', Palette.Yellow);
            }

            // Smoke
            foreach (var s in sim.Smoke)
            {
                 var pos = Transform(s.Pos.X, s.Pos.Y);
                 char c = s.Life > 1.0f ? '▒' : '░';
                 _term.Draw(pos.sx, pos.sy, c, Palette.Dim);
            }

            // Apply Darkness (Blackout Mode)
            if (sim.IsDarkness)
            {
                var lights = new List<(int x, int y, int r)>();
                
                // Jedi Saber
                if (sim.Jedi.SaberActive && jediTip.HasValue) 
                {
                    lights.Add((jediTip.Value.x, jediTip.Value.y, 10));
                    // Add actor center
                    var pos = Transform(sim.Jedi.FX, sim.Jedi.FY);
                    lights.Add((pos.sx, pos.sy, 5));
                }
                
                // Sith Saber
                if (sim.Sith.SaberActive && sithTip.HasValue) 
                {
                    lights.Add((sithTip.Value.x, sithTip.Value.y, 10));
                    var pos = Transform(sim.Sith.FX, sim.Sith.FY);
                    lights.Add((pos.sx, pos.sy, 5));
                }
                
                // Sparks
                foreach (var s in sim.Sparks)
                {
                     var pos = Transform(s.Pos.X, s.Pos.Y);
                     lights.Add((pos.sx, pos.sy, 4));
                }
                
                // Lightning
                 foreach (var bolt in sim.LightningBolts)
                {
                    if (bolt.Points.Count > 0)
                    {
                        // Light up along the bolt? Just use center for now to save perf
                        var p = bolt.Points[bolt.Points.Count / 2];
                        var pos = Transform(p.X, p.Y);
                        lights.Add((pos.sx, pos.sy, 8));
                    }
                }
                
                _term.ApplyDarkness(lights);
            }
        }

        private (int x, int y)? DrawActor(Actor actor, bool isSith, bool isReflection = false, int floorY = 0, int ox = 0, int oy = 0, bool isBlur = false, float widthScale = 1.0f, float heightScale = 1.0f)
        {
            bool isTopDown = heightScale < 0.4f;
            float useFX = isBlur ? actor.PrevFX : actor.FX;
            float useFY = isBlur ? actor.PrevFY : actor.FY;
            
            int x = (int)(useFX * _scaleX) + ox;
            int y = (int)(useFY * _scaleY) + oy;
            int dir = actor.FacingRight ? 1 : -1;
            
            string mainColor = isBlur ? Palette.Dim : (isReflection ? Palette.Dim : Palette.White);
            string saberColor = isBlur ? Palette.Dim : (isReflection ? Palette.Dim : actor.Color);

            if (isReflection)
            {
                 y = floorY + (floorY - y);
                 if (y >= Height) return null;
            }

            // Use PoseRegistry to get the pose structure
            var pose = Pose.Get(actor.PoseIndex, isSith);

            // Draw Cape (Sith only)
            if (isSith && actor.PoseIndex != 4 && !isBlur) 
            {
                 if (isTopDown) DrawCapeTopDown(x, y, dir);
                 else DrawCape(actor, x, y, dir, heightScale, isReflection, floorY);
            }

            if (isTopDown)
            {
                 // Shoulders/Connection
                 _term.Draw(x, y-1, '(', mainColor);
                 _term.Draw(x, y+1, ')', mainColor);
                 // Face Indicator
                 _term.Draw(x + dir, y, '·', mainColor);
            }

            // ROTATION LOGIC:
            // As heightScale decreases (Pitch -> 90), we shift parts based on Z-depth.
            // heightScale = Cos(angle).
            // Sin(angle) = Sqrt(1 - heightScale^2).
            float angleSin = (float)Math.Sqrt(1.0f - Math.Clamp(heightScale * heightScale, 0f, 1f));
            
            // Z-Depth simulation (Screen Y shift)
            // Back limbs (Left) move UP (negative Y)
            // Front limbs (Right) move DOWN (positive Y)
            float zFactor = 1.2f * angleSin; // Reduced from 3.0 to fix "too wide" top view

            bool reflect = isReflection;
            string color = mainColor;

            // Helper to draw parts with Z-shift
            void DrawPart3D(BodyPart part, int zDepth)
            {
                 // Squash vertical offset (Dy) based on camera pitch
                 int dy = (int)(part.Dy * heightScale);
                 
                 // Apply Z-shift (Simulate Top-Down perspective)
                 // zDepth: -1 (Back), 0 (Center), 1 (Front)
                 int zShift = (int)(zDepth * zFactor);
                 
                 int dx = (int)(part.Dx * widthScale);
                 
                 int drawX = x + (dx * dir);
                 int drawY = y + (isReflection ? -(dy + zShift) : (dy + zShift));
                 
                 char c = part.Char;
                 if (isTopDown)
                 {
                     // Replace chars for top-down clarity
                     if (zDepth == 0 && part.Dy == pose.Head.Dy && part.Dx == pose.Head.Dx) c = '●'; // Explicit Head check
                     else if (c == '▓' || c == '▒' || c == '█' || c == '▄' || c == '▀') c = '|';
                     else if (c == '╱' || c == '╲') c = '-';
                     else if (part.Equals(pose.Head)) c = '●'; // Backup check
                 }
                 
                 if (reflect)
                 {
                    if (c == '╱') c = '╲';
                    else if (c == '╲') c = '╱';
                    else if (c == '▄') c = '▀';
                    else if (c == '▀') c = '▄';
                    else if (c == '●') c = '●'; // Explicitly keep head char
                 }
                 
                 _term.Draw(drawX, drawY, c, color);
            }
            
            // Draw Order: Bottom -> Top (Painter's algo for pseudo-3D)
            // Back (Left) -> Center -> Front (Right)
            
            // Legs (Hide in extreme top down)
            if (!isTopDown) 
            {
                 DrawPart3D(pose.LegL, -1);
                 DrawPart3D(pose.LegR, 1);
                 DrawPart3D(pose.TorsoBottom, 0);
            }
            
            DrawPart3D(pose.TorsoTop, 0);
            
            // Arms
            DrawPart3D(pose.ArmL, -1);
            if (actor.PoseIndex != 3) DrawPart3D(pose.ArmR, 1);

            // Head (Always on top/center)
            DrawPart3D(pose.Head, 0);

            // Draw Saber
            (int x, int y)? tipPos = null;
            if (actor.SaberActive)
            {
                int handDx = (int)(pose.Hand.Dx * widthScale);
                int handDy = (int)(pose.Hand.Dy * heightScale);
                
                // Hand Z-shift (Front arm R is z=1, Back arm L is z=-1)
                int zShift = (int)(1 * zFactor); 
                
                int sx = x + (handDx * dir);
                int sy = y + (isReflection ? -(handDy + zShift) : (handDy + zShift));
                
                int bdy = isReflection ? -pose.Blade.Dy : pose.Blade.Dy;
                bdy = (int)(bdy * heightScale);
                
                // Calculate Tip Position
                int bladeDx = (int)(pose.Blade.Dx * actor.SaberLength * widthScale);
                int tipX = sx + (bladeDx * dir);
                int tipY = sy + (bdy * actor.SaberLength);
                
                tipPos = (tipX, tipY);
                
                // Draw Single Line for Saber
                // Calculate screen slope to pick best char to avoid "stairs" artifact
                int dSx = tipX - sx;
                int dSy = tipY - sy;
                
                char bChar = '|'; // Default
                
                if (dSx == 0) bChar = '|';
                else 
                {
                    float slope = (float)dSy / dSx;
                    
                    if (Math.Abs(slope) > 2.0f) bChar = '|';
                    else if (Math.Abs(slope) < 0.5f) bChar = '-';
                    else 
                    {
                        // Diagonal: In screen coords, Y increases downwards.
                        // Slope > 0 means (dy>0, dx>0) or (dy<0, dx<0) -> Down-Right or Up-Left -> '╲'
                        // Slope < 0 means (dy>0, dx<0) or (dy<0, dx>0) -> Down-Left or Up-Right -> '╱'
                        if (slope > 0) bChar = '╲'; 
                        else bChar = '╱';
                    }
                }
                
                DrawLine(sx, sy, tipX, tipY, bChar, saberColor);
            }
            return tipPos;
        }

        private void DrawSaberArc(Actor actor, (int x, int y)? currentTip, int ox, int oy, string color)
        {
            if (!currentTip.HasValue) return;

            // Calculate Previous Tip Position
            // We need to reconstruct where the tip WAS based on PrevFX, PrevFY, and PrevPoseIndex
            
            float prevFX = actor.PrevFX;
            float prevFY = actor.PrevFY;
            int prevPoseIdx = actor.PrevPoseIndex;
            bool isSith = (color == Palette.Red);
            
            var prevPose = Pose.Get(prevPoseIdx, isSith);
            int dir = actor.FacingRight ? 1 : -1; 

            int sx = (int)(prevFX * _scaleX) + ox + (prevPose.Hand.Dx * dir);
            int sy = (int)(prevFY * _scaleY) + oy + (prevPose.Hand.Dy); 
            
            int bdy = prevPose.Blade.Dy;
            int bx = sx + (prevPose.Blade.Dx * dir * actor.SaberLength);
            int by = sy + (bdy * actor.SaberLength);
            
            // previous tip pixel
            int prevTipX = bx;
            int prevTipY = by;

            // If distance is large enough, draw a trail
            int curTipX = currentTip.Value.x;
            int curTipY = currentTip.Value.y;

            int dx = curTipX - prevTipX;
            int dy = curTipY - prevTipY;
            int distSq = dx*dx + dy*dy;
            
            if (distSq > 4) // Only draw arc if moved significantly (> 2 pixels)
            {
                 DrawLine(prevTipX, prevTipY, curTipX, curTipY, '░', color);
                 DrawLine((sx + prevTipX)/2, (sy + prevTipY)/2, (sx + curTipX)/2, (sy + curTipY)/2, '▒', color);
            }
        }
        
        private void DrawCapeTopDown(int x, int y, int dir)
        {
            // Draw a "fan" shape behind the actor
            int backX = x - (dir * 2);
            string c = Palette.Red; // Sith Cape is Red
            
            // Central part
            _term.Draw(backX, y, '=', c);
            _term.Draw(backX + dir, y, '-', c);
            
            // Sides
            _term.Draw(backX, y - 1, dir == 1 ? '╱' : '╲', c);
            _term.Draw(backX, y + 1, dir == 1 ? '╲' : '╱', c);
        }

        private void DrawCape(Actor a, int anchorX, int anchorY, int dir, float heightScale, bool isReflection, int floorY)
        {
            // Cape Anchors relative to actor position (x,y passed in are already screen transformed)
            // Shoulder offset
            int shoulderDy = (int)(-3 * heightScale); 
            int startX = anchorX - dir;
            int startY = anchorY + (isReflection ? -shoulderDy : shoulderDy);
            
            // Tail needs to be transformed too.
            // We can approximate by taking the relative distance of tail from body and scaling Y
            float relTailX = a.CapeTail.X - a.FX;
            float relTailY = a.CapeTail.Y - a.FY;
            
            int tailDx = (int)(relTailX * _scaleX); // X not compressed
            int tailDy = (int)(relTailY * _scaleY * heightScale); // Y compressed
            
            int tailX = anchorX + tailDx;
            int tailY = anchorY + (isReflection ? -tailDy : tailDy);
            
            // Midpoint logic
            int midX = (startX + tailX) / 2;
            int midY = (startY + tailY) / 2;
            
            // Sag calculation (Gravity)
            int sag = (int)(1.0f * _scaleY * heightScale);
            midY += isReflection ? -sag : sag; // Invert gravity for reflection

            // Draw Bezier Curve
            int steps = 10;
            string color = isReflection ? Palette.White : Palette.Red;

            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                float invT = 1f - t;
                
                float lx = (invT * invT * startX) + (2 * invT * t * midX) + (t * t * tailX);
                float ly = (invT * invT * startY) + (2 * invT * t * midY) + (t * t * tailY);

                int cx = (int)lx;
                int cy = (int)ly;
                
                if (isReflection)
                {
                     // Re-project reflection?
                     // The maths above already handled reflection via startY/tailY flip logic
                     // But we need to check bounds
                     if (cy >= Height) continue;
                }

                char c = ' ';
                if (t < 0.3) c = '}'; 
                else if (t < 0.6) c = ')';
                else c = '›';
                
                float slope = (ly - startY) / (lx - startX + 0.001f);
                if (Math.Abs(slope) > 1.5) c = '│'; 
                else if (slope > 0.5) c = '╲';
                else if (slope < -0.5) c = '╱';
                
                _term.Draw(cx, cy, c, color);
                // Thickener
                _term.Draw(cx, cy+1, ':', color);
            }
        }

        private void DrawLine(int x0, int y0, int x1, int y1, char c, string color)
        {
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy, e2; 
            
            while (true)
            {
                _term.Draw(x0, y0, c, color);
                if (x0 == x1 && y0 == y1) break;
                e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }
    }
}
