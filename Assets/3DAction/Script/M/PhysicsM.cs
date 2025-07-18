using UnityEngine;

public class PhysicsM : MonoBehaviour
{
    //후에 모든 유니티 내부의 3d충돌을 내가 직접 구현한 2d 충돌체크로 바꿀 것
    //이터널 리턴은 높이값 충돌체크가 필요없다.

    public static PhysicsM Ins;
    private void Awake()
    {
        Ins = this;
    }

    private void Start()
    {
        
    }


}
