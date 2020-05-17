using System;
using System.Collections;

namespace ET
{
    public static class GameObjectHelper
    {
        public static void Traversal<T>(T t, Action<T> action, bool isDeep = false) where T : IEnumerable
        {
            foreach (T a in t)
            {
                action.Invoke(a);
                if (isDeep)
                {
                    Traversal<T>(a, action);
                }
            }
        }
    }
}