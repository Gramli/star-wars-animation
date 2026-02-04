using System;
using System.Collections.Generic;
using System.Text;

namespace StarWarsAnimation
{
    internal sealed class TerminalRenderer
    {
        public const int Width = 80;
        public const int Height = 25;

        struct Pixel
        {
            public char Char;
            public string Color;
        }

        private readonly Pixel[,] _buffer = new Pixel[Height, Width];
        private readonly StringBuilder _sb = new StringBuilder();
        private string _lastColor = "";

        public TerminalRenderer()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;
            Clear();
        }

        public void Clear(string color = "\u001b[30m") // Black background
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    _buffer[y, x].Char = ' ';
                    _buffer[y, x].Color = color;
                }
        }

        public void Fill(char c, string color)
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
                _sb.Append('\n');
            }
            _sb.Append("\u001b[0m"); // Reset
            Console.Write(_sb.ToString());
        }
    }
}
