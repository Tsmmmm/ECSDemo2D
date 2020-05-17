using System;
using System.IO;
using UnityEngine;

namespace ET
{
    public static class Live2dPathHelper
    {
        public static string GetAssetPath(ModelType type, string name)
        {
            string root = null;
            switch (type)
            {
                case ModelType.Player:
                    root = "Model/";
                    break;
            }

            if (root == null)
            {
                Log.Error($"未定义路径的Model类型：{type}");
                throw new InvalidCastException();
            }
            return $"{root}{name}/";
        }

        public static string GetModelConfigPath(ModelType type, string name)
        {
            string root = GetAssetPath(type, name);
            return $"{root}{name}Config";
        }
    }
}
