using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleSheetReader : MonoBehaviour
{
    [System.Serializable]
    public class GoogleSheetInfo
    {
        public string URL;
        public string RowMin;
        public string RowMax;
        public int ColumnMax;
    }

    [SerializeField] GoogleSheetInfo[] m_SheetInfo;
    const string URLFormat = "export?format=tsv&range=";

    //inspector에 입력데이터는 url, 열범위(?~?), 행범위(1~?)
    const string TestURL = "https://docs.google.com/spreadsheets/d/16g04AMh2YydjKwtZsKuhJb0_EqDqz1efA8vg0DIYr4g/edit?gid=492334559#gid=492334559";
    const string TestURL2 = "https://docs.google.com/spreadsheets/d/16g04AMh2YydjKwtZsKuhJb0_EqDqz1efA8vg0DIYr4g/export?format=tsv&range=A1:E&gid=492334559";

    [ContextMenu("구글시트 불러오기")]
    public void LoadGoogleSheet()
    {
        for (int i = 0; i < m_SheetInfo.Length; i++)
        {
            string url = EditURL(m_SheetInfo[i]);
            ReadURL(url);
        }
    }
    [ContextMenu("데이터 읽기")]
    public void ReadTest()
    {
        ReadBinary();
    }
    static string EditURL(GoogleSheetInfo info)
    {
        string[] splitSlash = info.URL.Split("/");
        string[] splitfragment = splitSlash[splitSlash.Length - 1].Split("#");
        string gid = splitfragment[splitfragment.Length - 1];
        string rowMin = string.IsNullOrEmpty(info.RowMin) ? "A1" : info.RowMin+"1";
        string rowMax = string.IsNullOrEmpty(info.RowMax) ? ":A" : ":"+info.RowMax;
        string columnMax = info.ColumnMax <= 0 ? "&" : info.ColumnMax.ToString()+"&";

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < splitSlash.Length - 1; i++)
        {
            builder.Append(splitSlash[i]);
            builder.Append("/");
        }
        builder.Append(URLFormat);
        builder.Append(rowMin);
        builder.Append(rowMax);
        builder.Append(columnMax);
        builder.Append("&");
        builder.Append(gid);
        return builder.ToString();
    }
    public static async void ReadURL(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            await request.SendWebRequest();
            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    Debug.Log($"성공 : {request.downloadHandler.text}");
                    TextData[] instancese = TSV2InstanceArray<TextData>(request.downloadHandler.text);

                    for(int i=0;i< instancese.Length;i++)
                    {
                        Debug.Log($"{instancese[i].ID}, {instancese[i].KOR}, {instancese[i].ENG}");
                    }
                    break;
                default:
                    Debug.Log($"URL : {url}\n 실패 : {request.result}");
                    break;

            }
        }
    }

    public static T[] TSV2InstanceArray<T>(string data) where T : new()
    {
        string[] line = data.Split('\n');
        int dataCount = line.Length - 1;
        string[] headersName = line[0].Split('\t');

        FieldInfo[] fieldInfose = new FieldInfo[headersName.Length];
        Type type = typeof(T);
        for(int i=0;i<headersName.Length;i++)
        {
            fieldInfose[i] = type.GetField(headersName[i].Trim(), BindingFlags.Public | BindingFlags.Instance);
        }
        T[] textDatas = new T[dataCount];

        for (int i = 0; i < line.Length - 1; i++)
        {
            string[] datas = line[i + 1].Split('\t');
            textDatas[i] = new T();
            for (int j = 0; j < fieldInfose.Length; j++)
            {
                object convertedValue = Convert.ChangeType(datas[j], fieldInfose[j].FieldType);
                fieldInfose[j].SetValue(textDatas[i], convertedValue);
            }
        }
        return textDatas;
    }
    public void TSV2Binary(string data)
    {
        string[] line = data.Split('\n');
        int dataCount = line.Length - 1;
        Debug.Log($"dataCount {dataCount}");    
        Dictionary<int, string> d_Kor = new Dictionary<int, string>(dataCount);
        Dictionary<int, string> d_Eng = new Dictionary<int, string>(dataCount);
        for(int i=1;i< line.Length;i++)
        {
            if (string.IsNullOrEmpty(line[i])==false)
            {
                string[] row = line[i].Split('\t');
                int id = int.Parse(row[0]);
                string kor = row[1];
                string eng = row[2];

                d_Kor[id] = kor;
                d_Eng[id] = eng;
            }
        }
        DirectoryInfo directoryInfo = new DirectoryInfo(Application.streamingAssetsPath);

        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }
        BinaryFormatter binary = new BinaryFormatter();
        string korPath = Application.streamingAssetsPath + "/Kor.bin";

        using (FileStream stream = File.Open(korPath, FileMode.OpenOrCreate))
        {
            binary.Serialize(stream, d_Kor);
            stream.Flush();
        } // 자동으로 Close() 호출됨
        string engPath = Application.streamingAssetsPath + "/Eng.bin";
        using (FileStream stream = File.Open(engPath, FileMode.OpenOrCreate))
        {
            binary.Serialize(stream, d_Eng);
            stream.Flush();
        }
        Debug.Log("바이너리 비동기 완");
    }

    public void ReadBinary()
    {
        string korPath = Application.streamingAssetsPath + "/Kor.bin";
        MemoryStream me = new MemoryStream(File.ReadAllBytes(korPath));
        BinaryFormatter bf = new BinaryFormatter();
        Dictionary<int, string> d_Kor = new Dictionary<int, string>();
        d_Kor = (Dictionary<int, string>)bf.Deserialize(me);

        string engPath = Application.streamingAssetsPath + "/Eng.bin";
        me = new MemoryStream(File.ReadAllBytes(engPath));
        Dictionary<int, string> d_Eng = new Dictionary<int, string>();
        d_Eng = (Dictionary<int, string>)bf.Deserialize(me);

        foreach(int key in d_Kor.Keys)
        {
            Debug.Log($"{key}, {d_Kor[key]}, {d_Eng[key]}");
        }
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
