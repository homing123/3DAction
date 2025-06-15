using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;

public class InputM : MonoBehaviour
{
    //각 인풋이 있는곳마다 인풋가능조건을 확인하기 힘드니
    //wasd는 움직임으로 통일된다. 쉽게 사용할 수 있도록 하자
    //나머지 입력은 오브젝트마다 다를거다. 차의경우 f = 내리기, 스페이스 = 브레이크
    //사람 f는 상호작용, 스페이스 = 점프
    public static InputM Ins;
    public static Vector2 InputDir { get; private set; }
    private void Awake()
    {
        if (Ins != null && Ins != this)
        {
            Destroy(Ins.gameObject);
        }
        Ins = this;
    }
    private void Update()
    {
        Vector2 inputDir = Vector2.zero;
        if (Input.GetKey(KeyCode.W))
        {
            inputDir.y = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            inputDir.x = -1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            inputDir.x = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            inputDir.y = -1;
        }
        if(inputDir!=Vector2.zero)
        {
            inputDir.Normalize();
        }
        InputDir = inputDir;
    }
   

  
 

}
