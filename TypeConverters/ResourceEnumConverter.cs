using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace VSTSBot.TypeConverters
{
    public class ResourceEnumConverter : EnumConverter
    {
        private class LookupTable : Dictionary<string, object>
        {
            public LookupTable()
                : base(StringComparer.Create(CultureInfo.CurrentCulture, true))
            {
            }
        }

        #region Attributes

        private Dictionary<CultureInfo, LookupTable> _lookupTables = new Dictionary<CultureInfo, LookupTable>();
        private ResourceManager _resourceManager;
        private bool ehUmFlagedEnum = false;
        private Array flagedValues;

        #endregion

        public ResourceEnumConverter(Type enumerador)
            : this(enumerador, new ResourceManager(typeof(Resources.Enums.Enums)))
        {
        }

        public ResourceEnumConverter(Type enumerador, ResourceManager resourceManager)
            : base(enumerador)
        {
            _resourceManager = resourceManager;
            ehUmFlagedEnum = enumerador.IsDefined(typeof(FlagsAttribute), false);

            if (ehUmFlagedEnum)
                flagedValues = Enum.GetValues(enumerador);
        }

        /// <summary>
        /// Recupera o dicionário de valores para caché (criando se necessário)
        /// </summary>
        /// <param name="culture"></param>
        private LookupTable GetLookupTable(CultureInfo culture)
        {
            LookupTable lookupTable = null;

            culture = culture == null ? CultureInfo.CurrentCulture : culture;

            if (!_lookupTables.TryGetValue(culture, out lookupTable))
            {
                lookupTable = new LookupTable();
                foreach (object value in base.GetStandardValues())
                {
                    string text = GetValueText(culture, value);
                    if (text != null)
                    {
                        lookupTable.Add(text, value);
                    }
                }
                _lookupTables.Add(culture, lookupTable);
            }
            return lookupTable;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            if (sourceType == typeof(int))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;

            if (destinationType == typeof(int))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        private object GetValue(CultureInfo culture, string text)
        {
            LookupTable lookupTable = GetLookupTable(culture);
            object result = null;
            lookupTable.TryGetValue(text, out result);
            return result;
        }

        private string GetValueText(CultureInfo culture, object value)
        {
            string resourceKey;
            string result;

            culture = culture == null ? CultureInfo.CurrentCulture : culture;

            // Para localização do nome do enumerador e não de um enumerado.
            if (value == null)
                return null;

            if (!Enum.IsDefined(value.GetType(), value))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            resourceKey = string.Format("{0}_{1}", value.GetType().Name, value.ToString());
            result = _resourceManager.GetString(resourceKey, culture);

            if (result != null)
                return result;

            return result ?? value.ToString();
        }

        private object GetFlagValue(CultureInfo culture, string text)
        {

            LookupTable lookupTable = GetLookupTable(culture);
            ulong result = 0;

            foreach (string textValue in text.Split(','))
            {
                object value = null;
                if (!lookupTable.TryGetValue(textValue.Trim(), out value))
                {
                    return null;
                }
                result |= Convert.ToUInt32(value);
            }

            return Enum.ToObject(EnumType, result);
        }

        private string GetFlagValueText(CultureInfo culture, object value)
        {
            if (value == null)
                return null;

            string result = null;

            culture = culture == null ? CultureInfo.CurrentCulture : culture;

            // Se for um valor simples, não combinado de um Flaged Enum retorna o seu valor localizado
            if (Enum.IsDefined(value.GetType(), value))
                return GetValueText(culture, value);

            // Caso contrário procurar a combinação que compõe o valor
            foreach (object flagValue in flagedValues)
            {
                ulong lFlagValue = Convert.ToUInt32(flagValue);

                if (IsSingleBitValue(lFlagValue))
                {
                    if ((lFlagValue & Convert.ToUInt32(value)) == lFlagValue)
                    {
                        string valueText = GetValueText(culture, flagValue);

                        if (result == null)
                            result = valueText;
                        else
                            result = string.Format("{0}, {1}", result, valueText);
                    }
                }
            }
            return result;
        }

        private bool IsSingleBitValue(ulong value)
        {
            if (value == 0)
                return false;

            if (value == 1)
                return true;

            return ((value & (value - 1)) == 0);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            culture = culture == null ? CultureInfo.CurrentCulture : culture;

            if (value is string && value != null)
            {
                object result = ehUmFlagedEnum ? GetFlagValue(culture, (string)value) : GetValue(culture, (string)value);

                if (result == null)
                    result = base.ConvertFrom(context, culture, value);

                return result;

            }

            if (value is int && value != null)
            {
                try
                {
                    return Enum.ToObject(this.EnumType, (int)value);
                }
                catch (Exception)
                {
                    throw new InvalidCastException();
                }
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            culture = culture == null ? CultureInfo.CurrentCulture : culture;

            if (value != null && destinationType == typeof(string))
            {
                return ehUmFlagedEnum ? GetFlagValueText(culture, value) : GetValueText(culture, value);
            }

            if (value != null && destinationType == typeof(int))
            {
                return (int)value;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public static IList<KeyValuePair<Enum, string>> GetValues(Type tipoDoEnumerador, CultureInfo culture,
            bool incluirValorPadrao)
        {
            List<KeyValuePair<Enum, string>> result = new List<KeyValuePair<Enum, string>>();

            TypeConverter converter = TypeDescriptor.GetConverter(tipoDoEnumerador);

            object valorPadrao = Activator.CreateInstance(tipoDoEnumerador);

            foreach (Enum value in Enum.GetValues(tipoDoEnumerador))
            {
                if (!incluirValorPadrao && ((Enum)valorPadrao).ToString() == value.ToString())
                    continue;

                KeyValuePair<Enum, string> pair = new KeyValuePair<Enum, string>(value,
                    converter.ConvertToString(null, culture, value));
                result.Add(pair);
            }
            return result;
        }
    }
}