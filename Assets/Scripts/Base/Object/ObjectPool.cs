using System;
using System.Collections.Generic;

namespace ET
{
	public class ObjectQueue : IDisposable
	{
		public string TypeName { get; }

		private readonly Queue<Object> queue = new Queue<Object>();

		public ObjectQueue(string typeName)
		{
			this.TypeName = typeName;
		}

		public void Enqueue(Object entity)
		{
			this.queue.Enqueue(entity);
		}

		public Object Dequeue()
		{
			return this.queue.Dequeue();
		}

		public Object Peek()
		{
			return this.queue.Peek();
		}

		public Queue<Object> Queue
		{
			get
			{
				return this.queue;
			}
		}

		public int Count
		{
			get
			{
				return this.queue.Count;
			}
		}

		public virtual void Dispose()
		{
			while (this.queue.Count > 0)
			{
				Object obj = this.queue.Dequeue();
				obj.Dispose();
			}
		}
	}

	public class ObjectPool : IDisposable
	{
		private static ObjectPool instance;

		public static ObjectPool Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new ObjectPool();
				}

				return instance;
			}
		}

		private readonly Dictionary<Type, ObjectQueue> dictionary = new Dictionary<Type, ObjectQueue>();

		public Object Fetch(Type type)
		{
			Object obj;
			if (!this.dictionary.TryGetValue(type, out ObjectQueue queue))
			{
				obj = (Object)Activator.CreateInstance(type);
			}
			else if (queue.Count == 0)
			{
				obj = (Object)Activator.CreateInstance(type);
			}
			else
			{
				obj = queue.Dequeue();
			}

			if (obj is IPoolObject poolObject) 
			{
				poolObject.OnFetch();
			}
			return obj;
		}

		public T Fetch<T>() where T : Object
		{
			T t = (T)this.Fetch(typeof(T));
			return t;
		}

		public void Recycle(Object obj)
		{
			Type type = obj.GetType();
			ObjectQueue queue;
			if (!this.dictionary.TryGetValue(type, out queue))
			{
				queue = new ObjectQueue(type.Name);
				this.dictionary.Add(type, queue);
			}
			queue.Enqueue(obj);

			if (obj is IPoolObject poolObject)
			{
				poolObject.OnRecycle();
			}
		}

		public virtual void Dispose()
		{
			foreach (var kv in this.dictionary)
			{
				kv.Value.Dispose();
			}
			this.dictionary.Clear();
			instance = null;
		}
	}
}