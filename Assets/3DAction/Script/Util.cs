using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class Util
{
    public const float Gravity = 9.8f;
    public const int CanvasResolutionWidth = 1600;
    public const int CanvasResolutionHeight = 900;
    public static Vector3 MoveDesti(Vector3 curPos, Vector3 desti, float speed, out bool isArrived)
    {
        Vector3 dir = desti - curPos;
        float disSquare = Vector3.SqrMagnitude(dir);
        float curMoveDisSquare = speed * speed;
        if(disSquare <= curMoveDisSquare)
        {
            isArrived = true;
            return desti;
        }
        else
        {
            isArrived = false;
            return curPos + dir.normalized * speed;
        }
    }
    public static Vector3 MoveDir(Vector3 curPos, Vector3 dir, float speed)
    {
        return curPos + dir.normalized * speed;
    }

    public static Vector2 Deg2Vt2(float degree)
    {
        return new Vector2(Mathf.Sin(degree * Mathf.Deg2Rad), Mathf.Cos(degree * Mathf.Deg2Rad));
    }
    public static float Vt2ToYRotDeg(Vector2 vt2)
    {
        return Mathf.Atan2(vt2.x, vt2.y) * Mathf.Rad2Deg;
    }
    public static Vector2 Vt2XZRotAxisY(Vector2 vt2xz, float degree)
    {
        float cos = Mathf.Cos(-degree * Mathf.Deg2Rad);
        float sin = Mathf.Sin(-degree * Mathf.Deg2Rad);

        return new Vector2(cos * vt2xz.x - sin * vt2xz.y, sin * vt2xz.x + cos * vt2xz.y);
    }
    public static float AccMoveStartSpeed(Vector3 startPos, Vector3 desti, float acc, float time)
    {
        float dis = Vector3.Distance(desti, startPos);
        return AccMoveStartSpeed(dis, acc, time);
    }
    public static float AccMoveStartSpeed(float dis, float acc, float time)
    {
        return dis / time - 0.5f * acc * time;
    }
    public static Vector3 ParabolaStartSpeed(Vector3 startPos, Vector3 desti, float time)
    {
        Vector3 start2Desti = desti - startPos;
        Vector2 s2Dxz = start2Desti.VT2XZ();
        Vector2 xzSpeed = s2Dxz / time;
        float s2Dy = start2Desti.y;

        //d = v0 * t + 0.5 * a * t * t
        //v0 * t = d - 0.5 * a * t * t
        //v0 = d / t - 0.5 * a * t

        float ySpeed = s2Dy / time - 0.5f * (-Gravity) * time;
        Vector3 startSpeed = new Vector3(xzSpeed.x, ySpeed, xzSpeed.y);
        return startSpeed;
    }

    public static Vector2 Screen2Canvas(Vector2 screenVt2)
    {
        float u = screenVt2.x / Screen.width;
        float v = screenVt2.y / Screen.height;

        return new Vector2(u * Util.CanvasResolutionWidth, v * Util.CanvasResolutionHeight);
    }

    /// <summary>
    /// 박스 콜라이더의 보간되지않은 노말을 얻는 함수
    /// </summary>
    /// <param name="hitPos"></param>
    /// <param name="box"></param>
    /// <returns></returns>
    public static Vector3 NotInterpolationNormalBox(Vector3 hitPos, BoxCollider box)
    {
        Vector3 localHitPos = box.transform.InverseTransformPoint(hitPos);
        Vector3 halfSize = box.size * 0.5f;
        float disX = Mathf.Abs(Mathf.Abs(localHitPos.x) - halfSize.x);
        float disY = Mathf.Abs(Mathf.Abs(localHitPos.y) - halfSize.y);
        float disZ = Mathf.Abs(Mathf.Abs(localHitPos.z) - halfSize.z);

        Vector3 localNormal = Vector3.zero;
        if(disX < disY && disX < disZ)
        {
            localNormal = new Vector3(Mathf.Sign(localHitPos.x), 0, 0);
        }
        else if(disY < disZ)
        {
            localNormal = new Vector3(0, Mathf.Sign(localHitPos.y), 0);
        }
        else
        {
            localNormal = new Vector3(0, 0, Mathf.Sign(localHitPos.z));
        }

        Vector3 worldNormal = box.transform.TransformDirection(localNormal).normalized;
        return worldNormal;
    }

    public static async UniTask WaitDelay(float delay, CancellationTokenSource cts)
    {
        float curTime = 0;
        if (delay < 0)
        {
            return;
        }
        while (curTime < delay)
        {
            if (cts.IsCancellationRequested)
            {
                return;
            }
            await UniTask.Yield();
            curTime += Time.deltaTime;
        }
    }
}
public static class UtilEX
{


    public static Vector2 VT2XZ(this Vector3 vt3)
    {
        return new Vector2(vt3.x, vt3.z);
    }
    public static Vector3 VT2XZToVT3(this Vector2 vt2xz, float y = 0)
    {
        return new Vector3(vt2xz.x, y, vt2xz.y);
    }

    public static void SetScreenPosition(this RectTransform recttransform, Vector2 screenPos)
    {
        Vector2 canvasPos = Util.Screen2Canvas(screenPos);
        Vector2 size = recttransform.sizeDelta;
        Vector2 pivot = recttransform.pivot;

        //피봇이 0,0 이면 canvasPos에서 size * 0.5만큼 뺀 위치
        recttransform.anchoredPosition = canvasPos + (pivot - new Vector2(0.5f, 0.5f)) * size;
    }

}   

