using System;
using System.Collections.Generic;

namespace StarWarsAnimation
{
    public class ParticleManager
    {
        public List<Spark> Sparks { get; private set; } = new();
        public List<Spark> Smoke { get; private set; } = new();
        public List<LightningBolt> LightningBolts { get; private set; } = new();
        public List<ScorchMark> ScorchMarks { get; private set; } = new();

        private readonly Random _rng = new Random();
        private const float LogicWidth = Constants.LogicWidth;

        public void Update(float dt)
        {
            UpdateSparks(dt);
            UpdateSmoke(dt);
            UpdateLightning(dt);
            UpdateScorchMarks(dt);
        }

        public void SpawnSparks(int x, int y, int count)
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

        public void SpawnSmoke(int x, int y, int count)
        {
            if (x < 0 || x > LogicWidth) return;

            for (int i = 0; i < count; i++)
            {
                Smoke.Add(new Spark
                {
                    Pos = new Vec2(x + (float)(_rng.NextDouble() * 2 - 1), y),
                    Vel = new Vec2((float)(_rng.NextDouble() * 1 - 0.5), (float)(_rng.NextDouble() * 2 + 1)),
                    Life = (float)(_rng.NextDouble() * 2.0 + 1.0)
                });
            }
        }

        public void AddScorchMark(Vec2 pos)
        {
            if (pos.X > 0 && pos.X < LogicWidth)
            {
                ScorchMarks.Add(new ScorchMark { Pos = pos, Age = 0f });
            }
        }

        public void AddLightning(LightningBolt bolt)
        {
            LightningBolts.Add(bolt);
        }

        private void UpdateScorchMarks(float dt)
        {
            for (int i = 0; i < ScorchMarks.Count; i++)
            {
                var m = ScorchMarks[i];
                m.Age += dt;
                ScorchMarks[i] = m;
            }
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

        private void UpdateSmoke(float dt)
        {
            for (int i = Smoke.Count - 1; i >= 0; i--)
            {
                var s = Smoke[i];
                s.Pos.Y -= s.Vel.Y * dt;
                s.Pos.X += s.Vel.X * dt;
                s.Life -= dt;
                Smoke[i] = s;

                if (s.Life <= 0 || s.Pos.X < 0 || s.Pos.X > LogicWidth) Smoke.RemoveAt(i);
            }

            // Ambient smoke from scorch marks
            if (_rng.NextDouble() > 0.9)
            {
                foreach (var sm in ScorchMarks)
                {
                    if (_rng.NextDouble() > 0.95)
                        SpawnSmoke((int)sm.Pos.X, (int)sm.Pos.Y, 1);
                }
            }
        }

        private void UpdateLightning(float dt)
        {
            for (int i = LightningBolts.Count - 1; i >= 0; i--)
            {
                LightningBolts[i].Life -= dt;
                if (LightningBolts[i].Life <= 0) LightningBolts.RemoveAt(i);
            }
        }
    }
}
