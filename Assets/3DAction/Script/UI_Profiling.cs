using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using NUnit.Framework;
using System.Collections.Generic;

public class UI_Profiling : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI T_Frame;
    [SerializeField] TMPro.TextMeshProUGUI T_UnityMem;
    [SerializeField] TMPro.TextMeshProUGUI T_GPUMem;
    [SerializeField] TMPro.TextMeshProUGUI T_Drawcalls;

    float m_UpdateDelay;
    //스크립트 타임
    //ProfilerRecorder m_PlayerMoveUpdate;

    //유니티 메모리
    ProfilerRecorder m_UnityMem;
    ProfilerRecorder m_GCMem;

    //gpu메모리
    ProfilerRecorder m_GPUMem;
    ProfilerRecorder m_TextureMem;
    ProfilerRecorder m_MeshMem;

    //드로우콜
    ProfilerRecorder m_DrawCall;
    ProfilerRecorder m_SetPass;
    ProfilerRecorder m_VerticesCount;

    private void Start()
    {
        Update();
        //List<ProfilerRecorderHandle> l_ProfilerRecorderHandle = new List<ProfilerRecorderHandle>();
        //ProfilerRecorderHandle.GetAvailable(l_ProfilerRecorderHandle);

        //for (int i = 0; i < l_ProfilerRecorderHandle.Count; i++)
        //{
        //    ProfilerRecorderDescription desc = ProfilerRecorderHandle.GetDescription(l_ProfilerRecorderHandle[i]);
        //    Debug.Log(desc.Category + " " + desc.Name + " " + desc.UnitType);
        //}
    }
    private void OnEnable()
    {
        //m_PlayerMoveUpdate = ProfilerRecorder.StartNew(ProfilerCategory.Scripts, "Assembly-CSharp.dll!::PlayerMove.Update() [Invoke]");
        m_UnityMem = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
        m_GCMem = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
        m_GPUMem = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Video Used Memory");
        m_TextureMem = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Texture Memory");
        m_MeshMem = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Mesh Memory");
        m_DrawCall = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
        m_SetPass = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
        m_VerticesCount = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");
    }

    private void OnDisable()
    {
        //m_PlayerMoveUpdate.Dispose();
        m_UnityMem.Dispose();
        m_GCMem.Dispose();
        m_GPUMem.Dispose();
        m_TextureMem.Dispose();
        m_MeshMem.Dispose();
        m_DrawCall.Dispose();
        m_SetPass.Dispose();
        m_VerticesCount.Dispose();
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

        //Debug.Log("update : " + m_PlayerMoveUpdate.LastValue);
        //Debug.Log("m_UnityMem : " + m_UnityMem.LastValue);
        //Debug.Log("m_GCMem : " + m_GCMem.LastValue);
        //Debug.Log("m_GPUMem : " + m_GPUMem.LastValue);
        //Debug.Log("m_TextureMem : " + m_TextureMem.LastValue);
        //Debug.Log("m_MeshMem : " + m_MeshMem.LastValue);
        //Debug.Log("m_DrawCall : " + m_DrawCall.LastValue);
        //Debug.Log("m_SetPass : " + m_SetPass.LastValue);
        //Debug.Log("m_VerticesCount : " + m_VerticesCount.LastValue);
        //Debug.Log("graphicsMem : " + Profiler.GetAllocatedMemoryForGraphicsDriver());
        //Debug.Log("totalReservedMem : " + Profiler.GetTotalReservedMemoryLong());
        //Debug.Log("totalAllocatedMem : " + Profiler.GetTotalAllocatedMemoryLong());
        //float graphicsMem = Profiler.GetAllocatedMemoryForGraphicsDriver() / symbolSize;
        //float totalReservedMem = Profiler.GetTotalReservedMemoryLong() / symbolSize;
        //float totalAllocatedMem = Profiler.GetTotalAllocatedMemoryLong() / symbolSize;
        T_Frame.text = $"FPS : {(1 / Time.deltaTime)} / DeltaTime : {(Time.deltaTime * 1000).ToString("F2")}";
        float total = m_UnityMem.LastValue / symbolSize;
        float gc = m_GCMem.LastValue / symbolSize;
        T_UnityMem.text = $"Total {total.ToString("F3")} / GC {gc.ToString("F3")}";
        float gpu = m_GPUMem.LastValue / symbolSize;
        float texture = m_TextureMem.LastValue / symbolSize;
        float mesh = m_MeshMem.LastValue / symbolSize;
        T_GPUMem.text = $"GPU {gpu.ToString("F3")} / Texture {texture.ToString("F3")} / Mesh {mesh.ToString("F3")}";
        T_Drawcalls.text = $"DrawCall {m_DrawCall.LastValue} / SetPass {m_SetPass.LastValue} / Vertices {m_VerticesCount.LastValue}";
        //T_Memory.text = $"Total {totalReservedMem.ToString("F3")}MB\nAllocated {totalAllocatedMem.ToString("F3")}MB\nGraphicMem{graphicsMem.ToString("F3")}MB";
    }
}
