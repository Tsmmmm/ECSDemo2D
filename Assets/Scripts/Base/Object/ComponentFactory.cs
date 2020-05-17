using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    public static class ComponentFactory
    {
        public static T CreateWithEntity<T>(Entity entity) where T : Component, new()
        {
            T t = ObjectPool.Instance.Fetch<T>();
            entity.AddComponent(t);
            EventSystem.Instance.Awake(t);
            EventSystem.Instance.RegisterSystem(t);
            return t;
        }

        public static T CreateWithEntity<T, A>(Entity entity, A a) where T : Component, new()
        {
            T t = ObjectPool.Instance.Fetch<T>();
            entity.AddComponent(t);
            EventSystem.Instance.Awake(t, a);
            EventSystem.Instance.RegisterSystem(t);
            return t;
        }

        public static T CreateWithEntity<T, A, B>(Entity entity, A a, B b) where T : Component, new()
        {
            T t = ObjectPool.Instance.Fetch<T>();
            entity.AddComponent(t);
            EventSystem.Instance.Awake(t, a, b);
            EventSystem.Instance.RegisterSystem(t);
            return t;
        }

        public static T CreateWithEntity<T, A, B, C>(Entity entity, A a, B b, C c) where T : Component, new()
        {
            T t = ObjectPool.Instance.Fetch<T>();
            entity.AddComponent(t);
            EventSystem.Instance.Awake(t, a, b, c);
            EventSystem.Instance.RegisterSystem(t);
            return t;
        }
    }
}
