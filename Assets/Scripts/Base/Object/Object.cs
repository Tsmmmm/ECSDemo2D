using System;
using UnityEngine;

namespace ET
{
    public abstract class Object : IDisposable
    {
        public long InstanceId { get; protected set; }

        public bool IsDisposed
        {
            get
            {
                return this.InstanceId == 0;
            }
        }

#if UNITY_EDITOR
        public static GameObject Global
        {
            get
            {
                GameObject global = GameObject.Find("/Global");
                if (global == null) 
                {
                    global = new GameObject("Global");
                }
                return global;
            }
        }

        public GameObject ViewGO { get; private set; }
#endif

        public Object()
        {
#if UNITY_EDITOR
            if (!this.GetType().IsDefined(typeof(HideInHierarchy), true))
            {
                this.ViewGO = new GameObject();
                this.ViewGO.name = this.GetType().Name;
                this.ViewGO.transform.SetParent(Global.transform, false);
            }
#endif
        }

        public virtual void Dispose()
        {
            if (this.IsDisposed) 
            {
                return;
            }

            EventSystem.Instance.Remove(this.InstanceId);
            this.InstanceId = 0;
            EventSystem.Instance.Destroy(this);
            ObjectPool.Instance.Recycle(this);

#if UNITY_EDITOR
            if (this.ViewGO != null)
            {
                UnityEngine.Object.Destroy(this.ViewGO);
            }
#endif
        }
    }
}
