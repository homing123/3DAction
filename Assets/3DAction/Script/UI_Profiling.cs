using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class UI_Profiling : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI T_Frame;
    [SerializeField] TMPro.TextMeshProUGUI T_Memory;

    float m_UpdateDelay;
    Recorder m_Recorder;
    private void Start()
    {
        Update();
    }

    private void Update()
    {
        m_UpdateDelay -= Time.deltaTime;
        if(m_UpdateDelay < 0)
        {
            m_UpdateDelay = 1;
            SetText();
        }
    }
    void SetText()
    {
        float symbolSize = 1024 * 1024;
        float graphicsMem = Profiler.GetAllocatedMemoryForGraphicsDriver() / symbolSize;
        float totalReservedMem = Profiler.GetTotalReservedMemoryLong() / symbolSize;
        float totalAllocatedMem = Profiler.GetTotalAllocatedMemoryLong() / symbolSize;
        T_Frame.text = $"FPS : {(1 / Time.deltaTime)}\nDeltaTime : {(Time.deltaTime * 1000).ToString("F2")}";
        T_Memory.text = $"Total {totalReservedMem.ToString("F3")}MB\nAllocated {totalAllocatedMem.ToString("F3")}MB\nGraphicMem{graphicsMem.ToString("F3")}MB";
    }
}
