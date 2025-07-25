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
    ChaseTarget2Sandbag = 7,
    UseSkill = 8,
    ViewInfo = 9
}
public struct InputInfo
{
    public InputCommandType command;
    public Vector3 groundPos;
    public Character characterTarget;
    public Monster monsterTarget;
    public Sandbag sandbagTarget;
    public Transform objectTarget;
    public SkillPos skillKey;
}

public class PlayerInput : MonoBehaviour
{
    
    public enum MouseTargetType
    {
        Character,
        Monster,
        Sandbag,
        Object,
        None
    }
   
    public static PlayerInput Ins;
    [SerializeField] float m_ScreenMoveWidth = 50;
    Vector3 m_MouseGroundPos;
    Character m_CharacterTarget;
    Monster m_MonsterTarget;
    Sandbag m_SandbagTarget;
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
        m_RayLayerMask = Define.D_LayerMask[Layer.Character] | Define.D_LayerMask[Layer.Monster]| Define.D_LayerMask[Layer.Sandbag] | Define.D_LayerMask[Layer.Object] | Define.D_LayerMask[Layer.Ground];
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
                    case MouseTargetType.Sandbag:
                        inputinfo.command = InputCommandType.ChaseTarget2Sandbag;
                        inputinfo.sandbagTarget = m_SandbagTarget;
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
                case MouseTargetType.Sandbag:
                    inputinfo.command = InputCommandType.ChaseTarget2Sandbag;
                    inputinfo.sandbagTarget = m_SandbagTarget;
                    break;
            }
            
        }
      
      
      
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isInput = true;
            inputinfo = default;
            inputinfo.command = InputCommandType.UseSkill;
            inputinfo.skillKey = SkillPos.Q;
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            isInput = true;
            inputinfo = default;
            inputinfo.command = InputCommandType.UseSkill;
            inputinfo.skillKey = SkillPos.W;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            isInput = true;
            inputinfo = default;
            inputinfo.command = InputCommandType.UseSkill;
            inputinfo.skillKey = SkillPos.E;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            isInput = true;
            inputinfo = default;
            inputinfo.command = InputCommandType.UseSkill;
            inputinfo.skillKey = SkillPos.R;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            isInput = true;
            inputinfo = default;
            inputinfo.command = InputCommandType.UseSkill;
            inputinfo.skillKey = SkillPos.Weapon;
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            isInput = true;
            inputinfo = default;
            inputinfo.command = InputCommandType.UseSkill;
            inputinfo.skillKey = SkillPos.Spell;
        }

        if(isInput)
        {
            m_isAttack = false;
            OnInput?.Invoke(inputinfo);
        }

        Vector2 mousePos = Input.mousePosition;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Vector2 camMoveDir = default;
        if (mousePos.x < m_ScreenMoveWidth)
        {
            camMoveDir.x = -1;
        }
        else if (mousePos.x > screenSize.x - m_ScreenMoveWidth)
        {
            camMoveDir.x = 1;
        }
        if (mousePos.y < m_ScreenMoveWidth)
        {
            camMoveDir.y = -1;
        }
        else if (mousePos.y > screenSize.y - m_ScreenMoveWidth)
        {
            camMoveDir.y = 1;
        }
        if(camMoveDir != Vector2.zero)
        {
            CamM.Ins.CamMove(camMoveDir);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            CamM.Ins.CamLookTarget(PlayerM.Ins.GetPlayerCharacter().transform);
        }
    }

    private void LateUpdate()
    {       

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
                case (int)Layer.Sandbag:
                    m_MouseTargetType = MouseTargetType.Sandbag;
                    m_SandbagTarget = hit.transform.GetComponent<Sandbag>();
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
    public Vector2 GetUser2MouseVT2(Vector3 userPos)
    {
        Vector3 toMousePos = m_MouseGroundPos - userPos;
        return toMousePos.VT2XZ();
    }

}
