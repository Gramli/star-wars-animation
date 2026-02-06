using System;
using StarWarsAnimation.Core;
using StarWarsAnimation.Entities;
using StarWarsAnimation.Simulation;

namespace StarWarsAnimation.Rendering
{
    public class EnvironmentRenderer
    {
        private readonly TerminalRenderer _term;
        private readonly float _scaleX;
        private readonly float _scaleY;
        private char[,] _bgChars;
        private string[,] _bgColors;

        public EnvironmentRenderer(TerminalRenderer term, float scaleX, float scaleY)
        {
            _term = term;
            _scaleX = scaleX;
            _scaleY = scaleY;
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

        public void DrawBackground(CameraTransform transform)
        {
            if (transform.Angle >= 0.8f)
            {
                return;
            }

            for (int y = 0; y < _term.Height; y++)
            {
                for (int x = 0; x < _term.Width; x++)
                {
                    int drawX = x + transform.ShakeX;
                    int drawY = y + transform.ShakeY;

                    char c = _bgChars[y, x];
                    if (c != ' ') _term.Draw(drawX, drawY, c, _bgColors[y, x]);
                }
            }
        }

        public void DrawWallDamage(DuelSimulation sim, CameraTransform transform)
        {
            if (!sim.WallDamaged || transform.Angle >= 0.5f)
            {
                return;
            }

            // Hole at right wall (approx x=74..80, y=4..14)
            // We draw "void" (spaces) and "rubble edges"
            for (int y = 4; y <= 14; y++)
            {
                for (int x = 74; x < 80; x++)
                {
                    int sx = (int)(x * _scaleX) + transform.ShakeX;
                    int sy = (int)(y * _scaleY) + transform.ShakeY;

                    // Center of the hole is empty/dark
                    bool isCenter = (x > 75 && y > 5 && y < 13);

                    if (isCenter)
                    {
                        // Erase background
                        _term.Draw(sx, sy, ' ', Palette.Dim);
                        // Add some depth details occasionally
                        if ((x + y) % 3 == 0) _term.Draw(sx, sy, '░', Palette.Dim);
                    }
                    else
                    {
                        // Edges - jagged
                        char c = (x + y) % 2 == 0 ? '▙' : '▟';
                        if (y == 4 || y == 14) c = '▄';
                        _term.Draw(sx, sy, c, Palette.Dim);
                    }
                }
            }
        }

        public void DrawYinYangCarpets(DuelSimulation sim, float angle, CameraTransform transform, float widthScale, float heightScale)
        {
            // Draw ONE big Yin-Yang floor at the center of the arena
            // Arena center is at (LogicWidth / 2, FloorY)
            // LogicWidth is 80, so Center X is 40. Floor Level is 15.
            
            var centerPos = transform.ToScreen(40, 15);
            int centerX = centerPos.sx;
            int centerY = centerPos.sy;
            
            int fixedRadiusX = (int)(28 * _scaleX * transform.Zoom);
            int fixedRadiusY = (int)(11 * _scaleY * transform.Zoom);

            int radiusX = fixedRadiusX;
            
            float aspect = (float)Math.Abs(Math.Sin(angle));
            int radiusY = (int)Math.Round(fixedRadiusY * aspect);
            
            if (angle > 0.05f && radiusY == 0) radiusY = 1;

            int innerRadiusX = (int)(11 * _scaleX * transform.Zoom);
            int innerRadiusY = (int)Math.Round(5 * _scaleY * transform.Zoom * aspect);

            DrawYinYangDisk(centerX, centerY, radiusX, radiusY, innerRadiusX, innerRadiusY);
        }



        private void DrawYinYangDisk(int cx, int cy, int radiusX, int radiusY, int innerRadiusX = 0, int innerRadiusY = 0)
        {
            if (radiusX <= 0 || radiusY <= 0) return;

            float rX = radiusX;
            float rY = radiusY;
            float irX = innerRadiusX;
            float irY = innerRadiusY;

            for (int y = -radiusY; y <= radiusY; y++)
            {
                float v = (float)y / rY;
                float val = 1.0f - (v * v);
                if (val < 0) val = 0;
                
                int xLimit = (int)(radiusX * Math.Sqrt(val));
                
                int screenY = cy + y;
                if (screenY < 0 || screenY >= _term.Height) continue;

                for (int x = -xLimit; x <= xLimit; x++)
                {
                    int screenX = cx + x;
                    if (screenX < 0 || screenX >= _term.Width) continue;
                    
                    if (irX > 0 && irY > 0)
                    {
                        float normIX = (float)x / irX;
                        float normIY = (float)y / irY;
                        if ((normIX * normIX) + (normIY * normIY) < 1.0f)
                        {
                            continue;
                        }
                    }

                    float u = (float)x / rX;
                    bool isRed = (x >= 0);
                    
                    float distTopSq = (u * u) + ((v + 0.5f) * (v + 0.5f));
                    float distBotSq = (u * u) + ((v - 0.5f) * (v - 0.5f));
                    
                    if (distTopSq < 0.25f) isRed = true;
                    if (distBotSq < 0.25f) isRed = false;
                    
                    if (distTopSq < 0.02f) isRed = false;
                    if (distBotSq < 0.02f) isRed = true;

                    float distSq = (u * u) + (v * v);
                    char c = '█'; 
                    
                    if (distSq > 0.85f) c = '▓';
                    else if (distSq > 0.95f) c = '▒';
                    
                    if (distTopSq < 0.02f || distBotSq < 0.02f) c = '█';

                    string color = isRed ? Palette.Red : Palette.Blue;
                    _term.Draw(screenX, screenY, c, color);
                }
            }
        }
    }
}
