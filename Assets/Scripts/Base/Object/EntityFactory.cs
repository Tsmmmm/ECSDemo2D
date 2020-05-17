using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    public static class EntityFactory
    {
        public static T Create<T>() where T : Entity, new()
        {
            T t = ObjectPool.Instance.Fetch<T>();
            EventSystem.Instance.Awake(t);
            EventSystem.Instance.RegisterSystem(t);
            return t;
        }

        public static T Create<T, A>(A a) where T : Entity, new()
        {
            T t = ObjectPool.Instance.Fetch<T>();
            EventSystem.Instance.Awake(t, a);
            EventSystem.Instance.RegisterSystem(t);
            return t;
        }

        public static T Create<T, A, B>(A a, B b) where T : Entity, new()
        {
            T t = ObjectPool.Instance.Fetch<T>();
            EventSystem.Instance.Awake(t, a, b);
            EventSystem.Instance.RegisterSystem(t);
            return t;
        }

        public static T Create<T, A, B, C>(A a, B b, C c) where T : Entity, new()
        {
            T t = ObjectPool.Instance.Fetch<T>();
            EventSystem.Instance.Awake(t, a, b, c);
            EventSystem.Instance.RegisterSystem(t);
            return t;
        }

        public static T CreateWithParent<T>(Entity parent) where T : Entity, new()
        {
            T t = ObjectPool.Instance.Fetch<T>();
            parent.AddChild(t);
            EventSystem.Instance.Awake(t);
            EventSystem.Instance.RegisterSystem(t);
            return t;
        }

        public static T CreateWithParent<T, A>(Entity parent, A a) where T : Entity, new()
        {
            T t = ObjectPool.Instance.Fetch<T>();
            parent.AddChild(t);
            EventSystem.Instance.Awake(t, a);
            EventSystem.Instance.RegisterSystem(t);
            return t;
        }

        public static T CreateWithParent<T, A, B>(Entity parent, A a, B b) where T : Entity, new()
        {
            T t = ObjectPool.Instance.Fetch<T>();
            parent.AddChild(t);
            EventSystem.Instance.Awake(t, a, b);
            EventSystem.Instance.RegisterSystem(t);
            return t;
        }

        public static T CreateWithParent<T, A, B, C>(Entity parent, A a, B b, C c) where T : Entity, new()
        {
            T t = ObjectPool.Instance.Fetch<T>();
            parent.AddChild(t);
            EventSystem.Instance.Awake(t, a, b, c);
            EventSystem.Instance.RegisterSystem(t);
            return t;
        }
    }
}
