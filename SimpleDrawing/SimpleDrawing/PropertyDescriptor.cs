using System;

namespace SimpleDrawing
{
    public class PropertyDescriptor
    {
        public string Caption { get; private set; }

        public string Key { get; private set; }

        public Type Type { get; private set; }

        public PropertyDescriptor(string caption, string key, Type type)
        {
            Caption = caption;
            Key = key;
            Type = type;
        }
    }
}
