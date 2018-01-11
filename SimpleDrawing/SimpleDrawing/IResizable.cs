using System.Windows.Forms;

namespace SimpleDrawing
{
    interface IResizable
    {
        bool Resize(int x, int y, int index);

        Cursor GetCursor(int index);
    }
}
