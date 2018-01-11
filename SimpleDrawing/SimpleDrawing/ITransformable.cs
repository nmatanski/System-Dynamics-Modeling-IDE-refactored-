using System.Drawing;

namespace SimpleDrawing
{
    public interface ITransformable
    {
        void Translate(int dX, int dY, int item);

        int Contains(int x, int y);

        Rectangle Bounds { get; }
    }
}
