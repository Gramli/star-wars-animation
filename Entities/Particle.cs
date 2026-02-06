namespace StarWarsAnimation.Entities
{
    public struct Vec2
    {
        public float X, Y;
        public Vec2(float x, float y) { X = x; Y = y; }
    }

    public struct Spark
    {
        public Vec2 Pos;
        public Vec2 Vel;
        public float Life;
    }

    public struct ScorchMark
    {
        public Vec2 Pos;
        public float Age;
    }

    public class LightningBolt
    {
        public List<Vec2> Points { get; set; } = new();
        public float Life { get; set; }
    }

    public struct DebrisChunk
    {
        public Vec2 Pos;
        public Vec2 Vel;
        public bool Active;
        public char Char; // Visual representation
    }
}
