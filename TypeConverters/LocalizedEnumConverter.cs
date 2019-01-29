using System;

namespace VSTSBot.TypeConverters
{
    public class LocalizedEnumConverter : ResourceEnumConverter
    {
        public LocalizedEnumConverter(Type type)
            : base(type, Resources.Enums.Enums.ResourceManager)
        {
        }
    }
}