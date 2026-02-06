using StarWarsAnimation.Core;
using System.Text;

namespace StarWarsAnimation.Rendering
{
    public sealed class TerminalRenderer
    {
        public readonly int Width;
        public readonly int Height;

        struct Pixel
        {
            public char Char;
            public string Color;
        }

        private readonly Pixel[,] _buffer;
        private readonly StringBuilder _sb = new StringBuilder();
        private string _lastColor = "";

        public TerminalRenderer()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;

            try 
            {
                // Detect console size
                Width = Console.WindowWidth;
                Height = Console.WindowHeight;

                // Sync buffer to window to prevent scrollbars/wrapping issues
                // This is crucial for stability on Windows consoles
                if (Console.BufferHeight != Height) Console.BufferHeight = Height;
                if (Console.BufferWidth != Width) Console.BufferWidth = Width;
            }
            catch
            {
                // Fallback if access denied or handle invalid
                Width = 80;
                Height = 25;
            }
            
            _buffer = new Pixel[Height, Width];
            Clear();
        }

        public void Clear(string color = "\u001b[30m") // Black background
        {
            FillBuffer(' ', color);
        }

        public void Fill(char c, string color)
        {
            FillBuffer(c, color);
        }

        private void FillBuffer(char c, string color)
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    _buffer[y, x].Char = c;
                    _buffer[y, x].Color = color;
                }
        }

        public void Draw(int x, int y, char c, string color)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                _buffer[y, x].Char = c;
                _buffer[y, x].Color = color;
            }
        }
        
        public void DrawString(int x, int y, string text, string color)
        {
            if (y < 0 || y >= Height) return;
            for (int i = 0; i < text.Length; i++)
            {
                Draw(x + i, y, text[i], color);
            }
        }

        public void DrawLine(int x0, int y0, int x1, int y1, char c, string color)
        {
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy, e2; 
            
            while (true)
            {
                Draw(x0, y0, c, color);
                if (x0 == x1 && y0 == y1) break;
                e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }

        public void ApplyLighting(int lx, int ly, int radius, string color)
        {
            int r2 = radius * radius;
            for (int y = ly - radius; y <= ly + radius; y++)
            {
                if (y < 0 || y >= Height) continue;
                for (int x = lx - radius; x <= lx + radius; x++)
                {
                    if (x < 0 || x >= Width) continue;
                    
                    int dx = x - lx;
                    int dy = y - ly;
                    if (dx*dx + dy*dy <= r2)
                    {
                        // Only light up background (Dim), White (Floor), or Empty
                        if (_buffer[y, x].Color == Palette.Dim || _buffer[y, x].Color == Palette.White)
                        {
                            _buffer[y, x].Color = color;
                        }
                    }
                }
            }
        }

        public void ApplyDarkness(List<(int x, int y, int r)> lights)
        {
            // Reset buffer to empty unless near a light
            // Optimization: If no lights, clear screen?
            // Actually, we need to iterate all pixels to check if they are near ANY light.
            
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    bool lit = false;
                    foreach (var light in lights)
                    {
                        int dx = x - light.x;
                        int dy = y - light.y;
                        if (dx*dx + dy*dy <= light.r * light.r)
                        {
                            lit = true;
                            break;
                        }
                    }

                    if (!lit)
                    {
                        // Mask out
                        _buffer[y, x].Char = ' ';
                    }
                }
            }
        }

        public void Present()
        {
            _sb.Clear();
            _sb.Append("\u001b[H"); // Home cursor

            _lastColor = "";

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var p = _buffer[y, x];
                    if (p.Color != _lastColor)
                    {
                        _sb.Append(p.Color);
                        _lastColor = p.Color;
                    }
                    _sb.Append(p.Char);
                }
                
                // Prevent scrolling by not printing newline on the last row
                if (y < Height - 1)
                {
                    _sb.Append('\n');
                }
            }
            _sb.Append("\u001b[0m"); // Reset
            
            // Write everything at once
            Console.Write(_sb.ToString());
        }
    }
}
