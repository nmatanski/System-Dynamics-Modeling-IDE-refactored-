using System.Collections.Generic;

namespace SimpleDrawing
{
    public interface IConnectable
    {
        List<ITransformable> References { get; set; }

        void Connect(IConnectable target);
    }
}
