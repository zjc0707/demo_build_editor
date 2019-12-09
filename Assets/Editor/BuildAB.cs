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

    [MenuItem(menu + "/Build AB from window Project select ")]
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
            Debug.Log(index + "    " + obj.name);
            string path = AssetDatabase.GetAssetPath(obj);
            string newPrefabPath = EditBoxCollider(obj, path);
            while (AssetPreview.GetAssetPreview(obj) == null)
            {
                Debug.Log("Load texture2D_" + (wait++) + ":" + obj.name);
                yield return null;
            }
            Texture2D texture2D = AssetPreview.GetAssetPreview(obj);
            string newImgPath = path.Split('.')[0] + "-sprite.png";
            File.WriteAllBytes(newImgPath, texture2D.EncodeToPNG());
            AssetBundleBuild assetBundle = new AssetBundleBuild();
            assetBundle.assetBundleName = obj.name + suffix;
            assetBundle.assetNames = new string[] { newPrefabPath, newImgPath };
            list.Add(assetBundle);

            index++;
            wait = 0;
        }
        AssetDatabase.Refresh();
        ObjToAB(list.ToArray());
    }
    private static void ObjToAB(AssetBundleBuild[] buildArray)
    {
        // if (BuildPipeline.BuildAssetBundles(abDirectory, buildArray, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets
        //                          | BuildAssetBundleOptions.DeterministicAssetBundle, BuildTarget.StandaloneOSX))
        if (BuildPipeline.BuildAssetBundles(abDirectory, buildArray, BuildAssetBundleOptions.None, BuildTarget.StandaloneOSX))
        {
            Debug.Log("build AB success");
        }
        else
        {
            Debug.Log("build AB failure");
        }
        foreach (AssetBundleBuild build in buildArray)
        {
            foreach (string path in build.assetNames)
            {
                File.Delete(path);
            }
        }
        AssetDatabase.Refresh();
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
