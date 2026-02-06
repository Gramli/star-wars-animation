using StarWarsAnimation.Entities;

namespace StarWarsAnimation.Rendering
{
    public readonly struct CameraTransform
    {
        private readonly float _scaleX;
        private readonly float _scaleY;
        private readonly float _logicWidth;
        private readonly float _logicHeight;

        public float Zoom { get; }
        public float Angle { get; }
        public float CamX { get; }
        public float CamY { get; }
        public int ShakeX { get; }
        public int ShakeY { get; }

        public CameraTransform(float scaleX, float scaleY, float logicWidth, float logicHeight, float zoom, float angle, Vec2 focus, int shakeX, int shakeY)
        {
            _scaleX = scaleX;
            _scaleY = scaleY;
            _logicWidth = logicWidth;
            _logicHeight = logicHeight;
            Zoom = zoom;
            Angle = angle;
            CamX = focus.X;
            CamY = focus.Y;
            ShakeX = shakeX;
            ShakeY = shakeY;
        }

        public (int sx, int sy) ToScreen(float wx, float wy)
        {
            float relX = (wx - CamX);
            float relY = (wy - CamY);

            float yScale = (float)Math.Cos(Angle);

            float rotatedY = relY * yScale;
            float rotatedX = relX;

            float zoomedX = rotatedX * Zoom;
            float zoomedY = rotatedY * Zoom;

            int screenX = (int)((zoomedX + _logicWidth / 2) * _scaleX) + ShakeX;
            int screenY = (int)((zoomedY + _logicHeight / 2) * _scaleY) + ShakeY;
            return (screenX, screenY);
        }
    }
}
