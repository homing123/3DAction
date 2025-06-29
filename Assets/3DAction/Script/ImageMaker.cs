using System.Data.SqlTypes;
using System.IO;
using UnityEngine;

public class ImageMaker : MonoBehaviour
{
    [SerializeField] string m_Path;
    [SerializeField] string m_FileName;
    [SerializeField] Texture2D m_Tex;

    [ContextMenu("이미지 생성")]
    public void CreateImage()
    {
        int width = m_Tex.width;
        int height = m_Tex.height;
        Color[] color = m_Tex.GetPixels();
        for (int y=0;y<height;y++)
        {
            for(int x=0;x<width;x++)
            {
                Color curColor = color[y * width + x];
                if(curColor.a > 0)
                {
                    curColor.r = 1;
                    curColor.g = 1;
                    curColor.b = 1;
                }
                color[y * width + x] = curColor;
            }
        }
        Texture2D newTex = new Texture2D(width, height);
        newTex.SetPixels(color);
        newTex.Apply();

        byte[] pngData = newTex.EncodeToPNG();
        string path = m_Path + m_FileName + ".png";
        File.WriteAllBytes(path, pngData);
        Debug.Log(path + " 완");
    }

}
