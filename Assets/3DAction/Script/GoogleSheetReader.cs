using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using static UnityEngine.Rendering.DebugUI;

public class GoogleSheetReader : MonoBehaviour
{
    //특정 sheet만 불러와서 binary화 저장
    //모든 sheet를 불러와서 binary화 저장
    //명시된 순서에 맞게 binary읽어서 로드

    //inspector에 입력데이터는 url, 열범위(?~?), 행범위(1~?)

    //const string TestURL = "https://docs.google.com/spreadsheets/d/16g04AMh2YydjKwtZsKuhJb0_EqDqz1efA8vg0DIYr4g/edit?gid=492334559#gid=492334559"; //origin
    //const string TestURL = "https://docs.google.com/spreadsheets/d/16g04AMh2YydjKwtZsKuhJb0_EqDqz1efA8vg0DIYr4g/export?format=tsv&range=A1:E&gid=492334559"; //edit

    [System.Serializable]
    public class GoogleSheetInfo
    {
        public string URL;
        public string RowMin;
        public string RowMax;
        public int ColumnMin;
        public int ColumnMax;
    }
    const string URLFormat = "export?format=tsv&range=";
    const string LineChangeSymbol = "[br]";

    public GoogleSheetInfo m_TextSheetInfo;
    public GoogleSheetInfo m_SkillSheetInfo;

    public static async Task<T[]> LoadGoogleSheet2Instances<T>(GoogleSheetInfo info) where T : new()
    {
        string url = EditURL(info);
        string data = await GetGoogleSheetData(url);
        if(data == null)
        {
            Debug.LogError($"LoadGoogleSheet Fail");
            return null;
        }
        TSV2InstanceArray(data, out T[] arr_Instance);
        return arr_Instance;
    }

    /// <summary>
    /// URL에 불러올 타입과 범위가 들어가야함 URL변경 함수
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    static string EditURL(GoogleSheetInfo info)
    {
        string[] splitSlash = info.URL.Split("/");
        string[] splitfragment = splitSlash[splitSlash.Length - 1].Split("#");
        string gid = splitfragment[splitfragment.Length - 1];
        string rowMin = string.IsNullOrEmpty(info.RowMin) ? "A" : info.RowMin;
        string rowMax = string.IsNullOrEmpty(info.RowMax) ? "A" : info.RowMax;
        string columnMin = info.ColumnMin <= 0 ? "" : info.ColumnMin.ToString();
        string columnMax = info.ColumnMax <= 0 ? "" : info.ColumnMax.ToString();

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < splitSlash.Length - 1; i++)
        {
            builder.Append(splitSlash[i]);
            builder.Append("/");
        }
        builder.Append(URLFormat);
        builder.Append(rowMin);
        builder.Append(columnMin);
        builder.Append(":");
        builder.Append(rowMax);
        builder.Append(columnMax);
        builder.Append("&");
        builder.Append(gid);
        return builder.ToString();
    }

    /// <summary>
    /// URL을 이용해서 데이터 받아오는 함수
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    static async Task<string> GetGoogleSheetData(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            await request.SendWebRequest();
            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    return request.downloadHandler.text;
                default:
                    Debug.Log($"URL : {url}\n 실패 : {request.result}");
                    break;

            }
        }
        return null;
    }

    static void TSV2InstanceArray<T>(string data, out T[] array) where T : new()
    {
        string[] line = data.Split('\n');
        int dataCount = line.Length - 1;
        string[] fieldNames = line[0].Split('\t');

        PropertyInfo[] fieldInfose = new PropertyInfo[fieldNames.Length];
        Type type = typeof(T);
        for(int i=0;i< fieldNames.Length;i++)
        {
            fieldInfose[i] = type.GetProperty(fieldNames[i].Trim(), BindingFlags.Public | BindingFlags.Instance);
        }
        T[] instances = new T[dataCount];

        for (int i = 0; i < line.Length - 1; i++)
        {
            string[] datas = line[i + 1].Split('\t');
            instances[i] = new T();
            for (int j = 0; j < fieldInfose.Length; j++)
            {
                object convertedValue = null;
                if (fieldInfose[j].PropertyType.IsEnum)
                {
                    if (Enum.TryParse(fieldInfose[j].PropertyType, datas[j], false, out convertedValue) == false)
                    {
                        convertedValue = 0;
                    }
                }
                //데이터가 비어있을 경우
                else if ((string.IsNullOrEmpty(datas[j]) || string.IsNullOrWhiteSpace(datas[j])))
                {
                    if (fieldInfose[j].PropertyType == typeof(float) || fieldInfose[j].PropertyType == typeof(int))
                    {
                        convertedValue = 0;
                    }
                    else if(fieldInfose[j].PropertyType == typeof(bool))
                    {
                        convertedValue = false;
                    }
                    else
                    {
                        convertedValue = Convert.ChangeType(datas[j], fieldInfose[j].PropertyType);
                    }
                }
                else
                {
                    convertedValue = Convert.ChangeType(datas[j], fieldInfose[j].PropertyType);
                }

                if (fieldInfose[j].PropertyType == typeof(string))
                {
                    string str = (string)convertedValue;
                    str = str.Replace(LineChangeSymbol, "\n");     
                    fieldInfose[j].SetValue(instances[i], str);
                }  
                else
                {
                    fieldInfose[j].SetValue(instances[i], convertedValue);
                }
            }
        }
        array = instances;
    }
    public static void SaveBinary<T>(T data, string fileName)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(Application.streamingAssetsPath);
        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }
        BinaryFormatter binary = new BinaryFormatter();
        string path = Application.streamingAssetsPath + $"/{fileName}.bin";
        using (FileStream stream = File.Open(path, FileMode.OpenOrCreate))
        {
            binary.Serialize(stream, data);
            stream.Flush();
        } // 자동으로 Close() 호출됨
    }
    public static void ReadBinary<T>(out T data, string fileName)
    {
        string korPath = Application.streamingAssetsPath + $"/{fileName}.bin";
        MemoryStream me = new MemoryStream(File.ReadAllBytes(korPath));
        BinaryFormatter bf = new BinaryFormatter();
        data = (T)bf.Deserialize(me);
    }



    //    public static T ReadData<T>(string filename, string filepath = null)
    //    {

    //#if UNITY_EDITOR
    //        if (filepath == null)
    //        {
    //            filepath = Application.streamingAssetsPath + "/Binary/";
    //        }
    //        string path = filepath + filename + ".bin";
    //        MemoryStream ms = new MemoryStream(File.ReadAllBytes(path));
    //        BinaryFormatter bf = new BinaryFormatter();
    //        return (T)bf.Deserialize(ms);


    //#elif UNITY_STANDALONE_WIN
    //        if (filepath == null)
    //        {
    //            filepath = Application.streamingAssetsPath +"/Binary/";
    //        }
    //        string path = filepath + filename + ".bin";
    //        WWW reader = new WWW(path);
    //        while (!reader.isDone) { }
    //        MemoryStream ms = new MemoryStream(reader.bytes);
    //        BinaryFormatter bf = new BinaryFormatter();
    //        return (T)bf.Deserialize(ms);
    //#elif UNITY_ANDROID
    //        if (filepath == null)
    //        {
    //            filepath = Application.streamingAssetsPath + "/Binary/";
    //        }
    //        string path = filepath + filename + ".bin";
    //        UnityWebRequest webrequest = UnityWebRequest.Get(path);
    //        webrequest.SendWebRequest();
    //        while (!webrequest.isDone)
    //        {

    //        }
    //        MemoryStream ms = new MemoryStream(webrequest.downloadHandler.data);
    //        BinaryFormatter bf = new BinaryFormatter();
    //        return (T)bf.Deserialize(ms);
    //#elif UNITY_IOS
    //        if (filepath == null)
    //        {
    //            filepath = Application.streamingAssetsPath + "/Binary/";
    //        }
    //        string path = filepath + filename + ".bin";
    //        MemoryStream ms = new MemoryStream(File.ReadAllBytes(path));
    //        BinaryFormatter bf = new BinaryFormatter();
    //        return (T)bf.Deserialize(ms);
    //#endif


    //    }
    //    public static void ReadFile(out string data_string, out byte[] data_byte, string filename, string filepath = null)
    //    {
    //#if UNITY_EDITOR    
    //        if (filepath == null)
    //        {
    //            filepath = Application.streamingAssetsPath + "/Binary/";
    //        }
    //        string path = filepath + filename + ".bin";
    //        data_byte = File.ReadAllBytes(path);
    //        data_string = null;
    //#elif UNITY_STANDALONE_WIN
    //        if (filepath == null)
    //        {
    //            filepath = Application.streamingAssetsPath + "/Binary/";
    //        }
    //        string path = filepath + filename + ".bin";
    //        WWW reader = new WWW(path);
    //        while (!reader.isDone) { }
    //        data_byte = reader.bytes;
    //        data_string = null;
    //#elif UNITY_ANDROID
    //        if (filepath == null)
    //        {
    //            filepath = Application.streamingAssetsPath + "/Binary/";
    //        }
    //        string path = filepath + filename + ".bin";
    //        UnityWebRequest webrequest = UnityWebRequest.Get(path);
    //        webrequest.SendWebRequest();
    //        while (!webrequest.isDone)
    //        {

    //        }
    //        data_byte = webrequest.downloadHandler.data;
    //        data_string = null;
    //#elif UNITY_IOS
    //       if (filepath == null)
    //        {
    //            filepath = Application.streamingAssetsPath + "/Binary/";
    //        }
    //        string path = filepath + filename + ".bin";
    //        data_byte = File.ReadAllBytes(path);
    //        data_string = null;
    //#endif

    //    }
}
