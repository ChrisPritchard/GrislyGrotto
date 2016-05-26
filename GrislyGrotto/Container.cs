using System;
using System.Collections.Generic;
using System.Linq;
using GrislyGrotto.Framework;
using GrislyGrotto.Framework.Data;
using GrislyGrotto.Framework.Data.Implementations;

namespace GrislyGrotto
{
    public class Container
    {
        private static SimpleContainer container;
        private static readonly object padlock = new object();

        private static void CheckContainer()
        {
            lock (padlock)
            {
                if (container == null)
                    SetupMappings();
            }
        }

        private static void SetupMappings()
        {
            container = new SimpleContainer();

            container.RegisterInstance<Random>(() => new Random());

            container.RegisterInstance<IPostData>(() => new SqlPostData());
            container.RegisterInstance<IResourceService>(() => new GenericResourceService());
            container.RegisterInstance<IUserData>(() => new SqlUserData());

            container.RegisterInstance<IAuthenticationService>(() => new CookieAuthenticationService());
            container.RegisterInstance<INavigationService>(() => new ContextNavigationService());
        }

        public static T GetInstance<T>()
        {
            CheckContainer();
            return (T)container.GetInstance(typeof(T));
        }

        public static T Create<T>() where T : class 
        {
            CheckContainer();
            var registeredTypes = container.RegisteredTypes();
            var constructor = typeof (T).GetConstructors()
                .FirstOrDefault(c => !c.GetParameters().Any(p => !registeredTypes.Contains(p.ParameterType)));
            if (constructor == null)
                return null;

            return (T)constructor.Invoke(constructor.GetParameters().Select(p => container.GetInstance(p.ParameterType)).ToArray());
        }

        class SimpleContainer
        {
            private readonly Dictionary<Type, Func<object>> registry;
            private readonly Dictionary<string, object> singletonsCreated;
            private readonly Dictionary<string, Func<object>> singletonsToCreate;

            public SimpleContainer()
            {
                registry = new Dictionary<Type, Func<object>>();
                singletonsCreated = new Dictionary<string, object>();
                singletonsToCreate = new Dictionary<string, Func<object>>();
            }

            public void RegisterInstance<TInterface>(Func<object> creator) where TInterface : class
            {
                if (registry.ContainsKey(typeof(TInterface)))
                    registry[typeof(TInterface)] = creator;
                else
                    registry.Add(typeof(TInterface), creator);
            }

            public void RegisterSingleton(string key, Func<object> creator)
            {
                if (singletonsToCreate.ContainsKey(key))
                    singletonsToCreate[key] = creator;
                else
                    singletonsToCreate.Add(key, creator);
            }

            public IEnumerable<Type> RegisteredTypes()
            {
                return registry.Keys.ToArray();
            }

            public object GetInstance(Type key)
            {
                return registry[key]();
            }

            public object GetInstance(string key)
            {
                if (singletonsCreated.ContainsKey(key))
                    return singletonsCreated[key];
                return (singletonsCreated[key] = singletonsToCreate[key]());
            }
        }
    }
}