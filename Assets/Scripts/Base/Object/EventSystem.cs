using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ET
{
	public sealed class EventSystem : IDisposable
	{
		private static EventSystem instance;

		public static EventSystem Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new EventSystem();
				}
				return instance;
			}
		}

		private readonly Dictionary<long, Object> allObject = new Dictionary<long, Object>();

		//先存起来，方便以后支持多项目加载
		private readonly Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

		private readonly UnOrderMultiMapSet<Type, Type> types = new UnOrderMultiMapSet<Type, Type>();

		private readonly Dictionary<string, List<object>> allEvents = new Dictionary<string, List<object>>();

		private readonly UnOrderMultiMap<Type, IAwakeSystem> awakeSystems = new UnOrderMultiMap<Type, IAwakeSystem>();
		private readonly UnOrderMultiMap<Type, IStartSystem> startSystems = new UnOrderMultiMap<Type, IStartSystem>();
		private readonly UnOrderMultiMap<Type, IUpdateSystem> updateSystems = new UnOrderMultiMap<Type, IUpdateSystem>();
		private readonly UnOrderMultiMap<Type, ILateUpdateSystem> lateUpdateSystems = new UnOrderMultiMap<Type, ILateUpdateSystem>();
		private readonly UnOrderMultiMap<Type, IDestroySystem> destroySystems = new UnOrderMultiMap<Type, IDestroySystem>();

		private readonly Queue<long> starts = new Queue<long>();

		private Queue<long> updates = new Queue<long>();
		private Queue<long> updates2 = new Queue<long>();

		private Queue<long> lateUpdates = new Queue<long>();
		private Queue<long> lateUpdates2 = new Queue<long>();

		private EventSystem()
		{
			this.Add(typeof(EventSystem).Assembly);
		}

		public void Add(Assembly assembly)
		{
			this.assemblies[assembly.ManifestModule.ScopeName] = assembly;
			this.types.Clear();
			foreach (Assembly value in this.assemblies.Values)
			{
				foreach (Type type in value.GetTypes())
				{
					if (type.IsAbstract)
					{
						continue;
					}

					object[] objects = type.GetCustomAttributes(typeof(BaseAttribute), true);
					if (objects.Length == 0)
					{
						continue;
					}

					foreach (BaseAttribute baseAttribute in objects)
					{
						this.types.Add(baseAttribute.AttributeType, type);
					}
				}
			}

			this.awakeSystems.Clear();
			this.startSystems.Clear();
			this.updateSystems.Clear();
			this.lateUpdateSystems.Clear();
			this.destroySystems.Clear();

			foreach (Type type in this.GetTypes(typeof(ObjectSystemAttribute)))
			{
				object obj = Activator.CreateInstance(type);
				switch (obj)
				{
					case IAwakeSystem objectSystem:
						this.awakeSystems.Add(objectSystem.Type(), objectSystem);
						break;
					case IStartSystem startSystem:
						this.startSystems.Add(startSystem.Type(), startSystem);
						break;
					case IUpdateSystem updateSystem:
						this.updateSystems.Add(updateSystem.Type(), updateSystem);
						break;
					case ILateUpdateSystem lateUpdateSystem:
						this.lateUpdateSystems.Add(lateUpdateSystem.Type(), lateUpdateSystem);
						break;
					case IDestroySystem destroySystem:
						this.destroySystems.Add(destroySystem.Type(), destroySystem);
						break;
				}
			}

			this.allEvents.Clear();
			foreach (Type type in types[typeof(EventAttribute)])
			{
				object[] attrs = type.GetCustomAttributes(typeof(EventAttribute), false);

				foreach (object attr in attrs)
				{
					EventAttribute aEventAttribute = (EventAttribute)attr;
					object obj = Activator.CreateInstance(type);
					IEvent iEvent = obj as IEvent;
					if (iEvent == null)
					{
						Log.Error($"{obj.GetType().Name} 没有继承IEvent");
					}
					this.RegisterEvent(aEventAttribute.Type, iEvent);
				}
			}
		}

		public Assembly GetAssembly(string name)
		{
			return this.assemblies[name];
		}

		public void RegisterEvent(string eventId, IEvent e)
		{
			if (!this.allEvents.ContainsKey(eventId))
			{
				this.allEvents.Add(eventId, new List<object>());
			}
			this.allEvents[eventId].Add(e);
		}

		public HashSet<Type> GetTypes(Type systemAttributeType)
		{
			if (!this.types.ContainsKey(systemAttributeType))
			{
				return new HashSet<Type>();
			}
			return this.types[systemAttributeType];
		}

		public List<Type> GetTypes()
		{
			List<Type> allTypes = new List<Type>();
			foreach (Assembly assembly in this.assemblies.Values)
			{
				allTypes.AddRange(assembly.GetTypes());
			}
			return allTypes;
		}

		public void RegisterSystem(Object obj, bool isRegister = true)
		{
			if (!isRegister)
			{
				this.Remove(obj.InstanceId);
				return;
			}
			this.allObject.Add(obj.InstanceId, obj);

			Type type = obj.GetType();

			if (this.startSystems.ContainsKey(type))
			{
				this.starts.Enqueue(obj.InstanceId);
			}

			if (this.updateSystems.ContainsKey(type))
			{
				this.updates.Enqueue(obj.InstanceId);
			}

			if (this.lateUpdateSystems.ContainsKey(type))
			{
				this.lateUpdates.Enqueue(obj.InstanceId);
			}
		}

		public void Remove(long instanceId)
		{
			this.allObject.Remove(instanceId);
		}

		public Object Get(long instanceId)
		{
			Object obj = null;
			this.allObject.TryGetValue(instanceId, out obj);
			return obj;
		}

		public bool IsRegister(long instanceId)
		{
			return this.allObject.ContainsKey(instanceId);
		}

        #region Awake
        public void Awake(Object obj)
		{
			List<IAwakeSystem> iAwakeSystems = this.awakeSystems[obj.GetType()];
			if (iAwakeSystems == null)
			{
				return;
			}

			foreach (IAwakeSystem aAwakeSystem in iAwakeSystems)
			{
				if (aAwakeSystem == null)
				{
					continue;
				}

				IAwake iAwake = aAwakeSystem as IAwake;
				if (iAwake == null)
				{
					continue;
				}

				try
				{
					iAwake.Run(obj);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public void Awake<P1>(Object obj, P1 p1)
		{
			List<IAwakeSystem> iAwakeSystems = this.awakeSystems[obj.GetType()];
			if (iAwakeSystems == null)
			{
				return;
			}

			foreach (IAwakeSystem aAwakeSystem in iAwakeSystems)
			{
				if (aAwakeSystem == null)
				{
					continue;
				}

				IAwake<P1> iAwake = aAwakeSystem as IAwake<P1>;
				if (iAwake == null)
				{
					continue;
				}

				try
				{
					iAwake.Run(obj, p1);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public void Awake<P1, P2>(Object obj, P1 p1, P2 p2)
		{
			List<IAwakeSystem> iAwakeSystems = this.awakeSystems[obj.GetType()];
			if (iAwakeSystems == null)
			{
				return;
			}

			foreach (IAwakeSystem aAwakeSystem in iAwakeSystems)
			{
				if (aAwakeSystem == null)
				{
					continue;
				}

				IAwake<P1, P2> iAwake = aAwakeSystem as IAwake<P1, P2>;
				if (iAwake == null)
				{
					continue;
				}

				try
				{
					iAwake.Run(obj, p1, p2);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public void Awake<P1, P2, P3>(Object obj, P1 p1, P2 p2, P3 p3)
		{
			List<IAwakeSystem> iAwakeSystems = this.awakeSystems[obj.GetType()];
			if (iAwakeSystems == null)
			{
				return;
			}

			foreach (IAwakeSystem aAwakeSystem in iAwakeSystems)
			{
				if (aAwakeSystem == null)
				{
					continue;
				}

				IAwake<P1, P2, P3> iAwake = aAwakeSystem as IAwake<P1, P2, P3>;
				if (iAwake == null)
				{
					continue;
				}

				try
				{
					iAwake.Run(obj, p1, p2, p3);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public void Awake<P1, P2, P3, P4>(Object obj, P1 p1, P2 p2, P3 p3, P4 p4)
		{
			List<IAwakeSystem> iAwakeSystems = this.awakeSystems[obj.GetType()];
			if (iAwakeSystems == null)
			{
				return;
			}

			foreach (IAwakeSystem aAwakeSystem in iAwakeSystems)
			{
				if (aAwakeSystem == null)
				{
					continue;
				}

				IAwake<P1, P2, P3, P4> iAwake = aAwakeSystem as IAwake<P1, P2, P3, P4>;
				if (iAwake == null)
				{
					continue;
				}

				try
				{
					iAwake.Run(obj, p1, p2, p3, p4);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}
        #endregion

        #region Unity Start
        private void Start()
		{
			while (this.starts.Count > 0)
			{
				long instanceId = this.starts.Dequeue();
				Object obj;
				if (!this.allObject.TryGetValue(instanceId, out obj))
				{
					continue;
				}

				List<IStartSystem> iStartSystems = this.startSystems[obj.GetType()];
				if (iStartSystems == null)
				{
					continue;
				}

				foreach (IStartSystem iStartSystem in iStartSystems)
				{
					try
					{
						iStartSystem.Run(obj);
					}
					catch (Exception e)
					{
						Log.Error(e);
					}
				}
			}
		}
		#endregion

		#region Unity Update
		public void Update()
		{
			this.Start();
			while (this.updates.Count > 0)
			{
				long instanceId = this.updates.Dequeue();
				Object obj;
				if (!this.allObject.TryGetValue(instanceId, out obj))
				{
					continue;
				}
				if (obj.IsDisposed)
				{
					continue;
				}

				List<IUpdateSystem> iUpdateSystems = this.updateSystems[obj.GetType()];
				if (iUpdateSystems == null)
				{
					continue;
				}

				this.updates2.Enqueue(instanceId);

				foreach (IUpdateSystem iUpdateSystem in iUpdateSystems)
				{
					try
					{
						iUpdateSystem.Run(obj);
					}
					catch (Exception e)
					{
						Log.Error(e);
					}
				}
			}

			ObjectHelper.Swap(ref this.updates, ref this.updates2);
		}

		#endregion

		#region Unity LateUpdate
		public void LateUpdate()
		{
			while (this.lateUpdates.Count > 0)
			{
				long instanceId = this.lateUpdates.Dequeue();
				Object obj;
				if (!this.allObject.TryGetValue(instanceId, out obj))
				{
					continue;
				}
				if (obj.IsDisposed)
				{
					continue;
				}

				List<ILateUpdateSystem> iLateUpdateSystems = this.lateUpdateSystems[obj.GetType()];
				if (iLateUpdateSystems == null)
				{
					continue;
				}

				this.lateUpdates2.Enqueue(instanceId);

				foreach (ILateUpdateSystem iLateUpdateSystem in iLateUpdateSystems)
				{
					try
					{
						iLateUpdateSystem.Run(obj);
					}
					catch (Exception e)
					{
						Log.Error(e);
					}
				}
			}

			ObjectHelper.Swap(ref this.lateUpdates, ref this.lateUpdates2);
		}
		#endregion

		#region Destroy
		public void Destroy(Object obj)
		{
			List<IDestroySystem> iDestroySystems = this.destroySystems[obj.GetType()];
			if (iDestroySystems == null)
			{
				return;
			}

			foreach (IDestroySystem iDestroySystem in iDestroySystems)
			{
				if (iDestroySystem == null)
				{
					continue;
				}

				try
				{
					iDestroySystem.Run(obj);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}
        #endregion

        #region RunFunction
        public void Run(string type)
		{
			List<object> iEvents;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}
			foreach (object obj in iEvents)
			{
				try
				{
					if (!(obj is AEvent aEvent))
					{
						Log.Error($"event error: {obj.GetType().Name}");
						continue;
					}
					aEvent.Run();
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public void Run<A>(string type, A a)
		{
			List<object> iEvents;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}
			foreach (object obj in iEvents)
			{
				try
				{
					if (!(obj is AEvent<A> aEvent))
					{
						Log.Error($"event error: {obj.GetType().Name}");
						continue;
					}
					aEvent.Run(a);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public void Run<A, B>(string type, A a, B b)
		{
			List<object> iEvents;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}
			foreach (object obj in iEvents)
			{
				try
				{
					if (!(obj is AEvent<A, B> aEvent))
					{
						Log.Error($"event error: {obj.GetType().Name}");
						continue;
					}
					aEvent.Run(a, b);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public void Run<A, B, C>(string type, A a, B b, C c)
		{
			List<object> iEvents;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}
			foreach (object obj in iEvents)
			{
				try
				{
					if (!(obj is AEvent<A, B, C> aEvent))
					{
						Log.Error($"event error: {obj.GetType().Name}");
						continue;
					}
					aEvent.Run(a, b, c);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}
        #endregion

        public void Dispose()
		{
			instance = null;
		}
	}
}