namespace ET 
{
    public abstract class Component : Object, IPoolObject
    {
        protected Entity entityRef;
        public Entity EntityRef 
        {
            get 
            {
                return this.entityRef;
            }
            set 
            {
                this.entityRef = value;

#if UNITY_EDITOR
                if (this.ViewGO != null && this.entityRef != null && this.entityRef.ViewGO) 
                {
                    this.ViewGO.transform.SetParent(this.entityRef.ViewGO.transform, false);
                }
#endif
            }
        }
        public T GetEntityRef<T>() where T : Entity
        {
            return this.entityRef as T;
        }

        public override void Dispose() 
        {
            base.Dispose();
            if (this.entityRef != null && !this.entityRef.IsDisposed) 
            {
                this.entityRef.RemoveComponent(this);
            }
            this.entityRef = null;
#if UNITY_EDITOR
            if (this.ViewGO != null) 
            {
                UnityEngine.Object.Destroy(this.ViewGO);
            }
#endif
        }

        public virtual void OnFetch()
        {
            this.InstanceId = IdGenerater.GenerateInstanceId();
        }

        public virtual void OnRecycle()
        {
            this.InstanceId = 0;
        }
    }
}