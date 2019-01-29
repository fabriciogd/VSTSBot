using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using VSTSBot.Attributes;

namespace VSTSBot.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum enumerador)
        {
            if (enumerador == null)
                return null;

            return enumerador.GetDescription(null);
        }

        public static string GetDescription(this Enum enumerador, CultureInfo cultura)
        {
            if (enumerador == null)
                return null;

            TypeConverter customConverter = TypeDescriptor.GetConverter(enumerador);

            if (customConverter.GetType() == typeof(EnumConverter))
            {
                // Retorna a conversão padrão para enums (value.ToString).
                return customConverter.ConvertToString(enumerador);
            }

            return customConverter.ConvertToString(null, cultura, enumerador);
        }

        public static Dictionary<Enum, string> GetDescriptions(this Enum enumerador)
        {
            return enumerador.GetDescriptions(null);
        }

        public static Dictionary<Enum, string> GetDescriptions(this Enum enumerador, CultureInfo cultura)
        {
            if (enumerador == null)
                return null;

            object valorPadrao = Activator.CreateInstance(enumerador.GetType());

            Dictionary<Enum, string> dicionarioLocalizado = new Dictionary<Enum, string>();

            foreach (Enum itemEnumerado in Enum.GetValues(enumerador.GetType()))
            {
                dicionarioLocalizado.Add(itemEnumerado, itemEnumerado.GetDescription(cultura));
            }

            return dicionarioLocalizado;
        }

        public static string GetStringValue(this Enum value)
        {
            string output = null;
            Type type = value.GetType();

            FieldInfo fi = type.GetField(value.ToString());

            StringValueAttribute[] attrs =
                fi.GetCustomAttributes(typeof(StringValueAttribute),
                                        false) as StringValueAttribute[];
            if (attrs.Length > 0)
            {
                output = attrs[0].Value;
            }

            return output;
        }
    }
}