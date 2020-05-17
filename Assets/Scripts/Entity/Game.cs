namespace ET 
{
    public static class Game 
    {
        private static Scene scene;
        public static Scene Scene 
        {
            get 
            {
                return scene ?? (scene = EntityFactory.Create<Scene>());
            }
        }

        public static ObjectPool ObjectPool
        {
            get
            {
                return ObjectPool.Instance;
            }
        }

        public static EventSystem EventSystem
        {
            get
            {
                return EventSystem.Instance;
            }
        }

        public static void Close()
        {
            scene?.Dispose();
            scene = null;
            ObjectPool.Instance.Dispose();
            EventSystem.Instance.Dispose();
        }
    }
}