using StarWarsAnimation.Core;
using StarWarsAnimation.Entities;
using StarWarsAnimation.Simulation;

namespace StarWarsAnimation.Rendering
{
    public class EffectRenderer
    {
        private readonly TerminalRenderer _term;

        public EffectRenderer(TerminalRenderer term)
        {
            _term = term;

        }

        public void DrawScorchMarks(IEnumerable<ScorchMark> scorchMarks, CameraTransform transform)
        {
            foreach (var d in scorchMarks)
            {
                var pos = transform.ToScreen(d.Pos.X, d.Pos.Y);

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
        }

        public void DrawDebris(IEnumerable<DebrisChunk> debrisChunks, CameraTransform transform)
        {
            foreach (var chunk in debrisChunks)
            {
                if (!chunk.Active) continue;
                var pos = transform.ToScreen(chunk.Pos.X, chunk.Pos.Y);
                _term.Draw(pos.sx, pos.sy, chunk.Char, Palette.White);
            }
        }

        public void DrawLightning(IEnumerable<LightningBolt> bolts, CameraTransform transform)
        {
            foreach (var bolt in bolts)
            {
                for (int i = 0; i < bolt.Points.Count - 1; i++)
                {
                    var p1 = bolt.Points[i];
                    var p2 = bolt.Points[i + 1];

                    var t1 = transform.ToScreen(p1.X, p1.Y);
                    var t2 = transform.ToScreen(p2.X, p2.Y);

                    _term.DrawLine(t1.sx, t1.sy, t2.sx, t2.sy, '+', Palette.White);
                    _term.ApplyLighting(t1.sx, t1.sy, 6, Palette.Cyan);
                }
            }
        }

        public void DrawSparks(IEnumerable<Spark> sparks, CameraTransform transform)
        {
            foreach (var s in sparks)
            {
                var pos = transform.ToScreen(s.Pos.X, s.Pos.Y);
                _term.Draw(pos.sx, pos.sy, '*', Palette.Yellow);
            }
        }

        public void DrawSmoke(IEnumerable<Spark> smoke, CameraTransform transform)
        {
            foreach (var s in smoke)
            {
                var pos = transform.ToScreen(s.Pos.X, s.Pos.Y);
                char c = s.Life > 1.0f ? '▒' : '░';
                _term.Draw(pos.sx, pos.sy, c, Palette.Dim);
            }
        }

        public void ApplyDarkness(DuelSimulation sim, (int x, int y)? jediTip, (int x, int y)? sithTip, CameraTransform transform)
        {
            if (!sim.IsDarkness)
            {
                return;
            }

            var lights = new List<(int x, int y, int r)>();

            if (sim.Jedi.SaberActive && jediTip.HasValue)
            {
                lights.Add((jediTip.Value.x, jediTip.Value.y, 10));
                var pos = transform.ToScreen(sim.Jedi.FX, sim.Jedi.FY);
                lights.Add((pos.sx, pos.sy, 5));
            }

            if (sim.Sith.SaberActive && sithTip.HasValue)
            {
                lights.Add((sithTip.Value.x, sithTip.Value.y, 10));
                var pos = transform.ToScreen(sim.Sith.FX, sim.Sith.FY);
                lights.Add((pos.sx, pos.sy, 5));
            }

            foreach (var s in sim.Sparks)
            {
                var pos = transform.ToScreen(s.Pos.X, s.Pos.Y);
                lights.Add((pos.sx, pos.sy, 4));
            }

            foreach (var bolt in sim.LightningBolts)
            {
                if (bolt.Points.Count > 0)
                {
                    var p = bolt.Points[bolt.Points.Count / 2];
                    var pos = transform.ToScreen(p.X, p.Y);
                    lights.Add((pos.sx, pos.sy, 8));
                }
            }

            _term.ApplyDarkness(lights);
        }
    }
}
