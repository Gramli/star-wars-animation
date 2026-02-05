using System.Collections.Generic;

namespace StarWarsAnimation.Entities
{
    public struct BodyPart
    {
        public int Dx, Dy;
        public char Char;

        public BodyPart(int dx, int dy, char c = ' ')
        {
            Dx = dx;
            Dy = dy;
            Char = c;
        }

        public void Deconstruct(out int dx, out int dy, out char c)
        {
            dx = Dx;
            dy = Dy;
            c = Char;
        }
    }

    public class Pose
    {
        public BodyPart Head { get; set; }
        public BodyPart TorsoTop { get; set; }
        public BodyPart TorsoBottom { get; set; }
        public BodyPart LegL { get; set; }
        public BodyPart LegR { get; set; }
        public BodyPart ArmL { get; set; }
        public BodyPart ArmR { get; set; }
        public BodyPart Hand { get; set; }
        public BodyPart Blade { get; set; }

        public static Pose Get(int index, bool isSith)
        {
            // Default "Idle" parts
            var p = new Pose
            {
                Head = new BodyPart(0, -4, isSith ? '⍙' : '●'),
                TorsoTop = new BodyPart(0, -3, isSith ? '▒' : '▓'),
                TorsoBottom = new BodyPart(0, -2, isSith ? '▓' : '█'),
                LegL = new BodyPart(-1, -1, '╱'),
                LegR = new BodyPart(1, -1, '╲'),
                ArmL = new BodyPart(-1, -3, '╱'),
                ArmR = new BodyPart(1, -3, '╲'),
                Hand = new BodyPart(1, -3, ' '),
                Blade = new BodyPart(0, -1, '│')
            };

            if (!isSith)
            {
                var belt = p.TorsoBottom;
                belt.Char = '≡';
                p.TorsoBottom = belt;
            }

            switch (index)
            {
                case 0: // Idle
                    if (isSith)
                    {
                        p.ArmR = new BodyPart(1, -3, '╲');
                        p.Blade = new BodyPart(1, 1, '╲');
                    }
                    else
                    {
                        p.ArmR = new BodyPart(1, -3, '╲');
                        p.Blade = new BodyPart(1, -1, '╱');
                    }
                    break;

                case 1: // Attack
                    p.Head = new BodyPart(2, -4, p.Head.Char);
                    p.TorsoTop = new BodyPart(1, -3, '▓');
                    p.LegL = new BodyPart(-2, -1, '╱');
                    p.LegR = new BodyPart(2, -1, '╲');
                    p.ArmL = new BodyPart(-1, -3, '╲');
                    p.ArmR = new BodyPart(2, -4, '╱');
                    p.Hand = new BodyPart(2, -4);
                    p.Blade = new BodyPart(1, -1, '╱');
                    break;

                case 2: // Guard
                    p.Head = new BodyPart(-1, -4, p.Head.Char);
                    p.TorsoTop = new BodyPart(-1, -3, '▓');
                    p.LegL = new BodyPart(-2, -1, '╱');
                    p.LegR = new BodyPart(1, -1, '╲');
                    p.ArmR = new BodyPart(0, -3, '│');
                    p.Hand = new BodyPart(0, -3);
                    p.Blade = new BodyPart(-1, -1, '╲');
                    break;

                case 3: // Lock
                    p.Head = new BodyPart(1, -4, p.Head.Char);
                    p.TorsoTop = new BodyPart(1, -3, '█');
                    p.LegL = new BodyPart(-2, -1, '╱');
                    p.LegR = new BodyPart(2, -1, '╲');
                    p.ArmR = new BodyPart(1, -3, '─');
                    p.Hand = new BodyPart(1, -3);
                    p.Blade = new BodyPart(1, 0, '─');
                    break;

                case 4: // Kneel
                    p.Head = new BodyPart(1, -2, isSith ? '⍙' : '●');
                    p.TorsoTop = new BodyPart(0, -1, '▄');
                    p.TorsoBottom = new BodyPart(0, 0, ' ');
                    p.LegL = new BodyPart(0, -1, ' ');
                    p.LegR = new BodyPart(2, -1, '▄');
                    p.ArmL = new BodyPart(0, 0, ' ');
                    p.ArmR = new BodyPart(0, 0, ' ');
                    break;

                case 5: // Anticipation
                    p.Head = new BodyPart(-1, -4, p.Head.Char);
                    p.TorsoTop = new BodyPart(-1, -3, '▓');
                    p.ArmR = new BodyPart(-2, -4, '╲');
                    p.Hand = new BodyPart(-2, -4);
                    p.Blade = new BodyPart(-1, -1, '╲');
                    break;

                case 6: // Force Push
                    p.Head = new BodyPart(0, -4, p.Head.Char);
                    p.TorsoTop = new BodyPart(0, -3, '▓');
                    p.ArmL = new BodyPart(2, -3, '─');
                    p.ArmR = new BodyPart(-1, -2, '╲');
                    p.Hand = new BodyPart(-1, -2);
                    p.Blade = new BodyPart(1, 1, '╲');
                    break;

                case 7: // Stagger
                    p.Head = new BodyPart(-2, -4, p.Head.Char);
                    p.TorsoTop = new BodyPart(-1, -3, '╲');
                    p.LegL = new BodyPart(-2, -1, '╱');
                    p.LegR = new BodyPart(1, -1, '╱');
                    p.ArmL = new BodyPart(-2, -3, '╱');
                    p.ArmR = new BodyPart(0, -3, '╲');
                    p.Hand = new BodyPart(0, -3);
                    p.Blade = new BodyPart(-1, -1, '╲');
                    break;

                case 8: // Jump
                    p.Head = new BodyPart(1, -4, p.Head.Char);
                    p.TorsoTop = new BodyPart(0, -3, '▓');
                    p.LegL = new BodyPart(-1, -2, '─');
                    p.LegR = new BodyPart(1, -2, '─');
                    p.ArmL = new BodyPart(-1, -3, '╱');
                    p.ArmR = new BodyPart(1, -3, '╲');
                    p.Hand = new BodyPart(1, -3);
                    p.Blade = new BodyPart(-1, -1, '╱');
                    break;

                case 9: // Suspended
                    p.Head = new BodyPart(0, -4, p.Head.Char);
                    p.TorsoTop = new BodyPart(0, -3, '▓');
                    p.LegL = new BodyPart(-1, -1, '│');
                    p.LegR = new BodyPart(1, -1, '│');
                    p.ArmL = new BodyPart(-2, -3, '╲');
                    p.ArmR = new BodyPart(2, -3, '╱');
                    p.Hand = new BodyPart(2, -3);
                    p.Blade = new BodyPart(0, -1, '│');
                    break;

                case 10: // Dash
                    p.Head = new BodyPart(3, -3, p.Head.Char);
                    p.TorsoTop = new BodyPart(2, -2, '─');
                    p.TorsoBottom = new BodyPart(0, -2, '─');
                    p.LegL = new BodyPart(-2, -2, '=');
                    p.LegR = new BodyPart(-1, -2, '=');
                    p.ArmL = new BodyPart(0, -2, '─');
                    p.ArmR = new BodyPart(3, -2, '─');
                    p.Hand = new BodyPart(3, -2);
                    p.Blade = new BodyPart(1, 0, '─');
                    break;

                case 11: // Crouch/Prep
                    p.Head = new BodyPart(0, -2, p.Head.Char);
                    p.TorsoTop = new BodyPart(0, -1, '▓');
                    p.TorsoBottom = new BodyPart(0, 0, ' ');
                    p.LegL = new BodyPart(-1, 0, '_');
                    p.LegR = new BodyPart(1, 0, '_');
                    p.ArmL = new BodyPart(-1, -1, '╱');
                    p.ArmR = new BodyPart(1, -1, '╲');
                    p.Hand = new BodyPart(1, -1);
                    p.Blade = new BodyPart(1, -1, '╱');
                    break;

                case 12: // Reach Hilt
                    p.ArmR = new BodyPart(-1, -2, '╱');
                    p.Hand = new BodyPart(-1, -2);
                    break;

                case 13: // Walk A
                    break;

                case 14: // Walk B
                    p.LegL = new BodyPart(1, -1, '╲');
                    p.LegR = new BodyPart(-1, -1, '╱');
                    break;
            }
            return p;
        }
    }
}
