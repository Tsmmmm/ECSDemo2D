using System;
using UnityEditor;
using UnityEngine;

public class GUIText
{
    /// <summary>
    /// 创建大标题
    /// </summary>
    /// <param name="title"></param>
    static public void LayoutHead(string title, int fontSize = 15, FontStyle style = FontStyle.Normal)
    {
        GUILayout.Space(10);
        GUIStyle gs = new GUIStyle();
        gs.fontSize = fontSize;
        gs.fontStyle = style;
        gs.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField(string.Format("<color=#000000>{0}</color>", title), gs);
        GUILayout.Space(10);
    }

    /// <summary>
    /// 用来划分参数类别
    /// </summary>
    /// <param name="title"></param>
    static public void LayoutSplit(string title, int fontSize = 15, FontStyle style = FontStyle.Normal)
    {
        GUILayout.Space(10);
        GUIStyle gs = new GUIStyle();
        gs.fontSize = fontSize;
        gs.fontStyle = style;
        gs.alignment = TextAnchor.MiddleLeft;
        EditorGUILayout.LabelField(string.Format("<color=#FF0000>{0}</color>", title), gs);
        GUILayout.Space(10);
    }

    /// <summary>
    /// 生成数组编辑界面，编辑完成会返回新的数组
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    static public string LayoutField(string title, string text)
    {
        return EditorGUILayout.TextField(title, text);
    }
}