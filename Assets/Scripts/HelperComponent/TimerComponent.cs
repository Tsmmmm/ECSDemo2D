using System;
using System.Collections.Generic;

namespace ET
{
	public interface ITimer
	{
		void Run(bool isTimeout);
	}

	[HideInHierarchy]
	public abstract class MyTimer : Object, ITimer, IPoolObject
	{
		public void OnFetch()
		{
			this.InstanceId = IdGenerater.GenerateInstanceId();
		}

		public void OnRecycle()
		{
			this.InstanceId = 0;
		}

		public virtual void Run(bool isTimeout) { }
	}

	public static class TimerFactory 
	{
		public static T Create<T>() where T : MyTimer, new()
		{
			T t = ObjectPool.Instance.Fetch<T>();
			EventSystem.Instance.Awake(t);
			EventSystem.Instance.RegisterSystem(t);
			return t;
		}

		public static T Create<T, A>(A a) where T : MyTimer, new()
		{
			T t = ObjectPool.Instance.Fetch<T>();
			EventSystem.Instance.Awake(t, a);
			EventSystem.Instance.RegisterSystem(t);
			return t;
		}

		public static T Create<T, A, B>(A a, B b) where T : MyTimer, new()
		{
			T t = ObjectPool.Instance.Fetch<T>();
			EventSystem.Instance.Awake(t, a, b);
			EventSystem.Instance.RegisterSystem(t);
			return t;
		}

		public static T Create<T, A, B, C>(A a, B b, C c) where T : MyTimer, new()
		{
			T t = ObjectPool.Instance.Fetch<T>();
			EventSystem.Instance.Awake(t, a, b, c);
			EventSystem.Instance.RegisterSystem(t);
			return t;
		}
	}

	[ObjectSystem]
	public class OnceWaitTimerAwakeSystem : AwakeSystem<OnceWaitTimer, ETTaskCompletionSource<bool>>
	{
		public override void Awake(OnceWaitTimer self, ETTaskCompletionSource<bool> callback)
		{
			self.Callback = callback;
		}
	}

	public class OnceWaitTimer : MyTimer
	{
		public ETTaskCompletionSource<bool> Callback { get; set; }

		public override void Run(bool isTimeout)
		{
			ETTaskCompletionSource<bool> tcs = this.Callback;
			TimerComponent.Instance.Remove(this.InstanceId);
			tcs.SetResult(isTimeout);
		}
	}

	[ObjectSystem]
	public class OnceTimerAwakeSystem : AwakeSystem<OnceTimer, Action<bool>>
	{
		public override void Awake(OnceTimer self, Action<bool> callback)
		{
			self.Callback = callback;
		}
	}

	public class OnceTimer : MyTimer
	{
		public Action<bool> Callback { get; set; }

		public override void Run(bool isTimeout)
		{
			try
			{
				this.Callback.Invoke(isTimeout);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}

	[ObjectSystem]
	public class RepeatedTimerAwakeSystem : AwakeSystem<RepeatedTimer, long, Action<bool>>
	{
		public override void Awake(RepeatedTimer self, long repeatedTime, Action<bool> callback)
		{
			self.Awake(repeatedTime, callback);
		}
	}

	public class RepeatedTimer : MyTimer
	{
		public void Awake(long repeatedTime, Action<bool> callback)
		{
			this.StartTime = TimeHelper.Now();
			this.RepeatedTime = repeatedTime;
			this.Callback = callback;
			this.Count = 1;
		}

		private long StartTime { get; set; }

		private long RepeatedTime { get; set; }

		// 下次一是第几次触发
		private int Count { get; set; }

		public Action<bool> Callback { private get; set; }

		public override void Run(bool isTimeout)
		{
			++this.Count;
			long tillTime = this.StartTime + this.RepeatedTime * this.Count;
			TimerComponent.Instance.AddToTimeId(tillTime, this.InstanceId);

			try
			{
				this.Callback?.Invoke(isTimeout);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			this.StartTime = 0;
			this.RepeatedTime = 0;
			this.Callback = null;
			this.Count = 0;
		}
	}

	[ObjectSystem]
	public class TimerComponentAwakeSystem : AwakeSystem<TimerComponent>
	{
		public override void Awake(TimerComponent self)
		{
			self.Awake();
		}
	}

	[ObjectSystem]
	public class TimerComponentUpdateSystem : UpdateSystem<TimerComponent>
	{
		public override void Update(TimerComponent self)
		{
			self.Update();
		}
	}

	public class TimerComponent : Component
	{
		public static TimerComponent Instance { get; private set; }

		private readonly Dictionary<long, ITimer> timers = new Dictionary<long, ITimer>();

		/// <summary>
		/// key: time, value: timer id
		/// </summary>
		public readonly MultiMap<long, long> TimeId = new MultiMap<long, long>();

		private readonly Queue<long> timeOutTime = new Queue<long>();

		private readonly Queue<long> timeOutTimerIds = new Queue<long>();

		// 记录最小时间，不用每次都去MultiMap取第一个值
		private long minTime;

		public void Awake() 
		{
			Instance = this;
		}

		public void Update()
		{
			if (this.TimeId.Count == 0)
			{
				return;
			}

			long timeNow = TimeHelper.Now();

			if (timeNow < this.minTime)
			{
				return;
			}

			foreach (KeyValuePair<long, List<long>> kv in this.TimeId.GetDictionary())
			{
				long k = kv.Key;
				if (k > timeNow)
				{
					minTime = k;
					break;
				}
				this.timeOutTime.Enqueue(k);
			}

			while (this.timeOutTime.Count > 0)
			{
				long time = this.timeOutTime.Dequeue();
				foreach (long timerId in this.TimeId[time])
				{
					this.timeOutTimerIds.Enqueue(timerId);
				}
				this.TimeId.Remove(time);
			}

			while (this.timeOutTimerIds.Count > 0)
			{
				long timerId = this.timeOutTimerIds.Dequeue();
				ITimer timer;
				if (!this.timers.TryGetValue(timerId, out timer))
				{
					continue;
				}

				timer.Run(true);
			}
		}

		public async ETTask<bool> WaitTillAsync(long tillTime, ETCancellationToken cancellationToken)
		{
			if (TimeHelper.Now() > tillTime)
			{
				return true;
			}
			ETTaskCompletionSource<bool> tcs = new ETTaskCompletionSource<bool>();
			OnceWaitTimer timer = TimerFactory.Create<OnceWaitTimer, ETTaskCompletionSource<bool>>(tcs);
			this.timers[timer.InstanceId] = timer;
			AddToTimeId(tillTime, timer.InstanceId);

			long instanceId = timer.InstanceId;
			cancellationToken.Register(() =>
			{
				if (instanceId != timer.InstanceId)
				{
					return;
				}

				timer.Run(false);

				this.Remove(timer.InstanceId);
			});
			return await tcs.Task;
		}

		public async ETTask<bool> WaitTillAsync(long tillTime)
		{
			if (TimeHelper.Now() > tillTime)
			{
				return true;
			}
			ETTaskCompletionSource<bool> tcs = new ETTaskCompletionSource<bool>();
			OnceWaitTimer timer = TimerFactory.Create<OnceWaitTimer, ETTaskCompletionSource<bool>>(tcs);
			this.timers[timer.InstanceId] = timer;
			AddToTimeId(tillTime, timer.InstanceId);
			return await tcs.Task;
		}

		public async ETTask<bool> WaitAsync(long time, ETCancellationToken cancellationToken)
		{
			long tillTime = TimeHelper.Now() + time;

			if (TimeHelper.Now() > tillTime)
			{
				return true;
			}

			ETTaskCompletionSource<bool> tcs = new ETTaskCompletionSource<bool>();
			OnceWaitTimer timer = TimerFactory.Create<OnceWaitTimer, ETTaskCompletionSource<bool>>(tcs);
			this.timers[timer.InstanceId] = timer;
			AddToTimeId(tillTime, timer.InstanceId);
			long instanceId = timer.InstanceId;
			cancellationToken.Register(() =>
			{
				if (instanceId != timer.InstanceId)
				{
					return;
				}

				timer.Run(false);

				this.Remove(timer.InstanceId);
			});
			return await tcs.Task;
		}

		public async ETTask<bool> WaitAsync(long time)
		{
			long tillTime = TimeHelper.Now() + time;
			ETTaskCompletionSource<bool> tcs = new ETTaskCompletionSource<bool>();
			OnceWaitTimer timer = TimerFactory.Create<OnceWaitTimer, ETTaskCompletionSource<bool>>(tcs);
			this.timers[timer.InstanceId] = timer;
			AddToTimeId(tillTime, timer.InstanceId);
			return await tcs.Task;
		}

		/// <summary>
		/// 创建一个RepeatedTimer
		/// </summary>
		/// <param name="time"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		public long NewRepeatedTimer(long time, Action<bool> action)
		{
			if (time < 30)
			{
				throw new Exception($"repeated time < 30");
			}
			long tillTime = TimeHelper.Now() + time;
			RepeatedTimer timer = TimerFactory.Create<RepeatedTimer, long, Action<bool>>(time, action);
			this.timers[timer.InstanceId] = timer;
			AddToTimeId(tillTime, timer.InstanceId);
			return timer.InstanceId;
		}

		public RepeatedTimer GetRepeatedTimer(long id)
		{
			if (!this.timers.TryGetValue(id, out ITimer timer))
			{
				return null;
			}
			return timer as RepeatedTimer;
		}

		public void Remove(long id)
		{
			if (id == 0)
			{
				return;
			}
			ITimer timer;
			if (!this.timers.TryGetValue(id, out timer))
			{
				return;
			}
			this.timers.Remove(id);

			(timer as IDisposable)?.Dispose();
		}

		public long NewOnceTimer(long tillTime, Action<bool> action)
		{
			OnceTimer timer = TimerFactory.Create<OnceTimer, Action<bool>>(action);
			this.timers[timer.InstanceId] = timer;
			AddToTimeId(tillTime, timer.InstanceId);
			return timer.InstanceId;
		}

		public OnceTimer GetOnceTimer(long id)
		{
			if (!this.timers.TryGetValue(id, out ITimer timer))
			{
				return null;
			}
			return timer as OnceTimer;
		}

		public void AddToTimeId(long tillTime, long id)
		{
			this.TimeId.Add(tillTime, id);
			if (tillTime < this.minTime)
			{
				this.minTime = tillTime;
			}
		}
	}
}