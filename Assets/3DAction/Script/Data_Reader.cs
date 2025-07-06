using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using UnityEngine.Networking;
using System.Text;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using static Unity.VisualScripting.Icons;
using static UnityEngine.ParticleSystem;
using TreeEditor;
using UnityEditor.SceneManagement;

public class Data_Reader : MonoBehaviour
{
    static Data_Reader m_Instance;
    public static Data_Reader Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = FindObjectOfType<Data_Reader>();
            }
            return m_Instance;
        }
    }
    [SerializeField] bool onlyBinary;
    public static bool OnlyBinary; //true=?¡À?????????? ?????? ???
    private void Awake()
    {
        OnlyBinary = onlyBinary;
    }

    /// <summary>
    /// ??????? ?????????? ?????? ????? ???
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="filename"></param>
    void WriteBinary<T>(string filename)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(Application.streamingAssetsPath + "/Binary/" + filename + ".bin"));

        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }

        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream fileStream = File.Open(Application.streamingAssetsPath + "/Binary/" + filename + ".bin", FileMode.OpenOrCreate);
        binaryFormatter.Serialize(fileStream, ReadData<T>(filename));
        fileStream.Close();
    }

    /// <summary>
    /// ?????? ??????? ??? ??????????? ????????, ????????????? ??????????
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static T ReadData<T>(string filename, string filepath = null)
    {

#if UNITY_EDITOR
        if (OnlyBinary)
        {
            if (filepath == null)
            {
                filepath = Application.streamingAssetsPath + "/Binary/";
            }
            string path = filepath + filename + ".bin";
            MemoryStream ms = new MemoryStream(File.ReadAllBytes(path));
            BinaryFormatter bf = new BinaryFormatter();
            return (T)bf.Deserialize(ms);
        }
        else
        {
            if (filepath == null)
            {
                filepath = Application.streamingAssetsPath + "/Text/";
            }
            string path = filepath + filename + ".txt";

            return JsonUtility.FromJson<T>(File.ReadAllText(path));
        }
#elif UNITY_STANDALONE_WIN
        if (filepath == null)
        {
            filepath = Application.streamingAssetsPath +"/Binary/";
        }
        string path = filepath + filename + ".bin";
        WWW reader = new WWW(path);
        while (!reader.isDone) { }
        MemoryStream ms = new MemoryStream(reader.bytes);
        BinaryFormatter bf = new BinaryFormatter();
        return (T)bf.Deserialize(ms);
#elif UNITY_ANDROID
        if (filepath == null)
        {
            filepath = Application.streamingAssetsPath + "/Binary/";
        }
        string path = filepath + filename + ".bin";
        UnityWebRequest webrequest = UnityWebRequest.Get(path);
        webrequest.SendWebRequest();
        while (!webrequest.isDone)
        {

        }
        MemoryStream ms = new MemoryStream(webrequest.downloadHandler.data);
        BinaryFormatter bf = new BinaryFormatter();
        return (T)bf.Deserialize(ms);
#elif UNITY_IOS
        if (filepath == null)
        {
            filepath = Application.streamingAssetsPath + "/Binary/";
        }
        string path = filepath + filename + ".bin";
        MemoryStream ms = new MemoryStream(File.ReadAllBytes(path));
        BinaryFormatter bf = new BinaryFormatter();
        return (T)bf.Deserialize(ms);
#endif


    }
    public static void ReadFile(out string data_string, out byte[] data_byte, string filename, string filepath = null)
    {
#if UNITY_EDITOR
        if (OnlyBinary)
        {
            if (filepath == null)
            {
                filepath = Application.streamingAssetsPath + "/Binary/";
            }
            string path = filepath + filename + ".bin";
            data_byte = File.ReadAllBytes(path);
            data_string = null;
        }
        else
        {
            if (filepath == null)
            {
                filepath = Application.streamingAssetsPath + "/Text/";
            }
            string path = filepath + filename + ".txt";
            data_byte = null;
            data_string = File.ReadAllText(path);
        }
#elif UNITY_STANDALONE_WIN
        if (filepath == null)
        {
            filepath = Application.streamingAssetsPath + "/Binary/";
        }
        string path = filepath + filename + ".bin";
        WWW reader = new WWW(path);
        while (!reader.isDone) { }
        data_byte = reader.bytes;
        data_string = null;
#elif UNITY_ANDROID
        if (filepath == null)
        {
            filepath = Application.streamingAssetsPath + "/Binary/";
        }
        string path = filepath + filename + ".bin";
        UnityWebRequest webrequest = UnityWebRequest.Get(path);
        webrequest.SendWebRequest();
        while (!webrequest.isDone)
        {

        }
        data_byte = webrequest.downloadHandler.data;
        data_string = null;
#elif UNITY_IOS
       if (filepath == null)
        {
            filepath = Application.streamingAssetsPath + "/Binary/";
        }
        string path = filepath + filename + ".bin";
        data_byte = File.ReadAllBytes(path);
        data_string = null;
#endif

    }


    public static string Log;
    public static T[] Get_Type<T>(object _class, int count, string str, int idx)
    {
        if (idx == 0)
        {
            if (Log == null)
            {
                Log = "";
            }
            T Temp = default;
            string str_data = "";
            switch (Temp)
            {
                case float:
                    for (int i = 0; i < count; i++)
                    {
                        if (i == count - 1)
                        {
                            str_data += str + i + "[i]";
                        }
                        else
                        {
                            str_data += str + i + "[i], ";
                        }
                    }
                    Log += "\nnew float[" + count + "] {" + str_data + "},";
                    break;
                case bool:
                    for (int i = 0; i < count; i++)
                    {
                        if (i == count - 1)
                        {
                            str_data += str + i + "[i]";
                        }
                        else
                        {
                            str_data += str + i + "[i], ";
                        }
                    }
                    Log += "\nnew bool[" + count + "] {" + str_data + "},";
                    break;
                case int:
                    for (int i = 0; i < count; i++)
                    {
                        if (i == count - 1)
                        {
                            str_data += str + i + "[i]";
                        }
                        else
                        {
                            str_data += str + i + "[i], ";
                        }
                    }
                    Log += "\nnew int[" + count + "] {" + str_data + "},";
                    break;
                case string:
                    for (int i = 0; i < count; i++)
                    {
                        if (i == count - 1)
                        {
                            str_data += str + i + "[i]";
                        }
                        else
                        {
                            str_data += str + i + "[i], ";
                        }
                    }
                    Log += "\nnew string[" + count + "] {" + str_data + "},";
                    break;
            }
        }
        //????????? ???
        T[] data = new T[count];
        for (int i = 0; i < count; i++)
        {
            data[i] = ((T[])_class.GetType().GetField(str + i).GetValue(_class))[idx];
        }
        return data;

        //??? ????? ???????? ??????????? ???
        //T[] data = new T[count];
        //for (int i = 0; i < count; i++)
        //{
        //    try
        //    {
        //        data[i] = ((T[])_class.GetType().GetField(str + i).GetValue(_class))[idx];

        //    }
        //    catch
        //    {
        //        Debug.Log(str + " " + count + " " + idx);
        //    }
        //}
        //return data;
    }

}