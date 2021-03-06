﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Creek.Tools
{
    /// <summary>
    ///   A marker interface for classes that subscribe to messages.
    /// </summary>
    public interface IHandle
    {
    }

    /// <summary>
    ///   Denotes a class which can handle a particular type of message.
    /// </summary>
    /// <typeparam name = "TMessage">The type of message to handle.</typeparam>
    public interface IHandle<TMessage> : IHandle
    {
        /// <summary>
        ///   Handles the message.
        /// </summary>
        /// <param name = "message">The message.</param>
        void Handle(TMessage message);
    }

    public interface IEventPublisher
    {
        /// <summary>
        /// Gets or sets the E vent aggregator.
        /// </summary>
        /// <value>
        /// The E vent aggregator.
        /// </value>
        IEventAggregator EventAggregator { get; set; }
    }

    public interface IEventPublisher<TMessage> : IEventPublisher
    {
        /// <summary>
        /// Publishes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Publish(TMessage message);
    }

    /// <summary>
    ///   Enables loosely-coupled publication of and subscription to events.
    /// </summary>
    public interface IEventAggregator
    {
        /// <summary>
        ///   Gets or sets the default publication thread marshaller.
        /// </summary>
        /// <value>
        ///   The default publication thread marshaller.
        /// </value>
        Action<Action> PublicationThreadMarshaller { get; set; }

        /// <summary>
        ///   Subscribes an instance to all events declared through implementations of <see cref = "IHandle{T}" />
        /// </summary>
        /// <param name = "instance">The instance to subscribe for event publication.</param>
        void Subscribe(object instance);

        /// <summary>
        ///   Unsubscribes the instance from all events.
        /// </summary>
        /// <param name = "instance">The instance to unsubscribe.</param>
        void Unsubscribe(object instance);

        /// <summary>
        ///   Publishes a message.
        /// </summary>
        /// <param name = "message">The message instance.</param>
        /// <remarks>
        ///   Uses the default thread marshaller during publication.
        /// </remarks>
        void Publish(object message);

        /// <summary>
        ///   Publishes a message.
        /// </summary>
        /// <param name = "message">The message instance.</param>
        /// <param name = "marshal">Allows the publisher to provide a custom thread marshaller for the message publication.</param>
        void Publish(object message, Action<Action> marshal);
    }

    /// <summary>
    ///   Enables loosely-coupled publication of and subscription to events.
    /// </summary>
    public class EventAggregator : IEventAggregator
    {
        /// <summary>
        ///   The default thread marshaller used for publication;
        /// </summary>
        public static Action<Action> DefaultPublicationThreadMarshaller = action => action();

        private readonly List<Handler> handlers = new List<Handler>();

        /// <summary>
        ///   Initializes a new instance of the <see cref = "EventAggregator" /> class.
        /// </summary>
        public EventAggregator()
        {
            PublicationThreadMarshaller = DefaultPublicationThreadMarshaller;
        }

        #region IEventAggregator Members

        /// <summary>
        ///   Gets or sets the default publication thread marshaller.
        /// </summary>
        /// <value>
        ///   The default publication thread marshaller.
        /// </value>
        public Action<Action> PublicationThreadMarshaller { get; set; }

        /// <summary>
        ///   Subscribes an instance to all events declared through implementations of <see cref = "IHandle{T}" />
        /// </summary>
        /// <param name = "instance">The instance to subscribe for event publication.</param>
        public virtual void Subscribe(object instance)
        {
            lock (handlers)
            {
                if (handlers.Any(x => x.Matches(instance)))
                    return;

                handlers.Add(new Handler(instance));
            }
        }

        /// <summary>
        ///   Unsubscribes the instance from all events.
        /// </summary>
        /// <param name = "instance">The instance to unsubscribe.</param>
        public virtual void Unsubscribe(object instance)
        {
            lock (handlers)
            {
                Handler found = handlers.FirstOrDefault(x => x.Matches(instance));

                if (found != null)
                    handlers.Remove(found);
            }
        }

        /// <summary>
        ///   Publishes a message.
        /// </summary>
        /// <param name = "message">The message instance.</param>
        /// <remarks>
        ///   Does not marshall the the publication to any special thread by default.
        /// </remarks>
        public virtual void Publish(object message)
        {
            Publish(message, PublicationThreadMarshaller);
        }

        /// <summary>
        ///   Publishes a message.
        /// </summary>
        /// <param name = "message">The message instance.</param>
        /// <param name = "marshal">Allows the publisher to provide a custom thread marshaller for the message publication.</param>
        public virtual void Publish(object message, Action<Action> marshal)
        {
            Handler[] toNotify;
            lock (handlers)
                toNotify = handlers.ToArray();

            marshal(() =>
                        {
                            Type messageType = message.GetType();

                            List<Handler> dead = toNotify
                                .Where(handler => !handler.Handle(messageType, message))
                                .ToList();

                            if (dead.Any())
                            {
                                lock (handlers)
                                {
                                    dead.Apply(x => handlers.Remove(x));
                                }
                            }
                        });
        }

        #endregion

        #region Nested type: Handler

        protected class Handler
        {
            private readonly WeakReference reference;
            private readonly Dictionary<Type, MethodInfo> supportedHandlers = new Dictionary<Type, MethodInfo>();

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class.
            /// </summary>
            /// <param name="handler">The handler.</param>
            public Handler(object handler)
            {
                reference = new WeakReference(handler);

                IEnumerable<Type> interfaces = handler.GetType().GetInterfaces()
                    .Where(x => typeof (IHandle).IsAssignableFrom(x) && x.IsGenericType);

                foreach (Type @interface in interfaces)
                {
                    Type type = @interface.GetGenericArguments()[0];
                    MethodInfo method = @interface.GetMethod("Handle");
                    supportedHandlers[type] = method;
                }
            }

            /// <summary>
            /// Matcheses the specified instance.
            /// </summary>
            /// <param name="instance">The instance.</param>
            /// <returns></returns>
            public bool Matches(object instance)
            {
                return reference.Target == instance;
            }

            /// <summary>
            /// Handles the specified message type.
            /// </summary>
            /// <param name="messageType">Type of the message.</param>
            /// <param name="message">The message.</param>
            /// <returns></returns>
            public bool Handle(Type messageType, object message)
            {
                object target = reference.Target;
                if (target == null)
                    return false;

                foreach (var pair in supportedHandlers)
                {
                    if (pair.Key.IsAssignableFrom(messageType))
                    {
                        pair.Value.Invoke(target, new[] {message});
                        return true;
                    }
                }

                return true;
            }
        }

        #endregion
    }
}