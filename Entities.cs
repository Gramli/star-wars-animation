namespace StarWarsAnimation;

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

public class Actor
{
    public float FX, FY; // World Position
    public float PrevFX, PrevFY;
    
    public string Color = "";
    public bool SaberActive;
    public int SaberLength;
    public int PoseIndex;
    public bool FacingRight;

    // Cape Simulation
    public Vec2 CapeTail;
}

public class LightningBolt
{
    public System.Collections.Generic.List<Vec2> Points { get; set; } = new();
    public float Life { get; set; }
}

public struct DebrisChunk
{
    public Vec2 Pos;
    public Vec2 Vel;
    public bool Active;
    public char Char; // Visual representation
}
