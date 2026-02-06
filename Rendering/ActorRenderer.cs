using StarWarsAnimation.Core;
using StarWarsAnimation.Entities;

namespace StarWarsAnimation.Rendering
{
    public class ActorRenderer
    {
        private readonly TerminalRenderer _term;
        private readonly float _scaleX;
        private readonly float _scaleY;

        public ActorRenderer(TerminalRenderer term, float scaleX, float scaleY)
        {
            _term = term;
            _scaleX = scaleX;
            _scaleY = scaleY;
        }

        public (int x, int y)? DrawActor(Actor actor, bool isSith, CameraTransform transform, float widthScale, float heightScale, bool isReflection = false, int floorY = 0, int ox = 0, int oy = 0, bool isBlur = false)
        {
            bool isTopDown = heightScale < 0.4f;
            float useFX = isBlur ? actor.PrevFX : actor.FX;
            float useFY = isBlur ? actor.PrevFY : actor.FY;
            
            int x = (int)(useFX * _scaleX) + ox;
            int y = (int)(useFY * _scaleY) + oy;
            int dir = actor.FacingRight ? 1 : -1;
            
            string mainColor = isBlur ? Palette.Dim : (isReflection ? Palette.Dim : Palette.White);
            string saberColor = isBlur ? Palette.Dim : (isReflection ? Palette.Dim : actor.Color);

            if (isReflection)
            {
                 y = floorY + (floorY - y);
                 if (y >= _term.Height) return null;
            }

            var pose = Pose.Get(actor.PoseIndex, isSith);

            if (isSith && actor.PoseIndex != 4 && !isBlur) 
            {
                 if (isTopDown) DrawCapeTopDown(x, y, dir, widthScale);
                 else DrawCape(actor, x, y, dir, heightScale, isReflection, floorY);
            }

            if (isTopDown)
            {
                 // Draw simplified top-down silhouette
                 _term.Draw(x, y, '●', mainColor); // Head/Center
                 _term.Draw(x - 1, y, '(', mainColor); // Left Shoulder
                 _term.Draw(x + 1, y, ')', mainColor); // Right Shoulder
                 
                 // Small indication of facing direction
                 _term.Draw(x + dir, y, dir == 1 ? '›' : '‹', mainColor);
            }

            float angleSin = (float)Math.Sqrt(1.0f - Math.Clamp(heightScale * heightScale, 0f, 1f));
            float zFactor = 4.5f * angleSin; 

            bool reflect = isReflection;
            string color = mainColor;

            void DrawPart3D(BodyPart part, int zDepth)
            {
                 int dy = (int)(part.Dy * heightScale);
                 int zShift = (int)(zDepth * zFactor);
                 int dx = RendererUtils.ScaleOffset(part.Dx, widthScale);
                  
                 int drawX = x + (dx * dir);
                 int drawY = y + (isReflection ? -(dy + zShift) : (dy + zShift));
                 
                 char c = part.Char;
                 if (isTopDown)
                 {
                     if (zDepth == 0 && part.Dy == pose.Head.Dy && part.Dx == pose.Head.Dx) c = '●';
                     else if (c == '▓' || c == '▒' || c == '█' || c == '▄' || c == '▀') c = '|';
                     else if (c == '╱' || c == '╲') c = '-';
                     else if (part.Equals(pose.Head)) c = '●';
                 }
                 
                 if (reflect)
                 {
                    if (c == '╱') c = '╲';
                    else if (c == '╲') c = '╱';
                    else if (c == '▄') c = '▀';
                    else if (c == '▀') c = '▄';
                    else if (c == '●') c = '●';
                 }
                 
                 _term.Draw(drawX, drawY, c, color);
            }
            
            if (!isTopDown) 
            {
                 DrawPart3D(pose.LegL, -1);
                 DrawPart3D(pose.LegR, 1);
                 DrawPart3D(pose.TorsoBottom, 0);
                 DrawPart3D(pose.TorsoTop, 0);
                 DrawPart3D(pose.ArmL, -1);
                 if (actor.PoseIndex != 3) DrawPart3D(pose.ArmR, 1);
            }
            else
            {
                 DrawPart3D(pose.ArmL, -1);
                 DrawPart3D(pose.TorsoTop, 0);
                 if (actor.PoseIndex != 3) DrawPart3D(pose.ArmR, 1);
            }

            DrawPart3D(pose.Head, 0);

            (int x, int y)? tipPos = null;
            if (actor.SaberActive)
            {
                int handDx = RendererUtils.ScaleOffset(pose.Hand.Dx, widthScale);
                int handDy = (int)(pose.Hand.Dy * heightScale);
                int zShift = (int)(1 * zFactor); 
                
                int sx = x + (handDx * dir);
                int sy = y + (isReflection ? -(handDy + zShift) : (handDy + zShift));
                
                int bdy = isReflection ? -pose.Blade.Dy : pose.Blade.Dy;
                bdy = (int)(bdy * heightScale);
                
                int bladeDx = RendererUtils.ScaleOffset(pose.Blade.Dx * actor.SaberLength, widthScale);
                int tipX = sx + (bladeDx * dir);
                int tipY = sy + (bdy * actor.SaberLength);
                
                // Fix for Top View: Prevent saber from floating "below" the character
                if (isTopDown)
                {
                    // In top view, Y represents "forward/backward" relative to camera, 
                    // not "up/down". We need to project the saber length differently.
                    // Simple fix: Force saber to be drawn relative to the hand position 
                    // without the heavy vertical foreshortening that pushes it down.
                    
                    int forwardFactor = pose.Blade.Dy < 0 ? -1 : 1; 
                    // If blade points up (Dy < 0), it should point "forward" (up on screen) in top view
                    // If blade points down, it points "backward" (down on screen)
                    
                    tipY = sy + (forwardFactor * (int)(actor.SaberLength * _scaleY * 0.3f));
                    // Pull the saber start point closer to the center body if needed
                    sx = x + (dir * 2); 
                    sy = y;
                }
                
                tipPos = (tipX, tipY);
                
                int dSx = tipX - sx;
                int dSy = tipY - sy;
                
                char bChar = '|';
                
                if (dSx == 0) bChar = '|';
                else 
                {
                    float slope = (float)dSy / dSx;
                    if (Math.Abs(slope) > 2.0f) bChar = '|';
                    else if (Math.Abs(slope) < 0.5f) bChar = '-';
                    else 
                    {
                        if (slope > 0) bChar = '╲'; 
                        else bChar = '╱';
                    }
                }
                
                _term.DrawLine(sx, sy, tipX, tipY, bChar, saberColor);
            }
            return tipPos;
        }

        private void DrawCapeTopDown(int x, int y, int dir, float widthScale)
        {
            int backOffset = Math.Abs(RendererUtils.ScaleOffset(2, widthScale));
            if (backOffset == 0) backOffset = 1;
            int backX = x - (dir * backOffset);
            string c = Palette.Red;
            
            _term.Draw(backX, y, '=', c);
            _term.Draw(backX + dir, y, '-', c);
            _term.Draw(backX, y - 1, dir == 1 ? '╱' : '╲', c);
            _term.Draw(backX, y + 1, dir == 1 ? '╲' : '╱', c);
        }

        private void DrawCape(Actor a, int anchorX, int anchorY, int dir, float heightScale, bool isReflection, int floorY)
        {
            int shoulderDy = (int)(-3 * heightScale); 
            int startX = anchorX - dir;
            int startY = anchorY + (isReflection ? -shoulderDy : shoulderDy);
            
            float relTailX = a.CapeTail.X - a.FX;
            float relTailY = a.CapeTail.Y - a.FY;
            
            int tailDx = (int)(relTailX * _scaleX);
            int tailDy = (int)(relTailY * _scaleY * heightScale);
            
            int tailX = anchorX + tailDx;
            int tailY = anchorY + (isReflection ? -tailDy : tailDy);
            
            int midX = (startX + tailX) / 2;
            int midY = (startY + tailY) / 2;
            
            int sag = (int)(1.0f * _scaleY * heightScale);
            midY += isReflection ? -sag : sag;

            int steps = 10;
            string color = isReflection ? Palette.White : Palette.Red;

            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                float invT = 1f - t;
                
                float lx = (invT * invT * startX) + (2 * invT * t * midX) + (t * t * tailX);
                float ly = (invT * invT * startY) + (2 * invT * t * midY) + (t * t * tailY);

                int cx = (int)lx;
                int cy = (int)ly;
                
                if (isReflection)
                {
                     if (cy >= _term.Height) continue;
                }

                char c = ' ';
                if (t < 0.3) c = '}'; 
                else if (t < 0.6) c = ')';
                else c = '›';
                
                float slope = (ly - startY) / (lx - startX + 0.001f);
                if (Math.Abs(slope) > 1.5) c = '│'; 
                else if (slope > 0.5) c = '╲';
                else if (slope < -0.5) c = '╱';
                
                _term.Draw(cx, cy, c, color);
                _term.Draw(cx, cy+1, ':', color);
            }
        }

        public void DrawSaberArc(Actor actor, (int x, int y)? currentTip, int ox, int oy, string color, float widthScale, float heightScale)
        {
            if (!currentTip.HasValue) return;
            
            float prevFX = actor.PrevFX;
            float prevFY = actor.PrevFY;
            int prevPoseIdx = actor.PrevPoseIndex;
            bool isSith = (color == Palette.Red);
            
            var prevPose = Pose.Get(prevPoseIdx, isSith);
            int dir = actor.FacingRight ? 1 : -1; 

            float angleSin = (float)Math.Sqrt(1.0f - Math.Clamp(heightScale * heightScale, 0f, 1f));
            float zFactor = 4.5f * angleSin;
            int zShift = (int)(1 * zFactor);

            int sx = (int)(prevFX * _scaleX) + ox + (RendererUtils.ScaleOffset(prevPose.Hand.Dx, widthScale) * dir);
            int sy = (int)(prevFY * _scaleY) + oy + (int)(prevPose.Hand.Dy * heightScale) + zShift; 
            
            int bdy = (int)(prevPose.Blade.Dy * heightScale);
            int bx = sx + (RendererUtils.ScaleOffset(prevPose.Blade.Dx * actor.SaberLength, widthScale) * dir);
            int by = sy + (bdy * actor.SaberLength);
            
            int prevTipX = bx;
            int prevTipY = by;

            int curTipX = currentTip.Value.x;
            int curTipY = currentTip.Value.y;

            int dx = curTipX - prevTipX;
            int dy = curTipY - prevTipY;
            int distSq = dx*dx + dy*dy;
            
            if (distSq > 4)
            {
                 _term.DrawLine(prevTipX, prevTipY, curTipX, curTipY, '░', color);
                 _term.DrawLine((sx + prevTipX)/2, (sy + prevTipY)/2, (sx + curTipX)/2, (sy + curTipY)/2, '▒', color);
            }
        }

        public void DrawShadows(float angle, int floorScreenY, (int sx, int sy) jediPos, (int sx, int sy) sithPos)
        {
            if (angle <= 0.3f || angle > 1.1f) return;

            int jx = jediPos.sx;
            int sx = sithPos.sx;
            int fy = floorScreenY;

            const string ShadowColor = "\u001b[30;1m";

            _term.Draw(jx, fy, '●', ShadowColor);
            _term.Draw(jx - 1, fy, '(', ShadowColor);
            _term.Draw(jx + 1, fy, ')', ShadowColor);

            _term.Draw(sx, fy, '●', ShadowColor);
            _term.Draw(sx - 1, fy, '(', ShadowColor);
            _term.Draw(sx + 1, fy, ')', ShadowColor);
        }
    }
}
