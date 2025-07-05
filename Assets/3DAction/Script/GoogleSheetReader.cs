using Cysharp.Threading.Tasks;
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

    public void Start()
    {
        for (int i = 0; i < m_SheetInfo.Length; i++)
        {
            string url = EditURL(m_SheetInfo[i]);
            ReadURL(url);
        }
    }
    string EditURL(GoogleSheetInfo info)
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
    public async void ReadURL(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            await request.SendWebRequest();
            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    Debug.Log($"성공 : {request.downloadHandler.text}");
                    break;
                default:
                    Debug.Log($"URL : {url}\n 실패 : {request.result}");
                    break;

            }
        }
    }
}
