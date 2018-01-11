using System.Collections.Generic;

namespace SimpleDrawing
{
    public interface IEditable
    {
        object this[string key] { get; set; }

        List<PropertyDescriptor> GetEditableProperties();
    }
}
