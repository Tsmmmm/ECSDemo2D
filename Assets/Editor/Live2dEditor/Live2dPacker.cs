using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Live2D.Cubism.Framework.Json;
using System;

namespace Game.Editor
{
    public class Live2dEditor : EditorWindow
    {
        private string modelPath;
        private string exportPath;

        private string DefaultModelDir = @"D:\workspace\Projects\ECSDemo2D\Assets\Res\Model";
        private string DefaultExportDir = @"D:\workspace\Projects\ECSDemo2D\Assets\Resources\Model";
        Live2dEditor()
        {
            this.titleContent = new GUIContent("live2d packer");
        }

        [MenuItem("Live2D/Packer")]
        static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(Live2dEditor));
        }

        void OnGUI()
        {
            GUIText.LayoutHead("LIVE2D TOOLS", 20);
            GUIText.LayoutSplit(">>选择要打包的模型根文件夹", 15);
            modelPath = GUIFolderSelect.OnGUI("", 0, modelPath ?? DefaultModelDir, onSelect: (arg) =>
            {
                arg = FileSystem.StandardizeSlashSeparator(arg);
                Debug.Log($"已选择模型文件夹:{arg}");
                return arg;
            });
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            if (GUILayout.Button("导出"))
            {
                Export();
            }
            GUILayout.EndVertical();
        }

        private void Export()
        {
            Debug.Log($"已选择模型文件夹:{modelPath}");
            Debug.Log($"***开始导出配置文件");
            string[] models = Directory.GetDirectories(modelPath);
            foreach (string modelPath in models)
            {
                try
                {
                    string root = modelPath.Substring(DefaultModelDir.Length+1);
                    string modelName = Path.GetFileNameWithoutExtension(modelPath);
                    string model3jsonPath = $"{modelName}.model3";
                    string[] motion3jsonPaths = Directory.GetFiles($"{modelPath}/motions/", "*.motion3.json", SearchOption.TopDirectoryOnly);
                    Live2dData data = new Live2dData();
                    data.Model = FileSystem.StandardizeBackslashSeparator(model3jsonPath);
                    data.Moc = $"{modelName}.moc3";
                    data.Cdi = $"{modelName}.cdi3";
                    data.Texture = "texture_00";
                    data.Motions = new string[motion3jsonPaths.Length];
                    for(int i = 0; i < motion3jsonPaths.Length; ++i)
                    {
                        string motion = Path.GetFileNameWithoutExtension(motion3jsonPaths[i]);
                        motion = FileSystem.StandardizeBackslashSeparator(motion);
                        data.Motions[i] = motion.Replace(".motion3", "");
                    }
                    string jsonData = LitJson.JsonMapper.ToJson(data);

                    exportPath = $"{DefaultExportDir}/{root}/{modelName}Config.json";

                    string dir = Path.GetDirectoryName(exportPath);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    if (!File.Exists(exportPath))
                    {
                        File.Create(exportPath).Close();
                    }

                    string desModelPath = $"{DefaultExportDir}/{root}";
                    FileSystem.CopyDirectory(modelPath, desModelPath, (arg) => 
                    {
                        if (arg.IndexOf(".motion3") >= 0 && arg.IndexOf(".meta") == -1)
                        {
                            ParseMotion3Json(root, arg);
                            return arg;
                        }

                        if (arg.IndexOf(".moc3") >= 0 && arg.IndexOf(".meta") == -1)
                        {
                            return $"{arg}.bytes";
                        }
                        return arg;
                    });
                    File.WriteAllText(exportPath, jsonData, Encoding.UTF8);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"{modelPath} export faild with {e.Message}");
                }
            }
            AssetDatabase.Refresh();
            Debug.Log($"***导出成功");
        }

        private void ParseMotion3Json(string root, string path)
        {
            bool ShouldClearAnimationCurves = false;
            bool ShouldImportAsOriginalWorkflow = false;

            string clipName = path.Replace(".motion3.json", "");
            string srcPath = $"Assets{DefaultModelDir}\\{root}\\motions\\{path}".Substring(Application.dataPath.Length);
            string desPath = $"Assets{DefaultExportDir}\\{root}\\motions\\{clipName}.anim".Substring(Application.dataPath.Length);

            var Motion3Json = CubismMotion3Json.LoadFrom(AssetDatabase.LoadAssetAtPath<TextAsset>(srcPath).text);
            var clip = (ShouldImportAsOriginalWorkflow) 
                        ? AssetDatabase.LoadAssetAtPath<AnimationClip>(srcPath.Replace(".motion3.json", ".anim")) 
                        : null;

            // Convert motion.
            var animationClip = (clip == null)
                                ? Motion3Json.ToAnimationClip(ShouldImportAsOriginalWorkflow, ShouldClearAnimationCurves)
                                : Motion3Json.ToAnimationClip(clip, ShouldImportAsOriginalWorkflow, ShouldClearAnimationCurves);

            AssetDatabase.CreateAsset(animationClip, desPath);

            var index = -1;
            var sourceAnimationEvents = AnimationUtility.GetAnimationEvents(animationClip);

            for (var i = 0; i < sourceAnimationEvents.Length; ++i)
            {
                if (sourceAnimationEvents[i].functionName != "InstanceId")
                {
                    continue;
                }

                index = i;
                break;
            }

            if (index == -1)
            {
                index = sourceAnimationEvents.Length;
                Array.Resize(ref sourceAnimationEvents, sourceAnimationEvents.Length + 1);
                sourceAnimationEvents[sourceAnimationEvents.Length - 1] = new AnimationEvent();
            }

            int instanceId = animationClip.GetInstanceID();

            sourceAnimationEvents[index].time = 0;
            sourceAnimationEvents[index].functionName = "InstanceId";
            sourceAnimationEvents[index].intParameter = instanceId;
            sourceAnimationEvents[index].messageOptions = SendMessageOptions.DontRequireReceiver;

            AnimationUtility.SetAnimationEvents(animationClip, sourceAnimationEvents);

            var assetImporter = AssetImporter.GetAtPath(srcPath);
            assetImporter.userData = JsonUtility.ToJson(this);
            assetImporter.SaveAndReimport();
        }
    }
}
