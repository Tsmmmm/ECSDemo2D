using System;
using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

public class GUIOrderList
{
    public static void LayoutOrderList<T>(IList elements, int elementHeight = 18, bool draggable = true, bool displayHeader = true, bool displayAddButton = true, bool displayRemoveButton = true)
    {
        GUILayout.Space(10);
        ReorderableList reorderableList = new ReorderableList(elements, typeof(T), draggable, displayAddButton, displayHeader, displayRemoveButton);
        reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            var element = elements[index];
            EditorGUI.RectField(rect, rect);
        };
        reorderableList.DoLayoutList();
    }
}
