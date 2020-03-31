using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System;
using System.IO;
using System.Text;

public class Json : MonoBehaviour
{
    /// <summary>
    /// 反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sb"></param>
    /// <returns></returns>
    public static T Parse<T>(String sb)
    {
        return JsonConvert.DeserializeObject<T>(sb, new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        });
    }
    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public static string Serialize(System.Object o)
    {
        return JsonConvert.SerializeObject(o,
            Formatting.None,
            new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            });

    }



}
