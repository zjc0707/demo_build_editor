using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class CenterAndSize
{
    public Vector3 Center { get; set; }
    public Vector3 Size { get; set; }
    public CenterAndSize ChangeSizeXZ()
    {
        return new CenterAndSize()
        {
            Size = new Vector3(this.Size.z, this.Size.y, this.Size.x),
            Center = this.Center
        };
    }

    public override string ToString()
    {
        return "Center:" + Center + "_Size:" + Size;
    }
}
