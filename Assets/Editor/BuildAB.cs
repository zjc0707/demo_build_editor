using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEditor;

public class BuildAB : Editor
{
    public const string abDirectory = "Assets/AB";
    public const string suffix = ".zjc";
    public const string menu = "My Editor";
    public const string title = "Build AB from window Project select";

    [MenuItem(menu + "/Build AB from window Project select")]
    static void Build()
    {
        if (!Directory.Exists(abDirectory))
        {
            Directory.CreateDirectory(abDirectory);
        }
        Object[] selects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        EditorCoroutineRunner.StartEditorCoroutine(CoroutineBuild(selects));
    }
    private static IEnumerator CoroutineBuild(Object[] selects)
    {
        Debug.Log("Start");
        List<AssetBundleBuild> list = new List<AssetBundleBuild>();
        int index = 0;
        int wait = 0;
        while (index < selects.Length)
        {
            Object obj = selects[index];
            EditorUtility.DisplayCancelableProgressBar(title, string.Format("获取文件[{0}/{1}]:{2}", index, selects.Length, obj.name), 0);
            string path = AssetDatabase.GetAssetPath(obj);
            EditorUtility.DisplayCancelableProgressBar(title, string.Format("预处理模型[{0}/{1}]:{2}", index, selects.Length, obj.name), 0.2f);
            string newPrefabPath = EditBoxCollider(obj, path);
            EditorUtility.DisplayCancelableProgressBar(title, string.Format("生成模型缩略图[{0}/{1}]:{2}", index, selects.Length, obj.name), 0.4f);
            while (AssetPreview.GetAssetPreview(obj) == null)
            {
                Debug.Log("Load texture2D_" + (wait++) + ":" + obj.name);
                yield return null;
            }
            Texture2D texture2D = AssetPreview.GetAssetPreview(obj);
            string newImgPath = path.Split('.')[0] + "-sprite.png";
            EditorUtility.DisplayCancelableProgressBar(title, string.Format("模型缩略图写入本地[{0}/{1}]:{2}", index, selects.Length, obj.name), 0.6f);
            File.WriteAllBytes(newImgPath, texture2D.EncodeToPNG());

            AssetBundleBuild assetBundle = new AssetBundleBuild();
            assetBundle.assetBundleName = obj.name + suffix;
            assetBundle.assetNames = new string[] { newPrefabPath, newImgPath };
            list.Add(assetBundle);

            index++;
            wait = 0;
        }
        AssetDatabase.Refresh();
        yield return ObjToAB(list.ToArray());
    }
    private static IEnumerator ObjToAB(AssetBundleBuild[] buildArray)
    {
        EditorUtility.DisplayCancelableProgressBar(title, "生成ab包", 0.0f);
        if (BuildPipeline.BuildAssetBundles(abDirectory, buildArray, BuildAssetBundleOptions.None, BuildTarget.StandaloneOSX))
        {
            Debug.Log("build AB success");
            EditorUtility.DisplayCancelableProgressBar(title, "上传", 1.0f);
            int index = 0;
            while (index < buildArray.Length)
            {
                Debug.Log("获取文件：" + System.Environment.CurrentDirectory + '/' + abDirectory + '/' + buildArray[index].assetBundleName);
                byte[] bytes = File.ReadAllBytes(System.Environment.CurrentDirectory + '/' + abDirectory + '/' + buildArray[index].assetBundleName);
                Debug.Log("大小：" + bytes.Length * 1.0f / 1024 + "kb");
                WWWForm form = new WWWForm();
                form.AddField("name", buildArray[index].assetBundleName);
                form.AddBinaryData("file", bytes);
                UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1:4567/unity/model/upload", form);
                www.SendWebRequest();
                while (www.isDone)
                {
                    Debug.Log(www.uploadProgress);
                    EditorUtility.DisplayCancelableProgressBar(title, string.Format("上传{0}/{1}", index, buildArray.Length), www.uploadProgress);
                    yield return 1;
                }
                index++;
            }
        }
        else
        {
            Debug.Log("build AB failure");
        }
        EditorUtility.DisplayCancelableProgressBar(title, "清理缓存", 1.0f);
        foreach (AssetBundleBuild build in buildArray)
        {
            foreach (string path in build.assetNames)
            {
                File.Delete(path);
            }
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog(title, "上传完成", "确定");
    }
    /// <summary>
    /// 计算包围盒，再另存为带-edit后缀的新prefab
    /// </summary>
    /// <param name="obj"></param>
    /// <returns>新prefab的路径</returns>
    private static string EditBoxCollider(Object obj, string assetPath)
    {
        GameObject gameObj = PrefabUtility.InstantiatePrefab(obj) as GameObject;
        if (gameObj.transform.localScale != Vector3.one)
        {
            GameObject parent = new GameObject(gameObj.name);
            parent.transform.position = gameObj.transform.position;
            gameObj.transform.SetParent(parent.transform);
            gameObj = parent;
        }
        BuildingUtil.AddBoxCollider(gameObj);
        string newPath = assetPath.Replace(".", "-edit.");
        bool success;
        PrefabUtility.SaveAsPrefabAsset(gameObj, newPath, out success);
        DestroyImmediate(gameObj);
        Debug.Log(newPath + "    " + success);
        return newPath;
    }
}
