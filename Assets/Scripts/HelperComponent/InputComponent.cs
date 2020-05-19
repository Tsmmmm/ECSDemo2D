using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace ET
{
    [ObjectSystem]
    public class InputComponentAwakeSystem : AwakeSystem<InputComponent, string, string>
    {
        public override void Awake(InputComponent self, string a, string b)
        {
            self.Awake(a, b);
        }
    }

    public class InputComponent : Component
    {
        public event Action<InputAction.CallbackContext> onStarted;
        public event Action<InputAction.CallbackContext> onPerformed;
        public event Action<InputAction.CallbackContext> onCanceled;

        public string AssetName { get; private set; }
        public string MapName { get; private set; }
        public InputActionAsset InputAsset { get; private set; }
        public InputActionMap InputActionMap { get; private set; }
        public ReadOnlyArray<InputAction> Actions { get; private set; }

        public void Awake(string asset, string map) 
        {
            this.AssetName = asset;
            this.MapName = map;
            this.InputAsset = ResourcesHelper.GetAsset<InputActionAsset>($"Input/{AssetName}");
            this.InputActionMap = this.InputAsset.FindActionMap(this.MapName, true);
            this.Actions = this.InputActionMap.actions;
            foreach (var action in this.Actions)
            {
                action.Enable();
                action.started += OnStarted;
                action.performed += OnPerformed;
                action.canceled += OnCanceled;
            }
        }

        private void OnStarted(InputAction.CallbackContext ctx) 
        {
            Log.Debug("OnStarted");
            onStarted?.Invoke(ctx);
        }
        private void OnPerformed(InputAction.CallbackContext ctx) 
        {
            Log.Debug("onPerformed");
            onPerformed.Invoke(ctx);
        }
        private void OnCanceled(InputAction.CallbackContext ctx) 
        {
            Log.Debug("OnCanceled");
            onCanceled?.Invoke(ctx);
        }
    }
}
