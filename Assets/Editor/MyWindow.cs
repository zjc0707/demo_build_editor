using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
public class MyWindow : EditorWindow
{
    // public static string fileName = "test";
    public string modelTypeName = "";
    public Vector2 scrollPos;
    // public string[] types = { "a", "b", "c" };
    public List<ModelType> modelTypes;
    public int index;
    public bool isError;
    public bool isAddModelType;
    [MenuItem(BuildAB.menu + "/open window")]
    static void Init()
    {
        MyWindow window = GetWindow<MyWindow>();
        window.position = new Rect(50, 50, 250, 250);
        Debug.Log("init");
    }
    private void OnEnable()
    {
        Debug.Log("OnEnable");
        GetModelTypeList();
        isAddModelType = false;
    }

    public void GetModelTypeList()
    {
        EditorCoroutineRunner.StartEditorCoroutine(MyWebRequset.IGet<List<ModelType>>("/modelType/findList", null, null, datas =>
        {
            modelTypes = datas;
            Repaint();
        }));
    }
    private void OnGUI()
    {
        isError = false;
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.BeginVertical();

        GUILayout.Label("Build AB", EditorStyles.boldLabel);
        Object[] selects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        for (int i = 0; i < selects.Length; i++)
        {
            EditorGUILayout.LabelField("fileName" + i, selects[i].name);
            EditorGUILayout.LabelField("path" + i, AssetDatabase.GetAssetPath(selects[i]));
        }
        if (selects.Length == 0)
        {
            EditorGUILayout.HelpBox("请选择文件", MessageType.Warning);
            isError = true;
        }
        GUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();
        if (modelTypes != null && modelTypes.Count > 0)
        {
            EditorGUILayout.LabelField("Type");
            index = EditorGUILayout.Popup(index, modelTypes.Select(s => s.Name).ToArray());
        }
        else
        {
            EditorGUILayout.HelpBox("请求后台数据中", MessageType.Info);
        }
        EditorGUILayout.EndHorizontal();

        if (isAddModelType)
        {
            EditorGUILayout.BeginHorizontal();
            modelTypeName = EditorGUILayout.TextField("modelTypeName", modelTypeName);
            if (GUILayout.Button("Add"))
            {
                if (!string.IsNullOrEmpty(modelTypeName))
                {
                    Debug.Log(modelTypeName);
                    isAddModelType = false;
                    EditorCoroutineRunner.StartEditorCoroutine(MyWebRequset.IGet<ModelType>("/modelType/add?name=" + modelTypeName, uploadProgress =>
                    {
                        EditorUtility.DisplayCancelableProgressBar("get", "", uploadProgress);
                    }, null, success =>
                    {
                        EditorUtility.ClearProgressBar();
                        Debug.Log(Json.Serialize(success));
                        modelTypes.Add(success);
                    }));
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            if (GUILayout.Button("Add new modelType"))
            {
                isAddModelType = true;
            }
        }

        GUILayout.Space(20);
        if (GUILayout.Button("submit"))
        {
            if (isError)
            {
                EditorUtility.DisplayDialog("error", "see the error message in helpBox", "close");
            }
            else
            {
                BuildAB.Build(selects, modelTypes[index].Id);
            }

        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }
}