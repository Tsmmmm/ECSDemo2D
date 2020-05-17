using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ET
{
    [ObjectSystem]
    public class InputComponentUpdateSystem : UpdateSystem<InputComponent>
    {
        public override void Update(InputComponent self)
        {
            self.Update();
        }
    }

    public class InputComponent : Component
    {
        public event Action OnCodeDown;

        public void Update()
        {
            if (Keyboard.current != null && Keyboard.current.added)
            {
                if (Keyboard.current.anyKey.isPressed)
                {
                    OnCodeDown?.Invoke();
                }
            }
        }
    }
}
