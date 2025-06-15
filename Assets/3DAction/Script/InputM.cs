using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;

public class InputM : MonoBehaviour
{
    //�� ��ǲ�� �ִ°����� ��ǲ���������� Ȯ���ϱ� �����
    //wasd�� ���������� ���ϵȴ�. ���� ����� �� �ֵ��� ����
    //������ �Է��� ������Ʈ���� �ٸ��Ŵ�. ���ǰ�� f = ������, �����̽� = �극��ũ
    //��� f�� ��ȣ�ۿ�, �����̽� = ����
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
