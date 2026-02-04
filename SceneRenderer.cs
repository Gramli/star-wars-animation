using System;

namespace StarWarsAnimation;

public class SceneRenderer
{
    private const float LogicWidth = 80f;
    private const float LogicHeight = 25f;

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
        float scrollY = sim.Time * 5.0f; 
        float startY = _term.Height; 
        
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            float worldY = startY + (i * 2) - scrollY; 
            
            if (worldY > _term.Height || worldY < 0) continue;
            
            int screenY = (int)worldY;
            int centerX = _term.Width / 2;
            int len = line.Length;
            int drawX = centerX - (len / 2);
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

        // Transform function: World -> Screen with Zoom & Pan
        (int sx, int sy) Transform(float wx, float wy)
        {
            // Center relative to camera focus
            float relX = (wx - camX) * zoom;
            float relY = (wy - camY) * zoom;
            
            // Project back to screen center
            int screenX = (int)((relX + LogicWidth/2) * _scaleX) + shakeX;
            int screenY = (int)((relY + LogicHeight/2) * _scaleY) + shakeY;
            return (screenX, screenY);
        }

        // Stars (Parallax - unaffected by zoom to feel distant)
        foreach (var s in sim.Stars)
        {
            int sx = (int)(s.X * _scaleX);
            int sy = (int)(s.Y * _scaleY);
            if (sx >= 0 && sx < _term.Width && sy >= 0 && sy < _term.Height)
            {
                if (_bgChars[sy, sx] == ' ')
                    _term.Draw(sx, sy, '.', Palette.Dim);
            }
        }

        // Background (Static architecture, but needs to zoom?)
        // To keep it simple, we only zoom actors/sparks/debris. Background stays as "set".
        // Actually, if actors zoom but walls don't, it looks weird.
        // Let's re-render BG chars with transform? No, expensive.
        // Compromise: Background is fixed "wide shot", Action is zoomed? No.
        // Solution: Keep BG fixed for now as "Architecture" (it's the room), 
        // but maybe we can just zoom the "Action Layer".
        // A true camera zoom requires re-sampling the world. 
        // For CLI, we'll just keep BG static (it's a big room) and Actors move.
        // Wait, that looks like they grow giant.
        // Okay, simpler: "Zoom" just means they get closer to each other? No.
        // Let's stick to standard rendering but apply shake. Zoom is too complex to resample bgChars.
        
        // REVISION: Only Shake applied globally. Zoom applied to positions only?
        // Let's try to just apply Shake to everything first.
        
        // Background
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

        // Wall Damage (Hole)
        if (sim.WallDamaged)
        {
            DrawWallDamage(shakeX, shakeY);
        }

        // Permanent Debris (Scorch marks)
        foreach (var d in sim.ScorchMarks)
        {
            int dx = (int)(d.X * _scaleX) + shakeX;
            int dy = (int)(d.Y * _scaleY) + shakeY; 
            _term.Draw(dx, dy, '░', Palette.Dim); 
        }

        // Flying Debris Chunks
        foreach (var chunk in sim.DebrisChunks)
        {
            if (!chunk.Active) continue;
            int cx = (int)(chunk.Pos.X * _scaleX) + shakeX;
            int cy = (int)(chunk.Pos.Y * _scaleY) + shakeY;
            _term.Draw(cx, cy, chunk.Char, Palette.White);
        }

        // Helper to draw actors with zoom (simulated by just positioning? No, scale matters)
        // If we can't scale the sprite (unicode), we can't "Zoom".
        // CLI Zoom is impossible without changing sprites.
        // SO: We will interpret "Zoom" as "Focus". 
        // We will implement Shake and Motion Blur instead of Zoom.

        // Reflections
        DrawActor(sim.Jedi, false, true, floorY, shakeX, shakeY);
        DrawActor(sim.Sith, true, true, floorY, shakeX, shakeY);

        // Motion Blur (Draw previous position faintly)
        if (Math.Abs(sim.Jedi.FX - sim.Jedi.PrevFX) > 0.5f)
             DrawActor(sim.Jedi, false, false, floorY, shakeX, shakeY, true);
        if (Math.Abs(sim.Sith.FX - sim.Sith.PrevFX) > 0.5f)
             DrawActor(sim.Sith, true, false, floorY, shakeX, shakeY, true);

        // Actors
        var jediTip = DrawActor(sim.Jedi, false, false, floorY, shakeX, shakeY);
        var sithTip = DrawActor(sim.Sith, true, false, floorY, shakeX, shakeY);

        // ... (Lighting & Rest same as before, just add shake offset)
        // Dynamic Lighting
        if (sim.Jedi.SaberActive && jediTip.HasValue)
            _term.ApplyLighting(jediTip.Value.x, jediTip.Value.y, 12, Palette.Blue);
        
        if (sim.Sith.SaberActive && sithTip.HasValue)
            _term.ApplyLighting(sithTip.Value.x, sithTip.Value.y, 12, Palette.Red);

        // Force Lightning
        foreach (var bolt in sim.LightningBolts)
        {
            for (int i = 0; i < bolt.Points.Count - 1; i++)
            {
                var p1 = bolt.Points[i];
                var p2 = bolt.Points[i+1];
                
                int x0 = (int)(p1.X * _scaleX) + shakeX;
                int y0 = (int)(p1.Y * _scaleY) + shakeY;
                int x1 = (int)(p2.X * _scaleX) + shakeX;
                int y1 = (int)(p2.Y * _scaleY) + shakeY;
                
                DrawLine(x0, y0, x1, y1, '+', Palette.White);
                _term.ApplyLighting(x0, y0, 6, Palette.Cyan);
            }
        }

        // Sparks
        foreach (var s in sim.Sparks)
        {
            int sx = (int)(s.Pos.X * _scaleX) + shakeX;
            int sy = (int)(s.Pos.Y * _scaleY) + shakeY;
            _term.Draw(sx, sy, '*', Palette.Yellow);
        }

        // Smoke
        foreach (var s in sim.Smoke)
        {
             int sx = (int)(s.Pos.X * _scaleX) + shakeX;
             int sy = (int)(s.Pos.Y * _scaleY) + shakeY;
             char c = s.Life > 1.0f ? '▒' : '░';
             _term.Draw(sx, sy, c, Palette.Dim);
        }

        // Apply Darkness (Blackout Mode) using local shake context
        if (sim.IsDarkness)
        {
            var lights = new List<(int x, int y, int r)>();
            
            // Jedi Saber
            if (sim.Jedi.SaberActive && jediTip.HasValue) 
            {
                lights.Add((jediTip.Value.x, jediTip.Value.y, 10));
                // Add actor center
                int ax = (int)(sim.Jedi.FX * _scaleX) + shakeX;
                int ay = (int)(sim.Jedi.FY * _scaleY) + shakeY;
                lights.Add((ax, ay, 5));
            }
            
            // Sith Saber
            if (sim.Sith.SaberActive && sithTip.HasValue) 
            {
                lights.Add((sithTip.Value.x, sithTip.Value.y, 10));
                int ax = (int)(sim.Sith.FX * _scaleX) + shakeX;
                int ay = (int)(sim.Sith.FY * _scaleY) + shakeY;
                lights.Add((ax, ay, 5));
            }
            
            // Sparks
            foreach (var s in sim.Sparks)
            {
                 int sx = (int)(s.Pos.X * _scaleX) + shakeX;
                 int sy = (int)(s.Pos.Y * _scaleY) + shakeY;
                 lights.Add((sx, sy, 4));
            }
            
            // Lightning
             foreach (var bolt in sim.LightningBolts)
            {
                if (bolt.Points.Count > 0)
                {
                    // Light up along the bolt? Just use center for now to save perf
                    var p = bolt.Points[bolt.Points.Count / 2];
                    int bx = (int)(p.X * _scaleX) + shakeX;
                    int by = (int)(p.Y * _scaleY) + shakeY;
                    lights.Add((bx, by, 8));
                }
            }
            
            _term.ApplyDarkness(lights);
        }

        // Smoke
        foreach (var s in sim.Smoke)
        {
             int sx = (int)(s.Pos.X * _scaleX) + shakeX;
             int sy = (int)(s.Pos.Y * _scaleY) + shakeY;
             char c = s.Life > 1.0f ? '▒' : '░';
             _term.Draw(sx, sy, c, Palette.Dim);
        }

        // Apply Darkness (Blackout Mode) using local shake context
        if (sim.IsDarkness)
        {
            var lights = new List<(int x, int y, int r)>();
            
            // Jedi Saber
            if (sim.Jedi.SaberActive && jediTip.HasValue) 
            {
                lights.Add((jediTip.Value.x, jediTip.Value.y, 10));
                // Add actor center
                int ax = (int)(sim.Jedi.FX * _scaleX) + shakeX;
                int ay = (int)(sim.Jedi.FY * _scaleY) + shakeY;
                lights.Add((ax, ay, 5));
            }
            
            // Sith Saber
            if (sim.Sith.SaberActive && sithTip.HasValue) 
            {
                lights.Add((sithTip.Value.x, sithTip.Value.y, 10));
                int ax = (int)(sim.Sith.FX * _scaleX) + shakeX;
                int ay = (int)(sim.Sith.FY * _scaleY) + shakeY;
                lights.Add((ax, ay, 5));
            }
            
            // Sparks
            foreach (var s in sim.Sparks)
            {
                 int sx = (int)(s.Pos.X * _scaleX) + shakeX;
                 int sy = (int)(s.Pos.Y * _scaleY) + shakeY;
                 lights.Add((sx, sy, 4));
            }
            
            // Lightning
             foreach (var bolt in sim.LightningBolts)
            {
                if (bolt.Points.Count > 0)
                {
                    // Light up along the bolt? Just use center for now to save perf
                    var p = bolt.Points[bolt.Points.Count / 2];
                    int bx = (int)(p.X * _scaleX) + shakeX;
                    int by = (int)(p.Y * _scaleY) + shakeY;
                    lights.Add((bx, by, 8));
                }
            }
            
            _term.ApplyDarkness(lights);
        }
    }

    private (int x, int y)? DrawActor(Actor actor, bool isSith, bool isReflection = false, int floorY = 0, int ox = 0, int oy = 0, bool isBlur = false)
    {
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

        // Draw Cape (Sith only, not blur, not pose 4)
        if (isSith && actor.PoseIndex != 4 && !isBlur) DrawCape(actor);

        // Handle Kneel Special Case (Simplified in Pose, but drawing order matters)
        if (actor.PoseIndex == 4)
        {
            _term.Draw(x + (pose.Head.Dx * dir), y + (isReflection ? -pose.Head.Dy : pose.Head.Dy), pose.Head.Char, mainColor);
            _term.Draw(x, y - (isReflection ? -1 : 1), isReflection ? '▀' : '▄', mainColor);
            return null;
        }

        // Draw Parts
        DrawPart(x, y, dir, pose.LegL, isReflection, mainColor);
        DrawPart(x, y, dir, pose.LegR, isReflection, mainColor);
        DrawPart(x, y, dir, pose.TorsoBottom, isReflection, mainColor);
        DrawPart(x, y, dir, pose.TorsoTop, isReflection, mainColor);
        DrawPart(x, y, dir, pose.ArmL, isReflection, mainColor);
        if (actor.PoseIndex != 3) DrawPart(x, y, dir, pose.ArmR, isReflection, mainColor);

        _term.Draw(x + (pose.Head.Dx * dir), y + (isReflection ? -pose.Head.Dy : pose.Head.Dy), pose.Head.Char, mainColor);

        // Draw Saber
        (int x, int y)? tipPos = null;
        if (actor.SaberActive)
        {
            int sx = x + (pose.Hand.Dx * dir);
            int sy = y + (isReflection ? -pose.Hand.Dy : pose.Hand.Dy);
            char bChar = pose.Blade.Char;
            
            if (dir == -1)
            {
                if (bChar == '╱') bChar = '╲';
                else if (bChar == '╲') bChar = '╱';
            }

            if (isReflection)
            {
                if (bChar == '╱') bChar = '╲';
                else if (bChar == '╲') bChar = '╱';
            }

            int bdy = isReflection ? -pose.Blade.Dy : pose.Blade.Dy;

            for (int k = 1; k <= actor.SaberLength; k++)
            {
                int bx = sx + (pose.Blade.Dx * dir * k);
                int by = sy + (bdy * k);
                _term.Draw(bx, by, bChar, saberColor);
                
                if (k == actor.SaberLength) tipPos = (bx, by);
            }
        }
        return tipPos;
    }

    private void DrawWallDamage(int ox, int oy)
    {
        // Hole at right wall (approx x=74..80, y=4..14)
        // We draw "void" (spaces) and "rubble edges"
        
        for (int y = 4; y <= 14; y++)
        {
            for (int x = 74; x < 80; x++)
            {
                int sx = (int)(x * _scaleX) + ox;
                int sy = (int)(y * _scaleY) + oy;

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

    private void DrawCape(Actor a)
    {
        // 1. Anchor point (Shoulders)
        float startX = a.FX - (a.FacingRight ? 1 : -1);
        float startY = a.FY - 3; // Shoulders
        
        // 2. Control Point (Mid-back / wind influence)
        // We use Bezier curve for smooth flow
        float tailX = a.CapeTail.X;
        float tailY = a.CapeTail.Y;

        float midX = (startX + tailX) / 2;
        float midY = (startY + tailY) / 2;
        
        // Add "sag" or "lift" based on movement
        midY += 1.0f; 

        // Draw Bezier Curve
        int steps = 10;
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            float invT = 1f - t;
            
            // Quadratic Bezier: B(t) = (1-t)^2 * P0 + 2(1-t)t * P1 + t^2 * P2
            float lx = (invT * invT * startX) + (2 * invT * t * midX) + (t * t * tailX);
            float ly = (invT * invT * startY) + (2 * invT * t * midY) + (t * t * tailY);

            int cx = (int)(lx * _scaleX);
            int cy = (int)(ly * _scaleY);

            char c = ' ';
            // Textured cape - using softer, flowy characters
            if (t < 0.3) c = '}'; 
            else if (t < 0.6) c = ')';
            else c = '›';
            
            // Adjust character based on slope for better shape
            float slope = (ly - startY) / (lx - startX + 0.001f);
            if (Math.Abs(slope) > 1.5) c = '│'; // Vertical hanging
            else if (slope > 0.5) c = '╲';
            else if (slope < -0.5) c = '╱';
            
            _term.Draw(cx, cy, c, Palette.Dim);
            
            // Draw a second layer for thickness/width (Shadow)
            _term.Draw(cx, cy+1, ':', Palette.Dim);
        }
    }

    private void DrawPart(int cx, int cy, int dir, BodyPart part, bool reflect, string color)
    {
        char c = part.Char;
        if (reflect)
        {
            if (c == '╱') c = '╲';
            else if (c == '╲') c = '╱';
            else if (c == '▄') c = '▀';
            else if (c == '▀') c = '▄';
        }
        _term.Draw(cx + (part.Dx * dir), cy + (reflect ? -part.Dy : part.Dy), c, color);
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
