using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEditor;

public class BuildAB : Editor
{
    public const string abDirectory = "Assets/AB";
    public const string abDirectoryMac = "Assets/AB/Mac";
    public const string abDirectoryWindows = "Assets/AB/Windows";
    public const string suffix = ".zjc";
    public const string menu = "My Editor";
    public const string title = "Build AB from window Project select";
    private static float uploadProgressFloat;
    public static void Build(Object[] selects, int typeId)
    {
        if (!Directory.Exists(abDirectory))
        {
            Directory.CreateDirectory(abDirectory);
        }
        if (!Directory.Exists(abDirectoryMac))
        {
            Directory.CreateDirectory(abDirectoryMac);
        }
        if (!Directory.Exists(abDirectoryWindows))
        {
            Directory.CreateDirectory(abDirectoryWindows);
        }
        EditorCoroutineRunner.StartEditorCoroutine(CoroutineBuild(selects, typeId));
    }
    private static IEnumerator CoroutineBuild(Object[] selects, int typeId)
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
        yield return ObjToAB(list.ToArray(), typeId);
    }
    private static IEnumerator ObjToAB(AssetBundleBuild[] buildArray, int typeId)
    {
        EditorUtility.DisplayCancelableProgressBar(title, "生成ab包", 0.0f);
        if (BuildPipeline.BuildAssetBundles(abDirectoryMac, buildArray, BuildAssetBundleOptions.None, BuildTarget.StandaloneOSX) &&
            BuildPipeline.BuildAssetBundles(abDirectoryWindows, buildArray, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64))
        {
            Debug.Log("build AB success");
            EditorUtility.DisplayCancelableProgressBar(title, "上传", 1.0f);
            int index = 0;
            while (index < buildArray.Length)
            {
                Debug.Log("获取文件：" + System.Environment.CurrentDirectory + '/' + abDirectoryMac + '/' + buildArray[index].assetBundleName);
                byte[] bytesMac = File.ReadAllBytes(System.Environment.CurrentDirectory + '/' + abDirectoryMac + '/' + buildArray[index].assetBundleName);
                Debug.Log("获取文件：" + System.Environment.CurrentDirectory + '/' + abDirectoryWindows + '/' + buildArray[index].assetBundleName);
                byte[] bytesWindows = File.ReadAllBytes(System.Environment.CurrentDirectory + '/' + abDirectoryWindows + '/' + buildArray[index].assetBundleName);
                WWWForm form = new WWWForm();
                form.AddField("name", buildArray[index].assetBundleName);
                form.AddField("typeId", typeId);
                form.AddBinaryData("fileWindows", bytesWindows);
                form.AddBinaryData("fileMac", bytesMac);
                uploadProgressFloat = 0;
                yield return MyWebRequset.IPost<string>("/model/upload", form, uploadProgress =>
                {
                    if (uploadProgress != uploadProgressFloat)
                    {
                        Debug.Log(uploadProgress);
                        uploadProgressFloat = uploadProgress;
                    }
                    EditorUtility.DisplayCancelableProgressBar(title, string.Format("上传{0}/{1}", index, buildArray.Length), uploadProgress);
                });
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
        BoxCollider boxCollider = BuildingUtil.AddBoxCollider(gameObj);
        //优化：对齐中心点
        if (boxCollider.center != Vector3.zero)
        {
            foreach (Transform t in gameObj.transform)
            {
                t.localPosition -= boxCollider.center;
            }
            boxCollider.center = Vector3.zero;
        }
        string newPath = assetPath.Replace(".", "-edit.");
        bool success;
        PrefabUtility.SaveAsPrefabAsset(gameObj, newPath, out success);
        DestroyImmediate(gameObj);
        Debug.Log(newPath + "    " + success);
        return newPath;
    }
}
