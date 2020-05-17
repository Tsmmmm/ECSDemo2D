namespace ET
{
    public class Live2dModelConfig
    {
        public string AssetPath;
        public string Model;
        public string Moc;
        public string Cdi;
        public string Texture;
        public string[] Motions;

        public string GetModelPath()
        {
            return $"{AssetPath}{Model}";
        }

        public string GetMotionPath(string motion)
        {
            return $"{AssetPath}motions/{motion}.motion3";
        }

        public string GetAnimationClipPath(string motion)
        {
            return $"{AssetPath}motions/{motion}";
        }
    }
}