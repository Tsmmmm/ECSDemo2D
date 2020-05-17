using UnityEngine.InputSystem;

namespace ET 
{
    [ObjectSystem]
    public class PlayerAwakeSystem : AwakeSystem<Player, string>
    {
        public override void Awake(Player self, string a)
        {
            self.Awake(a);
            Live2dComponent live2d = ComponentFactory.CreateWithEntity<Live2dComponent, ModelType, string>(self, ModelType.Player, a);
            live2d.PlayAnimation("Idle");
            InputComponent input = ComponentFactory.CreateWithEntity<InputComponent>(self);
            input.OnCodeDown += () => 
            {
                live2d.PlayAnimation("Run", priority:3);
            };
        }
    }

    public class Player : Entity 
    {
        public void Awake(string name) 
        {
            this.Name = name;
        }
    }
}