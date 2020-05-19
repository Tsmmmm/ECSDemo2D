using UnityEngine;
using Live2D.Cubism.Framework.Json;
using Live2D.Cubism.Core;
using Live2D.Cubism.Rendering;
using Live2D.Cubism.Framework.Motion;
using Live2D.Cubism.Framework.MotionFade;
using System.Collections.Generic;
using Live2D.Cubism.Framework;
using System;
using LitJson;
using System.IO;

namespace ET
{
    [ObjectSystem]
    public class Live2dComponentAwakeSystem : AwakeSystem<Live2dComponent, ModelType, string>
    {
        public override void Awake(Live2dComponent self, ModelType a, string b)
        {
            self.Awake(a, b);
        }
    }

    public class Live2dComponent : Component
    {
        public ModelType Type { get; private set; }
        public string Name { get; private set; }
        public string URL { get; private set; }
        public Live2dModelConfig ModelConfig { get; private set; }
        public CubismModel Model { get; private set; }
        public Dictionary<string, AnimationClip> Animations { get; private set; }
        private CubismFadeController fadeController;
        private CubismMotionController motionController;
        public void Awake(ModelType type, string name)
        {
            try
            {
                Type = type;
                Name = name;
                URL = Live2dPathHelper.GetModelConfigPath(type, name);
                TextAsset config = ResourcesHelper.GetAsset<TextAsset>(URL);
                ModelConfig = JsonMapper.ToObject<Live2dModelConfig>(config.text);
                ModelConfig.AssetPath = Live2dPathHelper.GetAssetPath(type, name);
                string modelPath = ModelConfig.GetModelPath();
                var model3Json = CubismModel3Json.LoadAtPath(modelPath, BuiltinLoadAssetAtPath);
                Model = model3Json.ToModel();
                EntityRef.GameObject = Model.gameObject;
                EntityRef.Transform = Model.gameObject.transform;
                EntityRef.GameObject.layer = LayerHelper.LayerNameToLayer("Unit");
                GameObjectHelper.Traversal(EntityRef.Transform, (arg) =>
                {
                    arg.gameObject.layer = LayerHelper.LayerNameToLayer("Unit");
                }, true);

                Animations = new Dictionary<string, AnimationClip>();
                fadeController = EntityRef.GameObject.AddComponent<CubismFadeController>();
                fadeController.CubismFadeMotionList = ScriptableObject.CreateInstance<CubismFadeMotionList>();
                fadeController.CubismFadeMotionList.MotionInstanceIds = new int[ModelConfig.Motions.Length];
                fadeController.CubismFadeMotionList.CubismFadeMotionObjects = new CubismFadeMotionData[ModelConfig.Motions.Length];

                for (int i = 0; i < ModelConfig.Motions.Length; ++i)
                {
                    string motion = ModelConfig.Motions[i];
                    string motionPath = ModelConfig.GetMotionPath(motion);
                    var motion3Json = CubismMotion3Json.LoadFrom(ResourcesHelper.GetAsset<TextAsset>(motionPath));
                    var clip = ResourcesHelper.GetAsset<AnimationClip>(ModelConfig.GetAnimationClipPath(motion));
                    clip.name = motion;

                    int instanceId = clip.GetInstanceID();
                    var events = clip.events;
                    for (var j = 0; j < events.Length; ++j)
                    {
                        if (events[j].functionName != "InstanceId")
                        {
                            continue;
                        }

                        instanceId = events[j].intParameter;
                    }

                    fadeController.CubismFadeMotionList.MotionInstanceIds[i] = instanceId;
                    fadeController.CubismFadeMotionList.CubismFadeMotionObjects[i] = CubismFadeMotionData.CreateInstance(motion3Json, clip.name, clip.length);

                    Animations.Add(motion, clip);
                }

                motionController = EntityRef.GameObject.AddComponent<CubismMotionController>();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public string curName;
        public void PlayAnimation(string anim, int layerIndex = 0, int priority = CubismMotionPriority.PriorityForce, bool isLoop = true, float speed = 1)
        {
            if (anim == curName) 
            {
                return;
            }

            AnimationClip clip;
            if (!this.Animations.TryGetValue(anim, out clip))
            {
                Log.Error($"{anim} is null");
                return;
            }

            motionController.PlayAnimation(clip, layerIndex, priority, isLoop, speed);
        }

        public override void Dispose()
        {
            base.Dispose();

            UnityEngine.Object.Destroy(EntityRef.GameObject);
        }

        private object BuiltinLoadAssetAtPath(Type assetType, string absolutePath)
        {
            if (assetType == typeof(byte[]))
            {
                return ResourcesHelper.GetAsset<TextAsset>(absolutePath).bytes;
            }
            else if (assetType == typeof(string))
            {
                return ResourcesHelper.GetAsset<TextAsset>(absolutePath).text;
            }
            else if (assetType == typeof(Texture2D))
            {
                string path = absolutePath.Replace(".png", "");
                return ResourcesHelper.GetAsset<Texture2D>(path);
            }

            throw new NotSupportedException();
        }
    }
}
