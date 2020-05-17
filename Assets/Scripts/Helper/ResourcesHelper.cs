using UnityEngine;

namespace ET
{
    public static class ResourcesHelper
    {
        public static T GetAsset<T>(string path) where T : UnityEngine.Object
        {
            return Resources.Load<T>(path);
        }
    }
}
