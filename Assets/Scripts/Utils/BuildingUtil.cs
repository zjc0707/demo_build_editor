using System.Collections.Generic;
using UnityEngine;
using System;
public class BuildingUtil
{
    public static BoxCollider AddBoxCollider(GameObject obj)
    {
        obj.transform.position = Vector3.zero;
        BoxCollider boxCollider = obj.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            CenterAndSize cs = Get(obj.transform);
            EnableAllCollider(obj.transform);
            boxCollider = obj.AddComponent<BoxCollider>();
            boxCollider.center = cs.Center;
            boxCollider.size = cs.Size;
            Debug.Log(obj.name + "-" + boxCollider.center + "-" + boxCollider.size);
        }
        //避免碰撞
        // boxCollider.isTrigger = true;
        return boxCollider;
    }
    public static void EnableAllCollider(Transform t)
    {
        foreach (Collider c in t.GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }
    }
    /// <summary>
    /// 根据子物体计算出物体的中心点和尺寸，数据为世界坐标系下
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static CenterAndSize Get(Transform t)
    {
        MeshRenderer[] meshRenderers = t.GetComponentsInChildren<MeshRenderer>();
        if (meshRenderers == null || meshRenderers.Length == 0)
        {
            return new CenterAndSize() { Center = Vector3.zero, Size = Vector3.one };
        }

        List<float> listx = new List<float>(),
                    listy = new List<float>(),
                    listz = new List<float>();
        foreach (var p in meshRenderers)
        {
            var center1 = p.bounds.center;
            var size = p.bounds.size / 2;
            listx.Add(center1.x + size.x);
            listx.Add(center1.x - size.x);
            listy.Add(center1.y + size.y);
            listy.Add(center1.y - size.y);
            listz.Add(center1.z + size.z);
            listz.Add(center1.z - size.z);
        }
        listx.Sort();
        listy.Sort();
        listz.Sort();

        float px = (listx[listx.Count - 1] + listx[0]) / 2,
              py = (listy[listy.Count - 1] + listy[0]) / 2,
              pz = (listz[listz.Count - 1] + listz[0]) / 2;
        float lx = listx[listx.Count - 1] - listx[0],
              ly = listy[listy.Count - 1] - listy[0],
              lz = listz[listz.Count - 1] - listz[0];

        return new CenterAndSize()
        {
            Center = new Vector3(px, py, pz),
            // Size = new Vector3(lx, ly, lz),
            //包围盒向上取整
            Size = new Vector3((float)Math.Ceiling(lx),
                                // (float)Math.Ceiling(ly),
                                ly,
                                (float)Math.Ceiling(lz)),
        };
    }
}