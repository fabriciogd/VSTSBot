namespace VSTSBot.Attributes
{
    public class StringValueAttribute : System.Attribute
    {
        #region Attributes

        public string Value { get; }

        #endregion

        public StringValueAttribute(string value)
        {
            this.Value = value;
        }
    }
}