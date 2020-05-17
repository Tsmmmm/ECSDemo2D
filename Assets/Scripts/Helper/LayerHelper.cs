using UnityEngine;

namespace ET
{
    public static class LayerHelper
    {
        public static int LayerNameToLayer(string name)
        {
            return LayerMask.NameToLayer(name);
        }

        public static string LayerToLayerName(int layer)
        {
            return LayerMask.LayerToName(layer);
        }
    }
}