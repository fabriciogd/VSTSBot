using System;
using System.ComponentModel.Composition;
using VSTSBot.Enums;
using VSTSBot.Extensions;

namespace VSTSBot.Attributes
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CommandMetadataAttribute : Attribute
    {
        #region Attributes

        public Dialog Dialog { get; }

        #endregion

        public CommandMetadataAttribute(Dialog dialog)
        {
            dialog.ThrowIfNull(nameof(dialog));

            this.Dialog = dialog;
        }
    }
}