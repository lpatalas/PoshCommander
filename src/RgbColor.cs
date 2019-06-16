namespace PoshCommander
{
    public struct RgbColor
    {
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        public RgbColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public static readonly RgbColor Black = new RgbColor(0, 0, 0);
        public static readonly RgbColor White = new RgbColor(255, 255, 255);
    }
}
