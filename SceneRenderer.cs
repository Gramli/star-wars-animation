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

                // Floor (Two Lines)
                if (y == floorY) c = '═';
                else if (y == floorY + 1) c = '=';
                // Under ground: Empty (Clear previous detailed plating)
                else if (y > floorY + 1) c = ' ';
                
                // Above ground: Restore Walls & Windows
                else if (y == 0) c = '═';
                else if (y == 1) c = '-';
                else
                {
                    float px = (float)x / w;
                    if (px < 0.1f || px > 0.9f)
                    {
                        if (x % 4 == 0) c = '║';
                        else if (y % 5 == 0) c = '=';
                    }
                    else if ((px >= 0.1f && px < 0.12f) || (px <= 0.9f && px > 0.88f))
                    {
                        c = '║';
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
        else
        {
            RenderScene(sim);
        }
        _term.Present();
    }

    private void RenderScene(DuelSimulation sim)
    {
        // Stars
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

        // Background
        for (int y = 0; y < _term.Height; y++)
        {
            for (int x = 0; x < _term.Width; x++)
            {
                char c = _bgChars[y, x];
                if (c != ' ') _term.Draw(x, y, c, _bgColors[y, x]);
            }
        }

        DrawActor(sim.Jedi, false);
        DrawActor(sim.Sith, true);

        // Sparks
        foreach (var s in sim.Sparks)
        {
            int sx = (int)(s.Pos.X * _scaleX);
            int sy = (int)(s.Pos.Y * _scaleY);
            _term.Draw(sx, sy, '*', Palette.Yellow);
        }
    }

    private void DrawActor(Actor actor, bool isSith)
    {
        int x = (int)(actor.FX * _scaleX);
        int y = (int)(actor.FY * _scaleY);
        int dir = actor.FacingRight ? 1 : -1;

        var head = (dx: 0, dy: -4, c: 'Q');
        var torsoT = (dx: 0, dy: -3, c: '|');
        var torsoB = (dx: 0, dy: -2, c: '|');
        var legL = (dx: -1, dy: -1, c: '/');
        var legR = (dx: 1, dy: -1, c: '\\');
        var armL = (dx: -1, dy: -3, c: '/');
        var armR = (dx: 1, dy: -3, c: '\\');
        var hand = (dx: 1, dy: -3);
        var blade = (dx: 0, dy: -1, c: '|');

        if (isSith) head.c = '@';

        switch (actor.PoseIndex)
        {
            case 0: // Idle
                // If Sith, use sloping stance instead of vertical
                if (isSith)
                {
                    armR = (1, -3, '\\');
                    hand = (1, -3);
                    blade = (1, 1, '\\'); // Sloping down and out
                }
                break;
            case 1: // Attack
                head = (2, -4, head.c); torsoT = (1, -3, '|');
                legL = (-2, -1, '/'); legR = (2, -1, '\\');
                armL = (-1, -3, '\\'); armR = (2, -4, '/');
                hand = (2, -4); blade = (1, -1, '/');
                break;
            case 2: // Guard
                head = (-1, -4, head.c); torsoT = (-1, -3, '|');
                legL = (-2, -1, '/'); legR = (1, -1, '\\');
                armR = (0, -3, '|');
                hand = (0, -3); blade = (-1, -1, '\\');
                break;
            case 3: // Lock
                head = (1, -4, head.c); torsoT = (1, -3, '|');
                legL = (-2, -1, '/'); legR = (2, -1, '\\');
                armR = (1, -3, '-');
                hand = (1, -3); blade = (1, 0, '-');
                break;
            case 4: // Kneel
                head = (1, -2, 'o'); torsoT = (0, -1, '_'); torsoB = (0, 0, ' ');
                legL = (0, -1, '_'); legR = (2, -1, '_');
                armL = (0, 0, ' '); armR = (0, 0, ' ');
                break;
            case 5: // Anticipation
                head = (-1, -4, head.c); torsoT = (-1, -3, '|');
                armR = (-2, -4, '\\');
                hand = (-2, -4); blade = (-1, -1, '\\');
                break;
            case 6: // Force Push
                head = (0, -4, head.c); torsoT = (0, -3, '|');
                armL = (2, -3, '-');
                armR = (-1, -2, '\\');
                hand = (-1, -2); blade = (1, 1, '\\');
                break;
            case 7: // Stagger
                head = (-2, -4, head.c); torsoT = (-1, -3, '\\');
                legL = (-2, -1, '/'); legR = (1, -1, '/');
                armL = (-2, -3, '/'); armR = (0, -3, '\\');
                hand = (0, -3); blade = (-1, -1, '\\');
                break;
            case 8: // Jump
                head = (1, -4, head.c); torsoT = (0, -3, '|');
                legL = (-1, -2, '-'); legR = (1, -2, '-');
                armL = (-1, -3, '/'); armR = (1, -3, '\\');
                hand = (1, -3); blade = (-1, -1, '/');
                break;
            case 9: // Suspended
                head = (0, -4, head.c); torsoT = (0, -3, '|');
                legL = (-1, -1, '|'); legR = (1, -1, '|');
                armL = (-2, -3, '\\'); armR = (2, -3, '/');
                hand = (2, -3); blade = (0, -1, '|');
                break;
            case 10: // Dash
                head = (3, -3, head.c); torsoT = (2, -2, '-'); torsoB = (0, -2, '-');
                legL = (-2, -2, '='); legR = (-1, -2, '=');
                armL = (0, -2, '-'); armR = (3, -2, '-');
                hand = (3, -2); blade = (1, 0, '-');
                break;
            case 11: // Crouch/Prep
                head = (0, -2, head.c); torsoT = (0, -1, '|'); torsoB = (0, 0, ' ');
                legL = (-1, 0, '_'); legR = (1, 0, '_');
                armL = (-1, -1, '/'); armR = (1, -1, '\\');
                hand = (1, -1); blade = (1, -1, '/');
                break;
            case 12: // Reach Hilt
                armR = (-1, -2, '/');
                hand = (-1, -2);
                break;
        }

        if (isSith && actor.PoseIndex != 4) DrawCape(actor);

        if (actor.PoseIndex == 4)
        {
            _term.Draw(x + (head.dx * dir), y + head.dy, head.c, Palette.White);
            _term.Draw(x, y - 1, '_', Palette.White);
            return;
        }

        DrawPart(x, y, dir, legL);
        DrawPart(x, y, dir, legR);
        DrawPart(x, y, dir, torsoB);
        DrawPart(x, y, dir, torsoT);
        DrawPart(x, y, dir, armL);
        if (actor.PoseIndex != 3) DrawPart(x, y, dir, armR);

        _term.Draw(x + (head.dx * dir), y + head.dy, head.c, Palette.White);

        if (actor.SaberActive)
        {
            int sx = x + (hand.dx * dir);
            int sy = y + hand.dy;
            char bChar = blade.c;
            if (dir == -1)
            {
                if (bChar == '/') bChar = '\\';
                else if (bChar == '\\') bChar = '/';
            }
            for (int k = 1; k <= actor.SaberLength; k++)
                _term.Draw(sx + (blade.dx * dir * k), sy + (blade.dy * k), bChar, actor.Color);
        }
    }

    private void DrawCape(Actor a)
    {
        float startX = a.FX - (a.FacingRight ? 1 : -1);
        float startY = a.FY - 3;
        float dist = (float)Math.Sqrt(Math.Pow(a.CapeTail.X - startX, 2) + Math.Pow(a.CapeTail.Y - startY, 2));
        int steps = (int)dist + 1;

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            float lx = startX + (a.CapeTail.X - startX) * t;
            float ly = startY + (a.CapeTail.Y - startY) * t;

            int cx = (int)(lx * _scaleX);
            int cy = (int)(ly * _scaleY);

            char c = '~';
            if (t > 0.8) c = '.';
            _term.Draw(cx, cy, c, Palette.Dim);
        }
    }

    private void DrawPart(int cx, int cy, int dir, (int dx, int dy, char c) part)
    {
        _term.Draw(cx + (part.dx * dir), cy + part.dy, part.c, Palette.White);
    }
}
