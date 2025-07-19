using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;

public class Monster : Bio
{
    protected const string Anim_Skill = "Skill";

    protected Bio m_Target; // ÇÃ·¹ÀÌ¾î Å¸°Ù
    //[SerializeField] protected Skill m_SkillInfo;
    float m_SkillCurCoolTime;
    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
        // ÇÃ·¹ÀÌ¾î Ã£±â
        //m_Target = Player.Ins;

        // HP¹Ù »ý¼º
        //UI_HPBar.Create(m_Status);
    }

    protected override UniTaskVoid BaseAttack(CancellationTokenSource cts)
    {
        throw new NotImplementedException();
    }
    //protected void Update()
    //{
    //    //UpdateMonster();
    //    m_SkillCurCoolTime -= Time.deltaTime;
    //}

    //void UpdateMonster()
    //{
    //    if(m_Target==null)
    //    {
    //        return;
    //    }
    //    bool isMove = false;
    //    Vector3 cur2Target = m_Target.transform.position - transform.position;
    //    Vector3 cur2TargetFlat = cur2Target;
    //    cur2TargetFlat.y = 0;
    //    float dis2Player = Vector2.Distance(m_Target.transform.position.VT2XZ(), transform.position.VT2XZ());
    //    Vector3 dir = cur2TargetFlat.normalized;
    //    if(dir != Vector3.zero)
    //    {
    //        m_InputDir = cur2TargetFlat.normalized;
    //    }

    //    if (dis2Player <= m_SkillInfo.range)
    //    {
    //        TryAttack();
    //    }
    //    else
    //    {
    //        if (m_Status.Movable())
    //        {
    //            isMove = true;
    //            Move();
    //        }
    //    }
    //    SetAnim(isMove, Anim_Move);
    //}
    //void Move()
    //{
    //    Vector3 movePos = Util.MoveDir(transform.position, InputDir, m_Status.m_MoveSpeed * Time.deltaTime);
    //    transform.position = movePos;
    //}
    //void TryAttack()
    //{
    //    if (m_Status.Skillable() == false)
    //    {
    //        return;
    //    }

    //    if(m_SkillCurCoolTime > 0)
    //    {
    //        return;
    //    }

    //    if(m_SkillCoroutine!= null)
    //    {
    //        return;
    //    }
    //    m_Status.StartSkillCasting();
    //    m_SkillCurCoolTime = m_SkillInfo.coolTime;
    //    m_SkillCoroutine = StartCoroutine(C_Skill());
    //}

    //protected virtual IEnumerator C_Skill()
    //{
    //    yield return null;
    //}


}


//Ã¢ Âî¸£´Â ¸÷

//¹®¾î Ã³·³ »ý°Ü¼­ ÃÑ¾Ë¹ß»çÇÏ´Â¸÷

