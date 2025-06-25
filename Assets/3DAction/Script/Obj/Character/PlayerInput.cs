using UnityEngine;
using System;

//move to groundpos
//move to objectTarget
//stop
//hold
//attackdesti to groundpos
//chaseTarget to monster
//chaseTarget to character (팀인지 적인지 캐릭터에서 판단 후 처리)
//useSkill at keycode
//view info
public enum InputCommandType
{
    Move2GroundPos = 0,
    Move2ObjectTarget = 1,
    Stop = 2,
    Hold = 3,
    AttackDesti = 4,
    ChaseTarget2Monster = 5,
    ChaseTarget2Character = 6,
    UseSkill = 7,
    ViewInfo = 8
}
public struct InputInfo
{
    public InputCommandType command;
    public Vector3 groundPos;
    public Character characterTarget;
    public Monster monsterTarget;
    public Transform objectTarget;
    public KeyCode skillKey;
}

public class PlayerInput : MonoBehaviour
{
    
    public enum MouseTargetType
    {
        Character,
        Monster,
        Object,
        None
    }
   
    public static PlayerInput Ins;

    Vector3 m_MouseGroundPos;
    Character m_CharacterTarget;
    Monster m_MonsterTarget;
    Transform m_ObjectTarget;
    int m_RayLayerMask;
    bool m_isAttack;
    MouseTargetType m_MouseTargetType;
    public event Action<InputInfo> OnInput;
    private void Awake()
    {
        if (Ins != null && Ins != this)
        {
            Destroy(Ins.gameObject);
        }
        Ins = this;
    }
    private void Start()
    {
        m_RayLayerMask = Define.D_LayerMask[Layer.Character] | Define.D_LayerMask[Layer.Monster] | Define.D_LayerMask[Layer.Object] | Define.D_LayerMask[Layer.Ground];
    }
    private void Update()
    {
        GetMouseRayCast();
        bool isInput = false;
        InputInfo inputinfo = default;

        //ui버튼 입력의 경우 처리해야함

        
        if (Input.GetKeyDown(KeyCode.A))
        {
            m_isAttack = true;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            inputinfo = default;
            isInput = true;
            inputinfo.command = InputCommandType.Stop;
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            isInput = true;
            inputinfo = default;
            inputinfo.command = InputCommandType.Hold;
        }
        if (Input.GetMouseButtonDown(0))
        {
            inputinfo = default;
            if(m_isAttack)
            {
                isInput = true;
                switch (m_MouseTargetType)
                {
                    case MouseTargetType.None:
                        inputinfo.command = InputCommandType.AttackDesti;
                        inputinfo.groundPos = m_MouseGroundPos;
                        break;
                    case MouseTargetType.Character:
                        inputinfo.command = InputCommandType.ChaseTarget2Character;
                        inputinfo.characterTarget = m_CharacterTarget;
                        break;
                    case MouseTargetType.Monster:
                        inputinfo.command = InputCommandType.ChaseTarget2Monster;
                        inputinfo.monsterTarget = m_MonsterTarget;
                        break;
                }
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            isInput = true;
            inputinfo = default;
           
            switch (m_MouseTargetType)
            {
                case MouseTargetType.None:
                    inputinfo.command = InputCommandType.Move2GroundPos;
                    inputinfo.groundPos = m_MouseGroundPos;
                    break;
                case MouseTargetType.Object:
                    inputinfo.command = InputCommandType.Move2ObjectTarget;
                    inputinfo.objectTarget = m_ObjectTarget;
                    break;
                case MouseTargetType.Character:
                    inputinfo.command = InputCommandType.ChaseTarget2Character;
                    inputinfo.characterTarget = m_CharacterTarget;
                    break;
                case MouseTargetType.Monster:
                    inputinfo.command = InputCommandType.ChaseTarget2Monster;
                    inputinfo.monsterTarget = m_MonsterTarget;
                    break;
            }
            
        }
      
      
      
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isInput = true;
            inputinfo = default;
            inputinfo.command = InputCommandType.UseSkill;
            inputinfo.skillKey = KeyCode.Q;
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            isInput = true;
            inputinfo = default;
            inputinfo.command = InputCommandType.UseSkill;
            inputinfo.skillKey = KeyCode.W;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            isInput = true;
            inputinfo = default;
            inputinfo.command = InputCommandType.UseSkill;
            inputinfo.skillKey = KeyCode.E;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            isInput = true;
            inputinfo = default;
            inputinfo.command = InputCommandType.UseSkill;
            inputinfo.skillKey = KeyCode.R;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            isInput = true;
            inputinfo = default;
            inputinfo.command = InputCommandType.UseSkill;
            inputinfo.skillKey = KeyCode.D;
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            isInput = true;
            inputinfo = default;
            inputinfo.command = InputCommandType.UseSkill;
            inputinfo.skillKey = KeyCode.F;
        }

        if(isInput)
        {
            m_isAttack = false;
            OnInput?.Invoke(inputinfo);
        }
    }

   
    void GetMouseRayCast()
    {
        Vector3 camPos = CamM.Ins.transform.position;
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(mouseRay, out RaycastHit hit, 30.0f, m_RayLayerMask))
        {
            switch(hit.collider.gameObject.layer)
            {
                case (int)Layer.Character:
                    m_MouseTargetType = MouseTargetType.Character;
                    m_CharacterTarget = hit.transform.GetComponent<Character>();
                    break;
                case (int)Layer.Monster:
                    m_MouseTargetType = MouseTargetType.Monster;
                    m_MonsterTarget = hit.transform.GetComponent<Monster>();
                    break;
                case (int)Layer.Object:
                    m_MouseTargetType = MouseTargetType.Object;
                    m_ObjectTarget = hit.transform;
                    break;
                case (int)Layer.Ground:
                    m_MouseTargetType = MouseTargetType.None;
                    m_MouseGroundPos = hit.point;
                    break;
            }
        }
    }
    public Vector2 GetSkillDirVT2(Vector3 userPos)
    {
        Vector3 toMousePos = m_MouseGroundPos - userPos;
        return toMousePos.VT2XZ().normalized;
    }
}
