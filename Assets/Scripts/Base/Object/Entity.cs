using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public abstract class Entity : Object, IPoolObject
    {
        private string name;
        public string Name 
        {
            get 
            {
                return this.name;
            }
            set 
            {
                this.name = value;
                if (this.GameObject != null) 
                {
                    this.GameObject.name = this.name;
                }
#if UNITY_EDITOR
                if (this.ViewGO != null) 
                {
                    this.ViewGO.name = this.name;
                }
#endif
            }
        }
        public GameObject GameObject { get; set; }
        public Transform Transform { get; set; }

        protected Entity parent;
        public Entity Parent
        {
            get
            {
                return this.parent;
            }
            set
            {
                if (this.parent != null)
                {
                    this.parent.RemoveChild(this);
                }

                if (value != null) 
                {
                    value.AddChild(this);
                }
            }
        }

        private Dictionary<string, Entity> children = new Dictionary<string, Entity>();
        private Dictionary<Type, Component> components = new Dictionary<Type, Component>();

        public Entity()
        {
            this.Name = this.GetType().Name;
        }

        public T GetParent<T>() where T : Entity
        {
            return this.parent as T;
        }

        public void AddChild(Entity child)
        {
            if (this.children.ContainsKey(child.Name)) 
            {
                Log.Error($"child with this same {child.Name} has been added");
                return;
            }
            this.children.Add(child.Name, child);

            if (child.parent != null)
            {
                child.parent.RemoveChild(child);
            }
            child.parent = this;

            if (child.GameObject != null && child.parent != null && child.parent.GameObject != null)
            {
                child.Transform.SetParent(this.Transform, false);
            }

#if UNITY_EDITOR
            if (child.ViewGO && child.parent != null && child.parent.ViewGO != null) 
            {
                child.ViewGO.transform.SetParent(child.parent.ViewGO.transform, false);
            }
#endif
        }

        public void RemoveChild(Entity child, bool clean = false) 
        {
            this.RemoveChildByName(child.Name, clean);
        }

        public void RemoveChildren(bool clean = false) 
        {
            foreach (Entity entity in this.children.Values)
            {
                this.RemoveChild(entity, clean);
            }
        }

        public void RemoveChildByName(string name, bool clean = false)
        {
            Entity value;
            if (!this.children.TryGetValue(name, out value))
            {
                Log.Error($"dont has child {name}, but try to remove");
                return;
            }
            this.children.Remove(name);

            if (clean) 
            {
                value.Dispose();
            }
        }

        public T GetChild<T>(string name) where T : Entity
        {
            Entity value;
            if (!this.children.TryGetValue(name, out value))
            {
                return null;
            }
            return value as T;
        }

        public Component AddComponent(Component component) 
        {
            Type type = component.GetType();
            if (this.components.ContainsKey(type)) 
            {
                Log.Error($"aleardy have component {type.Name}");
                return null;
            }
            component.EntityRef = this;
            this.components.Add(type, component);
            return component;
        }

        public void RemoveComponent(Component component) 
        {
            Type type = component.GetType();
            if (!this.components.ContainsKey(type))
            {
                Log.Error($"dont have component {type.Name}, but try to remove");
            }
            this.components.Remove(type);
            component.Dispose();
        }

        public void RemoveAllComponent() 
        {
            foreach (Component component in this.components.Values) 
            {
                this.RemoveComponent(component);
            }
        }

        public T GetComponent<T>() where T : Component
        {
            Type type = typeof(T);
            Component value;
            if (!this.components.TryGetValue(type, out value))
            {
                return null;
            }
            return value as T;
        }

        public override void Dispose()
        {
            base.Dispose();
            foreach (Entity entity in this.children.Values) 
            {
                entity.Dispose();
            }
            this.children.Clear();

            if (this.parent != null && !this.parent.IsDisposed) 
            {
                this.parent.RemoveChild(this);
            }
            this.parent = null;
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
