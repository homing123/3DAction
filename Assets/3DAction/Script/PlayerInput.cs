using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (GetMouseInputGroundPos(out Vector3 groundPos))
            {
                PlayerMove.Ins.Move(groundPos);
            }
        }
    }

    bool GetMouseInputGroundPos(out Vector3 groundPos)
    {
        Vector3 camPos = CamM.Ins.transform.position;
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(mouseRay, out RaycastHit hit, 30.0f, Define.D_LayerMask[Layer.Ground]))
        {
            groundPos = hit.point;
            return true;
        }
        else
        {
            groundPos = default;
            return false;
        }
    }
}
