﻿using System;
using System.Collections.Generic;

namespace ET
{
    [ObjectSystem]
    public class ETCancellationTokenDestroySystem : DestroySystem<ETCancellationToken>
    {
        public override void Destroy(ETCancellationToken self)
        {
            self.actions.Clear();
        }
    }

    [HideInHierarchy]
    public class ETCancellationToken : Entity
    {
        public readonly List<Action> actions = new List<Action>();

        public void Register(Action callback)
        {
            this.actions.Add(callback);
        }

        public void Cancel()
        {
            foreach (Action action in this.actions)
            {
                action.Invoke();
            }
        }

        public async ETVoid CancleAfter(long afterTimeCancel) 
        {
            await TimerComponent.Instance.WaitAsync(afterTimeCancel);
            this.Cancel();
        }
    }
}