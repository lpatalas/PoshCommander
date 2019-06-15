using System.Management.Automation.Host;

namespace PoshCommander
{
    public static class RectangleExtensions
    {
        public static Coordinates GetBottomLeft(this Rectangle rectangle)
            => new Coordinates(rectangle.Left, rectangle.Bottom);

        public static int GetHeight(this Rectangle rectangle)
            => rectangle.Bottom - rectangle.Top + 1;

        public static int GetWidth(this Rectangle rectangle)
            => rectangle.Right - rectangle.Left + 1;

        public static Coordinates GetTopLeft(this Rectangle rectangle)
            => new Coordinates(rectangle.Left, rectangle.Top);
    }
}
