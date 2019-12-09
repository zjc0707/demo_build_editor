using System.Collections.Generic;
using UnityEngine;
public class BuildingUtil
{
    public static BoxCollider AddBoxCollider(GameObject obj)
    {
        obj.transform.position = Vector3.zero;
        BoxCollider boxCollider = obj.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = obj.AddComponent<BoxCollider>();
            CenterAndSize cs = CenterAndSizeUtil.Get(obj.transform);
            boxCollider.center = cs.Center;
            boxCollider.size = cs.Size;
            Debug.Log(obj.name + "-" + boxCollider.center + "-" + boxCollider.size);
        }
        //避免碰撞
        // boxCollider.isTrigger = true;
        return boxCollider;
    }
}