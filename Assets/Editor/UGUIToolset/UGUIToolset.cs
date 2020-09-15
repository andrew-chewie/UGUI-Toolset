using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine.UI;

public class UGUIToolset : EditorWindow
{
    [MenuItem("Window/UGUI Toolset")]
    public static void ShowWindow()
    {
        GetWindow(typeof(UGUIToolset));
    }

    private class LayoutData
    {
        public UILayout LayoutType = UILayout.Vertical;
        public RectOffset padding = new RectOffset();
        public TextAnchor childAlignment = TextAnchor.UpperLeft;
        public bool childForceExpandHeight;
        public bool childForceExpandWidth;
        public float spacing;
        
        public LayoutData()
        {

        }

        public LayoutData(HorizontalOrVerticalLayoutGroup layoutGroup)
        {
            if (layoutGroup != null)
            {
                if (layoutGroup.GetComponent<VerticalLayoutGroup>() != null)
                    LayoutType = UILayout.Vertical;

                if (layoutGroup.GetComponent<HorizontalLayoutGroup>() != null)
                    LayoutType = UILayout.Horizontal;

                spacing = layoutGroup.spacing;
                padding = layoutGroup.padding;
                childAlignment = layoutGroup.childAlignment;
                childForceExpandHeight = layoutGroup.childForceExpandHeight;
                childForceExpandWidth = layoutGroup.childForceExpandWidth;
            }
            else
            {
                new LayoutData();
            }
        }
    }

    private enum UILayout
    {
        Vertical,
        Horizontal
    }

    private GameObject SelectedObject
    {
        get
        {
            if (_selectedObjects == null || _selectedObjects.Count == 0)
                return null;

            return _selectedObjects[0].gameObject;
        }
    }

    private List<Transform> _selectedObjects;
    private List<Transform> SelectedObjects
    {
        get {
            if (IncludeChild)
            {
                var result = new HashSet<Transform>();
                foreach (Transform o in _selectedObjects)
                {
                    result.Add(o);
                    if (IncludeRecursively)
                    {
                        foreach (Transform c in o.GetComponentsInChildren<Transform>(true))
                        {
                            result.Add(c);
                        }
                    }
                    else
                    {
                        foreach (Transform c in o.transform)
                        {
                            result.Add(c);
                        }
                    }
                }

                result.RemoveWhere(transform => transform.GetComponent<RectTransform>() == null);

                return new List<Transform>(result);
            }

            return _selectedObjects;
        }
        set { _selectedObjects = value; }
    }

    private bool IncludeChild = false;
    private bool IncludeRecursively = false;
    
    void OnEnable()
    {
        SceneView.onSceneGUIDelegate += OnSceneGui;
    }

    private void OnSceneGui(SceneView sceneView)
    {
        Event e = Event.current;

        Repaint();
    }

    float buttonWidth = 100;
    float buttonHeight = 30;

    private void OnGUI()
    {
        SelectedObjects = new List<Transform>(Selection.transforms);

        IncludeChild = GUILayout.Toggle(IncludeChild, "Include Child Objects", GUILayout.MinWidth(buttonWidth),GUILayout.MinHeight(20));
        
        if(IncludeChild)
            IncludeRecursively = GUILayout.Toggle(IncludeRecursively, "Include Recursively", GUILayout.MinWidth(buttonWidth),GUILayout.MinHeight(20));
        
        DrawUILine(Color.black);

        if (GUILayout.Button("Group selected", GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
        {
            GroupSelected();
        }
        if (GUILayout.Button("Add Child", GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
        {
            AddEmptyChild();
        }

        GUI.color = Color.white;

        if (!WrongObject && SelectedObject.GetComponent<VerticalLayoutGroup>())
            GUI.color = Color.green;

        if (GUILayout.Button("V Group", GUILayout.MinWidth(buttonWidth),GUILayout.MinHeight(buttonHeight)))
        {
            SetLayout(UILayout.Vertical);
        }

        GUI.color = Color.white;

        if (!WrongObject && SelectedObject.GetComponent<HorizontalLayoutGroup>())
            GUI.color = Color.green;

        if (GUILayout.Button("H Group", GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
        {
            SetLayout(UILayout.Horizontal);
        }

        GUI.color = Color.white;

        if (GUILayout.Button("Flip layout", GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
        {
            FlipLayout();
        }

        GUI.color = Color.white;

        if (!WrongObject && SelectedObject.GetComponent<ContentSizeFitter>())
            GUI.color = Color.green;

        if (GUILayout.Button("Content Size", GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
        {
            AddContentSizeFitter();
        }

        GUI.color = Color.white;

        if (!WrongObject && SelectedObject.GetComponent<LayoutElement>())
            GUI.color = Color.green;

        if (GUILayout.Button("Layout Element", GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
        {
            AddUIComponent(typeof(LayoutElement));
        }

        GUI.color = Color.white;

        if (!WrongObject && SelectedObject.GetComponent<Image>())
            GUI.color = Color.green;

        if (GUILayout.Button("Add Image", GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
        {
            AddUIComponent(typeof(Image));
        }

        GUI.color = Color.white;

        if (!WrongObject && SelectedObject.GetComponent<Button>())
            GUI.color = Color.green;

        if (!WrongObject && SelectedObject.GetComponent<Button>())
            GUI.color = Color.green;

        if (GUILayout.Button("Add Button", GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
        {
            AddUIComponent(typeof(Button));
        }

        DrawUILine(Color.black);
        
        GUI.color = Color.white;

        if (GUILayout.Button("Duplicate", GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
        {
            Duplicate();
        }

        if (GUILayout.Button("V-Align", GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
        {
            AlignV();
        }

        if (GUILayout.Button("H-Align", GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
        {
            AlignH();
        }

        if (GUILayout.Button("V-Spacing", GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
        {
            SpacingV();
        }
        
        if (GUILayout.Button("H-Spacing", GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
        {
            SpacingH();
        }

        if (GUILayout.Button("TXT -> TMP", GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
        {
            SwitchToTMP();
        }
        
        DrawUILine(Color.black);
    }

    public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding / 2;
        r.x -= 2;
        r.width += 6;
        EditorGUI.DrawRect(r, color);
    }

    private void GroupSelected()
    {
        if(SelectedObjects == null || SelectedObjects.Count == 0)
            return;

        var parent = SelectedObjects[0].parent;

        GameObject groupObject = new GameObject("Group", typeof(RectTransform));
        groupObject.transform.SetParent(parent.transform);
        groupObject.transform.localPosition = Vector3.zero;
        groupObject.transform.localScale = Vector3.one;

        foreach (Transform o in SelectedObjects)
        {
            o.SetParent(groupObject.transform);
        }
    }

    private void SwitchToTMP()
    {
        foreach (Transform o in SelectedObjects)
        {
            var text = o.GetComponent<Text>();
            if(text == null)
                continue;

            var textTmpText = text.text;
            var textTmpFontSize = text.fontSize;
            var textTmpSize = text.rectTransform.sizeDelta;
            var textTmpAlign = text.alignment;

            DestroyImmediate(text);

            TextMeshProUGUI textTMP = o.gameObject.AddComponent<TextMeshProUGUI>();

            textTMP.text = textTmpText;
            textTMP.fontSize = textTmpFontSize;
            textTMP.rectTransform.sizeDelta = textTmpSize;

            switch (textTmpAlign)
            {
                case TextAnchor.UpperLeft:
                    textTMP.alignment = TextAlignmentOptions.MidlineLeft;
                    break;
                case TextAnchor.UpperCenter:
                    textTMP.alignment = TextAlignmentOptions.Top;
                    break;
                case TextAnchor.UpperRight:
                    textTMP.alignment = TextAlignmentOptions.TopRight;
                    break;
                case TextAnchor.MiddleLeft:
                    textTMP.alignment = TextAlignmentOptions.MidlineLeft;
                    break;
                case TextAnchor.MiddleCenter:
                    textTMP.alignment = TextAlignmentOptions.Center;
                    break;
                case TextAnchor.MiddleRight:
                    textTMP.alignment = TextAlignmentOptions.MidlineRight;
                    break;
                case TextAnchor.LowerLeft:
                    textTMP.alignment = TextAlignmentOptions.BottomLeft;
                    break;
                case TextAnchor.LowerCenter:
                    textTMP.alignment = TextAlignmentOptions.Bottom;
                    break;
                case TextAnchor.LowerRight:
                    textTMP.alignment = TextAlignmentOptions.BottomRight;
                    break;
            }
        }
    }

    private void SpacingH()
    {
        if (SelectedObjects.Count == 0)
            return;

        int count = SelectedObjects.Count;
        float spacing;
        float minPos = Single.MaxValue;
        float maxPos = -Single.MaxValue;

        foreach (Transform o in SelectedObjects)
        {
            Vector2 anchoredPosition = o.GetComponent<RectTransform>().anchoredPosition;
            if (anchoredPosition.x > maxPos)
            {
                maxPos = anchoredPosition.x;
            }

            if (anchoredPosition.x < minPos)
            {
                minPos = anchoredPosition.x;
            }
        }

        spacing = (maxPos - minPos)/(count-1);

        for (int i = 0; i < count; i++)
        {
            Vector2 anchoredPosition = SelectedObjects[i].GetComponent<RectTransform>().anchoredPosition;
            SelectedObjects[i].GetComponent<RectTransform>().anchoredPosition =
                new Vector2(minPos + i * spacing, anchoredPosition.y);
        }
    }

    private void SpacingV()
    {
        if (SelectedObjects.Count == 0)
            return;

        int count = SelectedObjects.Count;
        float spacing;
        float minPos = Single.MaxValue;
        float maxPos = -Single.MaxValue;

        foreach (Transform o in SelectedObjects)
        {
            Vector2 anchoredPosition = o.GetComponent<RectTransform>().anchoredPosition;
            if (anchoredPosition.y > maxPos)
            {
                maxPos = anchoredPosition.y;
            }

            if (anchoredPosition.y < minPos)
            {
                minPos = anchoredPosition.y;
            }
        }

        spacing = (maxPos - minPos) / (count - 1);

        for (int i = 0; i < count; i++)
        {
            Vector2 anchoredPosition = SelectedObjects[i].GetComponent<RectTransform>().anchoredPosition;
            SelectedObjects[i].GetComponent<RectTransform>().anchoredPosition =
                new Vector2(anchoredPosition.x, minPos + i * spacing);
        }
    }

    private void AlignH()
    {
        if (SelectedObjects.Count == 0)
            return;

        Transform first = SelectedObjects[0];

        foreach (Transform o in SelectedObjects)
        {
            o.transform.position = new Vector3(o.transform.position.x, first.transform.position.y);
        }
    }

    private void AlignV()
    {
        if(SelectedObjects.Count == 0)
            return;

        Transform first = SelectedObjects[0];

        foreach (Transform o in SelectedObjects)
        {
            o.transform.position = new Vector3(first.transform.position.x, o.transform.position.y);
        }
    }

    private void FlipLayout()
    {
        foreach (Transform t in SelectedObjects)
        {
            FlipLayout(t);
        }
    }

    private void FlipLayout(Transform t)
    {
        if (t.GetComponent<HorizontalLayoutGroup>())
        {
            SetLayout(t,UILayout.Vertical);
        }
        else if (t.GetComponent<VerticalLayoutGroup>())
        {
            SetLayout(t,UILayout.Horizontal);
        }
    }

    private void Duplicate()
    {
        if(WrongObject)
            return;

        GameObject go = Instantiate(SelectedObject);
        go.transform.SetParent(SelectedObject.transform.parent);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;

        RectTransform rectTransform = go.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;
        go.name = SelectedObject.name;

        Selection.activeGameObject = go;
    }

    private void AddUIComponent(Type type)
    {
        foreach (Transform o in SelectedObjects)
        {
            AddUIComponent(o, type);
        }
    }

    private Component AddUIComponent(Transform transform, Type type)
    {
        var component = transform.gameObject.GetComponent(type);
        if (component == null)
        {
            component = transform.gameObject.AddComponent(type);
        }
        else
        {
            DestroyImmediate(component);
        }

        return component;
    }

    private void AddEmptyChild()
    {
        foreach (var o in SelectedObjects)
        {
            GameObject go = new GameObject("Empty Child", typeof(RectTransform));
            go.transform.SetParent(o.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;

            RectTransform rectTransform = go.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = Vector2.zero;

            Selection.activeGameObject = go;
        }
    }

    private void AddContentSizeFitter()
    {
        foreach (var o in SelectedObjects)
        {
            AddUIComponent(typeof(ContentSizeFitter));
            ContentSizeFitter contentSizeFitter = o.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter != null)
            {
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }
    }

    private void SetLayout(UILayout layoutType)
    {
        foreach (var t in SelectedObjects)
        {
            SetLayout(t,layoutType);
        }
    }

    private void SetLayout(Transform t,UILayout layoutType)
    {
        var layoutGroup = t.GetComponent<HorizontalOrVerticalLayoutGroup>();
        LayoutData newLayoutData = new LayoutData(layoutGroup);

        switch (layoutType)
        {
            case UILayout.Vertical:
                newLayoutData.LayoutType = UILayout.Vertical;
                break;
            case UILayout.Horizontal:
                newLayoutData.LayoutType = UILayout.Horizontal;
                break;
        }

        CreateLayout(t, newLayoutData);
    }

    private void CreateLayout(Transform t, LayoutData layoutData)
    {
        var layoutGroup = t.GetComponent<HorizontalOrVerticalLayoutGroup>();

        if (layoutGroup != null)
            DestroyImmediate(layoutGroup);

        if (layoutData.LayoutType == UILayout.Vertical && !(layoutGroup is VerticalLayoutGroup))
        {
            layoutGroup = (HorizontalOrVerticalLayoutGroup) AddUIComponent(t, typeof(VerticalLayoutGroup));
        }
        else if (layoutData.LayoutType == UILayout.Horizontal && !(layoutGroup is HorizontalLayoutGroup))
        {
            layoutGroup = (HorizontalOrVerticalLayoutGroup) AddUIComponent(t, typeof(HorizontalLayoutGroup));
        }

        SetLayout(layoutGroup, layoutData);
    }

    private void SetLayout(HorizontalOrVerticalLayoutGroup layoutGroup, LayoutData layoutData)
    {
        if(layoutGroup == null)
            return;

        layoutGroup.childForceExpandHeight = layoutData.childForceExpandHeight;
        layoutGroup.childForceExpandWidth = layoutData.childForceExpandWidth;
        layoutGroup.padding = layoutData.padding;
        layoutGroup.childAlignment = layoutData.childAlignment;
        layoutGroup.spacing = layoutData.spacing;
    }

    public bool WrongObject
    {
        get
        {
            if (SelectedObject == null)
                return true;
            if (SelectedObject.GetComponent<RectTransform>() == null)
                return true;
            return false;
        }
    }
}
