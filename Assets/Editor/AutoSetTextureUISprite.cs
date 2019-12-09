/************************** * 文件名:AutoSetTextureUISprite.cs; * 文件描述:导入图片资源到Unity时，自动修改为UI 2D Sprite，自动设置打包tag 为文件夹名字; * 创建日期:2015/05/04; * Author:陈鹏; ***************************/using UnityEngine;
using System.Collections;
using UnityEditor;
public class AutoSetTextureUISprite : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        //自动设置类型;        
        TextureImporter textureImporter = (TextureImporter)assetImporter; textureImporter.textureType = TextureImporterType.Sprite;
        //自动设置打包tag;        
        // string dirName = System.IO.Path.GetDirectoryName(assetPath);
        // Debug.Log("Import ---  " + dirName);
        // string folderStr = System.IO.Path.GetFileName(dirName);
        // Debug.Log("Set Packing Tag ---  " + folderStr);
        // textureImporter.spritePackingTag = folderStr;
    }
}
// ————————————————
// 版权声明：本文为CSDN博主「这个有点吓人」的原创文章，遵循 CC 4.0 BY - SA 版权协议，转载请附上原文出处链接及本声明。
// 原文链接：https://blog.csdn.net/qq_43667944/article/details/84103936