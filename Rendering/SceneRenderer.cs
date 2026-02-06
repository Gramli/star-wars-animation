using StarWarsAnimation.Core;
using StarWarsAnimation.Simulation;

namespace StarWarsAnimation.Rendering
{
    public class SceneRenderer
    {
        private const float LogicWidth = Constants.LogicWidth;
        private const float LogicHeight = Constants.LogicHeight;

        private readonly TerminalRenderer _term;
        private readonly EnvironmentRenderer _envRenderer;
        private readonly ActorRenderer _actorRenderer;
        private readonly EffectRenderer _effectRenderer;
        
        private readonly float _scaleX;
        private readonly float _scaleY;

        public int Width => _term.Width;
        public int Height => _term.Height;

        public SceneRenderer()
        {
            _term = new TerminalRenderer();
            _scaleX = _term.Width / LogicWidth;
            _scaleY = _term.Height / LogicHeight;

            _envRenderer = new EnvironmentRenderer(_term, _scaleX, _scaleY);
            _actorRenderer = new ActorRenderer(_term, _scaleX, _scaleY);
            _effectRenderer = new EffectRenderer(_term);
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
            var (shakeX, shakeY) = RendererUtils.GetShakeOffset(sim.ShakeScreen);
            var transform = new CameraTransform(_scaleX, _scaleY, LogicWidth, LogicHeight, sim.ZoomLevel, sim.CameraAngle, sim.CameraFocus, shakeX, shakeY);

            // Draw Stars in Space Background
            _envRenderer.DrawStars(sim.Stars, transform);

            // Draw Background & Environment
            _envRenderer.DrawBackground(transform);
            _envRenderer.DrawWallDamage(sim, transform);

            // Calculate Scales
            float heightScale = (float)Math.Cos(transform.Angle);
            float widthScale = RendererUtils.CalculateWidthScale(heightScale);

            // Draw Floor/YinYang
            _envRenderer.DrawYinYangCarpets(sim, transform.Angle, transform, widthScale, heightScale);

            // Draw Effects (Bottom Layer)
            _effectRenderer.DrawScorchMarks(sim.ScorchMarks, transform);
            _effectRenderer.DrawDebris(sim.DebrisChunks, transform);

            // Shadows
            var jediPos = transform.ToScreen(sim.Jedi.FX, sim.Jedi.FY);
            var sithPos = transform.ToScreen(sim.Sith.FX, sim.Sith.FY);
            int floorY = (int)(15 * _scaleY) + shakeY; 
            _actorRenderer.DrawShadows(transform.Angle, floorY, jediPos, sithPos);

            // Calculate offsets for actors (Handling Shake & Zoom)
            int jediOx = jediPos.sx - (int)(sim.Jedi.FX * _scaleX);
            int jediOy = jediPos.sy - (int)(sim.Jedi.FY * _scaleY);
            int sithOx = sithPos.sx - (int)(sim.Sith.FX * _scaleX);
            int sithOy = sithPos.sy - (int)(sim.Sith.FY * _scaleY);

            // Reflections (Only if not top down)
            if (transform.Angle < 0.5f)
            {
                 _actorRenderer.DrawActor(sim.Jedi, false, transform, widthScale, heightScale, true, floorY, jediOx, jediOy);
                 _actorRenderer.DrawActor(sim.Sith, true, transform, widthScale, heightScale, true, floorY, sithOx, sithOy);
            }

            // Motion Blur
            if (Math.Abs(sim.Jedi.FX - sim.Jedi.PrevFX) > 0.5f)
            {
                 var prevJ = transform.ToScreen(sim.Jedi.PrevFX, sim.Jedi.PrevFY);
                 int pjOx = prevJ.sx - (int)(sim.Jedi.PrevFX * _scaleX);
                 int pjOy = prevJ.sy - (int)(sim.Jedi.PrevFY * _scaleY);
                 _actorRenderer.DrawActor(sim.Jedi, false, transform, widthScale, heightScale, false, floorY, pjOx, pjOy, true);
            }
            if (Math.Abs(sim.Sith.FX - sim.Sith.PrevFX) > 0.5f)
            {
                 var prevS = transform.ToScreen(sim.Sith.PrevFX, sim.Sith.PrevFY);
                 int psOx = prevS.sx - (int)(sim.Sith.PrevFX * _scaleX);
                 int psOy = prevS.sy - (int)(sim.Sith.PrevFY * _scaleY);
                 _actorRenderer.DrawActor(sim.Sith, true, transform, widthScale, heightScale, false, floorY, psOx, psOy, true);
            }

            // Actors (Main) Pass 1 - To determine tip position for lighting
            var jediTip = _actorRenderer.DrawActor(sim.Jedi, false, transform, widthScale, heightScale, false, floorY, jediOx, jediOy);
            var sithTip = _actorRenderer.DrawActor(sim.Sith, true, transform, widthScale, heightScale, false, floorY, sithOx, sithOy);

            // Dynamic Lighting (Restore Glow)
            if (sim.Jedi.SaberActive && jediTip.HasValue)
                _term.ApplyLighting(jediTip.Value.x, jediTip.Value.y, 12, Palette.Blue);
            
            if (sim.Sith.SaberActive && sithTip.HasValue)
                _term.ApplyLighting(sithTip.Value.x, sithTip.Value.y, 12, Palette.Red);

            // Redraw Actors (Pass 2 - Overlay on top of lighting with clean colors)
            _actorRenderer.DrawActor(sim.Jedi, false, transform, widthScale, heightScale, false, floorY, jediOx, jediOy);
            _actorRenderer.DrawActor(sim.Sith, true, transform, widthScale, heightScale, false, floorY, sithOx, sithOy);

            // Sabers
            _actorRenderer.DrawSaberArc(sim.Jedi, jediTip, jediOx, jediOy, Palette.Blue, widthScale, heightScale);
            _actorRenderer.DrawSaberArc(sim.Sith, sithTip, sithOx, sithOy, Palette.Red, widthScale, heightScale);

            // Effects (Top Layer)
            _effectRenderer.DrawLightning(sim.LightningBolts, transform);
            _effectRenderer.DrawSparks(sim.Sparks, transform);
            _effectRenderer.DrawSmoke(sim.Smoke, transform);

            // Lighting/Darkness
            _effectRenderer.ApplyDarkness(sim, jediTip, sithTip, transform);
        }
    }
}
