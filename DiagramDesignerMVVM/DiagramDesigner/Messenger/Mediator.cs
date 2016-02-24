using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

namespace DiagramDesigner
{
    #region WeakAction Inner Class
    /// <summary>
    /// This class creates a weak delegate of form Action(Of Object)
    /// </summary>
    public class WeakAction
    {
        #region Data
        private readonly WeakReference _target;
        private readonly Type _ownerType;
        private readonly Type _actionType;
        private readonly string _methodName;
        #endregion

        #region Public Properties/Methods
        public WeakAction(object target, Type actionType, MethodBase mi)
        {
            if (target == null)
            {
                Debug.Assert(mi.IsStatic);
                _ownerType = mi.DeclaringType;
            }
            else
                _target = new WeakReference(target);
            _methodName = mi.Name;
            _actionType = actionType;
        }

        public Type ActionType
        {
            get { return _actionType; }
        }

        public bool HasBeenCollected
        {
            get
            {
                return (_ownerType == null && (_target == null || !_target.IsAlive));
            }
        }

        public Delegate GetMethod()
        {
            if (_ownerType != null)
            {
                return Delegate.CreateDelegate(_actionType, _ownerType, _methodName);
            }

            if (_target != null && _target.IsAlive)
            {
                object target = _target.Target;
                if (target != null)
                    return Delegate.CreateDelegate(_actionType, target, _methodName);
            }

            return null;
        }
        #endregion
    }
    #endregion



    /// <summary>
    /// This class creates a simple Mediator which loosely connects different objects together. 
    /// The message handlers are organized using string-based message keys and are held in a WeakReference
    /// collection.
    /// </summary>
    public class Mediator
    {


        #region Data
        static readonly Mediator instance = new Mediator();
        static readonly object syncLock = new object();
        private readonly Dictionary<object, List<WeakAction>> _registeredHandlers =
            new Dictionary<object, List<WeakAction>>();
        #endregion

        #region Ctor
        static Mediator()
        {

        }

        private Mediator()
        {

        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Performs the actual registration of a target
        /// </summary>
        /// <param name="key">Key to store in dictionary</param>
        /// <param name="actionType">Delegate type</param>
        /// <param name="handler">Method</param>
        private void RegisterHandler(object key, Type actionType, Delegate handler)
        {
            var action = new WeakAction(handler.Target, actionType, handler.Method);

            lock (_registeredHandlers)
            {
                List<WeakAction> wr;
                if (_registeredHandlers.TryGetValue(key, out wr))
                {
                    if (wr.Count > 0)
                    {
                        WeakAction wa = wr[0];
                        if (wa.ActionType != actionType &&
                            !wa.ActionType.IsAssignableFrom(actionType))
                            throw new ArgumentException("Invalid key passed to RegisterHandler - existing handler has incompatible parameter type");
                    }

                    wr.Add(action);
                }
                else
                {
                    wr = new List<WeakAction> { action };
                    _registeredHandlers.Add(key, wr);
                }
            }
        }

        /// <summary>
        /// Performs the unregistration from a target
        /// </summary>
        /// <param name="key">Key to store in dictionary</param>
        /// <param name="actionType">Delegate type</param>
        /// <param name="handler">Method</param>
        private void UnregisterHandler(object key, Type actionType, Delegate handler)
        {
            lock (_registeredHandlers)
            {
                List<WeakAction> wr;
                if (_registeredHandlers.TryGetValue(key, out wr))
                {
                    wr.RemoveAll(wa => handler == wa.GetMethod() && actionType == wa.ActionType);

                    if (wr.Count == 0)
                        _registeredHandlers.Remove(key);
                }
            }
        }

        /// <summary>
        /// This method broadcasts a message to all message targets for a given
        /// message key and passes a parameter.
        /// </summary>
        /// <param name="key">Message key</param>
        /// <param name="message">Message parameter</param>
        /// <returns>True/False if any handlers were invoked.</returns>
        private bool NotifyColleagues(object key, object message)
        {
            List<WeakAction> wr;
            List<WeakAction> wrCopy = new List<WeakAction>();
            lock (_registeredHandlers)
            {
                if (!_registeredHandlers.TryGetValue(key, out wr))
                    return false;
                else
                {
                    foreach (var weakRe in wr)
                    {
                        wrCopy.Add(weakRe);
                    }
                }

            }

            foreach (var cb in wrCopy)
            {
                Delegate action = cb.GetMethod();

                if (action != null)
                    action.DynamicInvoke(message);
            }

            lock (_registeredHandlers)
            {
                wr.RemoveAll(wa => wa.HasBeenCollected);
            }

            return true;
        }
        #endregion

        #region Public Properties/Methods

        /// <summary>
        /// Singleton Instance
        /// </summary>
        public static Mediator Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// This registers a Type with the mediator.  Any methods decorated with <seealso cref="MediatorMessageSinkAttribute"/> will be 
        /// registered as target method handlers for the given message key.
        /// </summary>
        /// <param name="view">Object to register</param>
        public void Register(object view)
        {
            // Look at all instance/static methods on this object type.
            foreach (var mi in view.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
            {
                // See if we have a target attribute - if so, register the method as a handler.
                foreach (var att in mi.GetCustomAttributes(typeof(MediatorMessageSinkAttribute), true))
                {
                    var mha = (MediatorMessageSinkAttribute)att;
                    var pi = mi.GetParameters();
                    if (pi.Length != 1)
                        throw new InvalidCastException("Cannot cast " + mi.Name + " to Action<T> delegate type.");

                    Type actionType = typeof(Action<>).MakeGenericType(pi[0].ParameterType);
                    object key = (mha.MessageKey) ?? actionType;

                    if (mi.IsStatic)
                        RegisterHandler(key, actionType, Delegate.CreateDelegate(actionType, mi));
                    else
                        RegisterHandler(key, actionType, Delegate.CreateDelegate(actionType, view, mi.Name));
                }
            }
        }

        /// <summary>
        /// This method unregisters a type from the message mediator.
        /// </summary>
        /// <param name="view">Object to unregister</param>
        public void Unregister(object view)
        {
            foreach (var mi in view.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
            {
                foreach (var att in mi.GetCustomAttributes(typeof(MediatorMessageSinkAttribute), true))
                {
                    var mha = (MediatorMessageSinkAttribute)att;
                    var pi = mi.GetParameters();
                    if (pi.Length != 1)
                        throw new InvalidCastException("Cannot cast " + mi.Name + " to Action<T> delegate type.");

                    Type actionType = typeof(Action<>).MakeGenericType(pi[0].ParameterType);
                    object key = (mha.MessageKey) ?? actionType;

                    if (mi.IsStatic)
                        UnregisterHandler(key, actionType, Delegate.CreateDelegate(actionType, mi));
                    else
                        UnregisterHandler(key, actionType, Delegate.CreateDelegate(actionType, view, mi.Name));
                }
            }
        }

        /// <summary>
        /// This registers a specific method as a message handler for a specific type.
        /// </summary>
        /// <param name="key">Message key</param>
        /// <param name="handler">Handler method</param>
        public void RegisterHandler<T>(string key, Action<T> handler)
        {
            RegisterHandler(key, handler.GetType(), handler);
        }

        /// <summary>
        /// This registers a specific method as a message handler for a specific type.
        /// </summary>
        /// <param name="handler">Handler method</param>
        public void RegisterHandler<T>(Action<T> handler)
        {
            RegisterHandler(typeof(Action<T>), handler.GetType(), handler);
        }

        /// <summary>
        /// This unregisters a method as a handler.
        /// </summary>
        /// <param name="key">Message key</param>
        /// <param name="handler">Handler</param>
        public void UnregisterHandler<T>(string key, Action<T> handler)
        {
            UnregisterHandler(key, handler.GetType(), handler);
        }

        /// <summary>
        /// This unregisters a method as a handler for a specific type
        /// </summary>
        /// <param name="handler">Handler</param>
        public void UnregisterHandler<T>(Action<T> handler)
        {
            UnregisterHandler(typeof(Action<T>), handler.GetType(), handler);
        }

        /// <summary>
        /// This method broadcasts a message to all message targets for a given
        /// message key and passes a parameter.
        /// </summary>
        /// <param name="key">Message key</param>
        /// <param name="message">Message parameter</param>
        /// <returns>True/False if any handlers were invoked.</returns>
        public bool NotifyColleagues<T>(string key, T message)
        {
            return NotifyColleagues((object)key, message);
        }

        /// <summary>
        /// This method broadcasts a message to all message targets for a given parameter type.
        /// If a derived type is passed, any handlers for interfaces or base types will also be
        /// invoked.
        /// </summary>
        /// <param name="message">Message parameter</param>
        /// <returns>True/False if any handlers were invoked.</returns>
        public bool NotifyColleagues<T>(T message)
        {
            Type actionType = typeof(Action<>).MakeGenericType(typeof(T));
            var keyList = from key in _registeredHandlers.Keys
                          where key is Type && ((Type)key).IsAssignableFrom(actionType)
                          select key;
            bool rc = false;
            foreach (var key in keyList)
                rc |= NotifyColleagues(key, message);

            return rc;
        }

        /// <summary>
        /// This method broadcasts a message to all message targets for a given
        /// message key and passes a parameter.  The message targets are all called
        /// asynchronously and any resulting exceptions are ignored.
        /// </summary>
        /// <param name="key">Message key</param>
        /// <param name="message">Message parameter</param>
        public void NotifyColleaguesAsync<T>(string key, T message)
        {
            Func<string, T, bool> smaFunc = NotifyColleagues;
            smaFunc.BeginInvoke(key, message, ia =>
            {
                try { smaFunc.EndInvoke(ia); }
                catch { }
            }, null);
        }

        /// <summary>
        /// This method broadcasts a message to all message targets for a given parameter type.
        /// If a derived type is passed, any handlers for interfaces or base types will also be
        /// invoked.  The message targets are all called asynchronously and any resulting exceptions
        /// are ignored.
        /// </summary>
        /// <param name="message">Message parameter</param>
        public void NotifyColleaguesAsync<T>(T message)
        {
            Func<T, bool> smaFunc = NotifyColleagues;
            smaFunc.BeginInvoke(message, ia =>
            {
                try { smaFunc.EndInvoke(ia); }
                catch { }
            }, null);
        }
        #endregion
    }
}
