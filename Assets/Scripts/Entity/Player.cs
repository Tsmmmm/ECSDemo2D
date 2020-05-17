namespace ET 
{
    [ObjectSystem]
    public class PlayerAwakeSystem : AwakeSystem<Player, string>
    {
        public override void Awake(Player self, string a)
        {
            self.Awake(a);
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