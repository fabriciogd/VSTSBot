using System;
using System.Runtime.Serialization;
using VSTSBot.Extensions;

namespace VSTSBot.Exceptions
{
    [Serializable]
    public class UnknownCommandException : Exception
    {
        #region Attributes

        public string CommandName { get; } = string.Empty;

        #endregion

        public UnknownCommandException(string commandName)
            : this(commandName, string.Format(Resources.Exceptions.Exceptions.UnknownCommandException, commandName), null)
        {
        }

        public UnknownCommandException(string commandName, string message, Exception innerException)
           : base(message, innerException)
        {
            this.CommandName = commandName;
        }

        public UnknownCommandException(string message, Exception innerException)
          : this(string.Empty, message, innerException)
        {
        }

        protected UnknownCommandException(SerializationInfo info, StreamingContext context)
           : base(info, context)
        {
            this.CommandName = info.GetString(nameof(this.CommandName));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.ThrowIfNull(nameof(info));

            info.AddValue(nameof(this.CommandName), this.CommandName);
            base.GetObjectData(info, context);
        }
    }
}