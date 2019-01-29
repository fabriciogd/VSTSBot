using System.ComponentModel;
using VSTSBot.Attributes;
using VSTSBot.TypeConverters;

namespace VSTSBot.Enums
{
    [TypeConverter(typeof(LocalizedEnumConverter))]
    public enum Language
    {
        [StringValue("pt-BR")]
        Portugues,

        [StringValue("en-US")]
        Ingles
    }
}