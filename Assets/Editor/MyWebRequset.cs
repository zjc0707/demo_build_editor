using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using System;
using UnityEditor;
public class MyWebRequset
{
    // private const int TIMEOUT = 10;
    // private const string ip = "http://127.0.0.1:4567/unity";
    private const string ip = "http://47.102.133.53:4567/unity/";
    public static IEnumerator IPost<T>(string api, WWWForm form, Action<float> uploadProgress = null, Action<float> downloadProgress = null, Action<T> success = null, Action<string> failure = null) where T : class
    {
        Debug.Log("post:" + ip + api);
        UnityWebRequest webRequest = UnityWebRequest.Post(ip + api, form);
        // webRequest.timeout = TIMEOUT;
        webRequest.SendWebRequest();
        while (!webRequest.isDone)
        {
            if (uploadProgress != null)
            {
                uploadProgress(webRequest.uploadProgress);
            }
            if (downloadProgress != null)
            {
                downloadProgress(webRequest.downloadProgress);
            }
            yield return 1;
        }
        if (webRequest.isHttpError || webRequest.isNetworkError)
        {
            Debug.Log(webRequest.error);
            if (failure != null)
            {
                failure(webRequest.error);
            }
        }
        else
        {
            ResultData<object> rsData = Json.Parse<ResultData<object>>(webRequest.downloadHandler.text);
            Debug.Log(webRequest.downloadHandler.text);
            if (!rsData.Success)
            {
                failure(rsData.Obj.ToString());
            }
            else if (success != null)
            {
                success(typeof(T) == typeof(string) ? rsData.Obj.ToString() as T : Json.Parse<T>(rsData.Obj.ToString()));
            }
        }
    }
    public static IEnumerator IGet<T>(string api, Action<float> uploadProgress = null, Action<float> downloadProgress = null, Action<T> success = null, Action<string> failure = null) where T : class
    {
        Debug.Log("get:" + ip + api);
        UnityWebRequest webRequest = UnityWebRequest.Get(ip + api);
        // webRequest.timeout = TIMEOUT;
        webRequest.SendWebRequest();
        while (!webRequest.isDone)
        {
            if (uploadProgress != null)
            {
                uploadProgress(webRequest.uploadProgress);
            }
            if (downloadProgress != null)
            {
                downloadProgress(webRequest.downloadProgress);
            }
            yield return 1;
        }
        if (webRequest.isHttpError || webRequest.isNetworkError)
        {
            Debug.Log(webRequest.error);
            if (failure != null)
            {
                failure(webRequest.error);
            }
        }
        else
        {
            ResultData<object> rsData = Json.Parse<ResultData<object>>(webRequest.downloadHandler.text);
            Debug.Log(webRequest.downloadHandler.text);
            if (!rsData.Success)
            {
                failure(rsData.Obj.ToString());
            }
            else if (success != null)
            {
                Debug.Log("success:" + typeof(T));
                success(typeof(T) == typeof(string) ? rsData.Obj.ToString() as T : Json.Parse<T>(rsData.Obj.ToString()));
            }
        }
    }
}