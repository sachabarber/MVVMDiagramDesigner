using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiagramDesigner
{
    /// <summary>
    /// This attribute allows a method to be targeted as a recipient for a message.
    /// It requires that the Type is registered with the MessageMediator through the
    /// <seealso cref="MessageMediator.Register"/> method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MediatorMessageSinkAttribute : Attribute
    {
        /// <summary>
        /// Message key
        /// </summary>
        public object MessageKey { get; private set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MediatorMessageSinkAttribute()
        {
            MessageKey = null;
        }

        /// <summary>
        /// Constructor that takes a message key
        /// </summary>
        /// <param name="messageKey">Message Key</param>
        public MediatorMessageSinkAttribute(string messageKey)
        {
            MessageKey = messageKey;
        }
    }

}
