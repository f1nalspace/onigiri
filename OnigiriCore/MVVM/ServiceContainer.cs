using System;
using System.Collections.Generic;

namespace Finalspace.Onigiri.MVVM
{
    public class ServiceContainer
    {
        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();

        private static ServiceContainer _instance = null;
        public static ServiceContainer Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ServiceContainer();
                return _instance;
            }
        }

        public void Register<T>(T instance) where T : class {
            Type type = typeof(T);
            if (!_instances.ContainsKey(type))
                _instances.Add(type, instance);
            else
                throw new Exception($"The type '{type}' is already registered!");
        }

        public T Get<T>() where T : class
        {
            Type type = typeof(T);
            if (_instances.ContainsKey(type))
                return (T)_instances[type];
            return default(T);
        }
    }
}
