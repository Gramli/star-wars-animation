namespace StarWarsAnimation.Entities
{
    public class Actor
    {
        public float FX, FY; // World Position
        public float PrevFX, PrevFY;
        
        public string Color = "";
        public bool SaberActive;
        public int SaberLength;
        public int PoseIndex;
        public int PrevPoseIndex; // For motion trails
        public bool FacingRight;

        // Cape Simulation
        public Vec2 CapeTail;

        public void SaveState()
        {
            PrevFX = FX;
            PrevFY = FY;
            PrevPoseIndex = PoseIndex;
        }

        public void UpdateCape(float dt)
        {
            float anchorX = FX - (FacingRight ? 1.0f : -1.0f); 
            float anchorY = FY - 3.0f;
            float velX = (FX - PrevFX) / dt;
            float velY = (FY - PrevFY) / dt;
            
            // Default hanging position (Gravity)
            float targetX = anchorX - (FacingRight ? 1.5f : -1.5f);
            float targetY = anchorY + 4.0f;

            // Physics: Drag
            targetX -= velX * 0.15f; // Drag against movement
            targetY -= velY * 0.15f; // Drag against jump

            // Force Action Reaction (Wind from power)
            if (PoseIndex == 6) // Casting Force
            {
                 targetX -= (FacingRight ? 4.0f : -4.0f); // Blows back violently
                 targetY -= 2.0f; // Lift
            }
            else if (velY < -5.0f) // Jumping up
            {
                 targetY += 1.0f; // Pull down
            }

            // Smooth Damping
            CapeTail.X += (targetX - CapeTail.X) * 8.0f * dt;
            CapeTail.Y += (targetY - CapeTail.Y) * 8.0f * dt;
        }
    }
}
