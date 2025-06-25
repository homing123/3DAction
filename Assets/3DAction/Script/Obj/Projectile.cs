using UnityEngine;
using System;
using NUnit.Framework.Internal;

//옵션
//발사체 오브젝트 : ResM에서 참조
//이동방식 : 포물선, 베이직, 유도, 회전락 4가지가 있음 (중복 불가능)
//제거 옵션 : 시간, 충돌 3가지가 있음 (중복 가능)
//회전방식 : 현재 날아가는 방향을 forward로 유지되도록, 날아가는 방향으로 데굴데굴(xz기준)


public struct ProjectileCreateOption
{
    public Projectile projectile;
    public Vector3 createPos;
    public float rotY;
    public bool autoStart;
    public ProjectileCreateOption(Projectile proj, Vector3 createpos, float roty, bool autostart = true)
    {
        projectile = proj;
        createPos = createpos;
        rotY = roty;
        autoStart = autostart;
    }
}


public struct ProjectileMoveInfo
{
    public Projectile.MOVE_TYPE moveType;
    public Vector3 startSpeed;
    public float acc;
    public Transform target;
    public float rotateLockValue;
    public static ProjectileMoveInfo Parabola(Vector3 startPos, Vector3 desti, float time)
    {
        ProjectileMoveInfo info = new ProjectileMoveInfo();
        info.moveType = Projectile.MOVE_TYPE.PARABOLA;
        info.startSpeed = Util.ParabolaStartSpeed(startPos, desti, time);
        return info;
    }
    public static ProjectileMoveInfo Bullet(Vector3 startSpeed, float acc)
    {
        ProjectileMoveInfo info = new ProjectileMoveInfo();
        info.moveType = Projectile.MOVE_TYPE.BULLET;
        info.startSpeed = startSpeed;
        info.acc = acc;
        return info;
    }
    public static ProjectileMoveInfo Auto(Vector3 startSpeed, float acc, Transform target)
    {
        ProjectileMoveInfo info = new ProjectileMoveInfo();
        info.moveType = Projectile.MOVE_TYPE.AUTO;
        info.startSpeed = startSpeed;
        info.acc = acc;
        info.target = target;
        return info;
    }
    public static ProjectileMoveInfo RotateLock(Vector3 startSpeed, float acc, Transform target, float rotateLockValue)
    {
        ProjectileMoveInfo info = new ProjectileMoveInfo();
        info.moveType = Projectile.MOVE_TYPE.ROTATE_LOCK;
        info.startSpeed = startSpeed;
        info.acc = acc;
        info.target = target;
        info.rotateLockValue = rotateLockValue;
        return info;
    }
}

public struct ProjectileDestroyInfo
{
    public Projectile.DESTROY_OPTION destroyOption;
    public float lifeTime;
    public int hitCount;

    public ProjectileDestroyInfo(Projectile.DESTROY_OPTION destroyoption, float lifetime = 0, int hitcount = 0)
    {
        destroyOption = destroyoption;
        lifeTime = lifetime;
        hitCount = hitcount;
    }
}
public class Projectile : MonoBehaviour
{
    //포물선 발사체 : y축을 이용해서 포물선으로 이동
    //원하는 도착 위치, 걸리는 시간 으로 세팅
    const float RotateSpeed = 360;
    public enum DESTROY_OPTION
    {
        TIME = 1<< 0,
        HIT = 1<< 1
    }
    public enum MOVE_TYPE
    {
        PARABOLA = 1, //포물선
        BULLET = 2, //속도만 있는 기본 발사체
        AUTO = 3, //유도
        ROTATE_LOCK = 4, //회전각도 제한이 있는 유도
        FUNC = 5, //함수에 정의됨
    }
    public enum ROTATE_TYPE
    {
        LOOK_DIR, //진행방향을 +Z로 하는 회전값세팅
        ROTATE_DIR, //진행 방향으로 회전 바퀴굴러가듯이
    }


    

    public static Projectile Create(in ProjectileCreateOption createOption, in ProjectileMoveInfo moveInfo, in ProjectileDestroyInfo destroyInfo, Action<Bio> ac_hit)
    {
        Projectile projectile = Instantiate(createOption.projectile.gameObject).GetComponent<Projectile>();
        projectile.Init(createOption, moveInfo, destroyInfo, ac_hit);
        return projectile;
    }


    bool m_isStart;

    Projectile.MOVE_TYPE m_MoveType;
    float m_CurSpeedXZ;
    float m_CurSpeedY;
    float m_Acc;
    Vector2 m_DirXZ;

    [SerializeField] Projectile.ROTATE_TYPE m_RotType;

    Transform m_Target;
    Vector3 m_LastTargetPos;
    float m_RotateLockValue;

    Projectile.DESTROY_OPTION m_DestroyOption;
    float m_LifeTime;
    int m_HitCount;
    Action<Bio> ac_Hit;

    protected virtual void Init(in ProjectileCreateOption createOption, in ProjectileMoveInfo moveInfo, in ProjectileDestroyInfo destroyInfo, Action<Bio> ac_hit)
    {
        transform.position = createOption.createPos;
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, createOption.rotY + transform.eulerAngles.y, transform.eulerAngles.z);
        m_isStart = createOption.autoStart;


        m_MoveType = moveInfo.moveType;
        Vector2 startSpeedXZ = moveInfo.startSpeed.VT2XZ();
        float startSpeedY = moveInfo.startSpeed.y;
        m_CurSpeedXZ = startSpeedXZ.magnitude;
        m_CurSpeedY = startSpeedY;
        m_Acc = moveInfo.acc;
        m_DirXZ = startSpeedXZ.normalized;
        m_Target = moveInfo.target;
        m_LastTargetPos = m_Target != null ? m_Target.position : Vector3.zero;
        m_RotateLockValue = moveInfo.rotateLockValue;

        m_DestroyOption = destroyInfo.destroyOption;
        m_LifeTime = destroyInfo.lifeTime;
        m_HitCount = destroyInfo.hitCount;

        ac_Hit = ac_hit;
    }

    
    public void Fire()
    {
        m_isStart = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Bio.CheckisBio(other, out Bio bio))
        {
            ac_Hit?.Invoke(bio);
        }
    }
    private void Update()
    {
        if(m_isStart == false)
        {
            return;
        }
        switch (m_MoveType)
        {
            case MOVE_TYPE.PARABOLA:
                {
                    Vector2 moveXZ = m_DirXZ * m_CurSpeedXZ;
                    transform.position += new Vector3(moveXZ.x, m_CurSpeedY, moveXZ.y) * Time.deltaTime;
                    m_CurSpeedY -= Util.Gravity * Time.deltaTime;
                }
                break;
            case MOVE_TYPE.BULLET:
                {
                    Vector2 moveXZ = m_DirXZ * m_CurSpeedXZ;
                    transform.position += new Vector3(moveXZ.x, 0, moveXZ.y) * Time.deltaTime;
                    m_CurSpeedXZ += m_Acc * Time.deltaTime;
                }
                break;
            case MOVE_TYPE.AUTO:
                {
                    m_LastTargetPos = m_Target != null ? m_Target.position : m_LastTargetPos;
                    Vector3 movePos = Util.MoveDesti(transform.position, m_LastTargetPos, m_CurSpeedXZ, out bool isArrived);
                    transform.position = movePos;
                    m_CurSpeedXZ += m_Acc * Time.deltaTime;
                }
                break;
            case MOVE_TYPE.ROTATE_LOCK:
                break;
            case MOVE_TYPE.FUNC:
                break;
        }

        switch(m_RotType)
        {
            case ROTATE_TYPE.LOOK_DIR:
                transform.LookAt(m_DirXZ);
                break;
            case ROTATE_TYPE.ROTATE_DIR:
                //dir = +Z
                //rot axis = +x
                Vector3 up = Vector3.up;
                Vector3 front = m_DirXZ;
                Vector3 right = Vector3.Cross(up, front);

                transform.rotation *= Quaternion.AngleAxis(RotateSpeed * Time.deltaTime, right);
                break;
        }


        switch(m_DestroyOption)
        {
            case DESTROY_OPTION.TIME:
                m_LifeTime -= Time.deltaTime;
                if(m_LifeTime <= 0 )
                {
                    Destroy(this.gameObject);
                }
                break;
            case DESTROY_OPTION.HIT:
                m_HitCount--;
                if(m_HitCount <= 0)
                {
                    Destroy(this.gameObject);
                }
                break;
        }
    }
    
}
