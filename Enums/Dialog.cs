using System.ComponentModel;
using VSTSBot.TypeConverters;

namespace VSTSBot.Enums
{
    [TypeConverter(typeof(LocalizedEnumConverter))]
    public enum Dialog
    {
        Connect,

        Disconnect,

        Language,

        Teams,

        TeamMembers,

        WorkItems
    }
}