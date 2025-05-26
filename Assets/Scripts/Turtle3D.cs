using System.Collections;
using UnityEngine;

//================================================================================
// Turtle3D: 이동/회전 로직
//================================================================================
public class Turtle3D : MonoBehaviour
{
    public Transform tr;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotateSpeed = 90f;
    [SerializeField] private float gridScale = 1f;
    public Transform gridParent;
    public string TurtleName { get; private set; }

    void Awake()
    {
        tr = transform;
        if (gridParent == null && tr.parent != null)
            gridParent = tr.parent;
    }

    public void Initialize(string name, Vector3 pos, Quaternion rot)
    {
        TurtleName = name;
        gameObject.name = name;
        if (gridParent != null)
        {
            tr.SetParent(gridParent, false);
            tr.localPosition = pos;
            tr.localRotation = rot;
        }
        else
        {
            tr.position = pos;
            tr.rotation = rot;
        }
    }

    public Vector3 Position => tr.localPosition;

    public IEnumerator Forward(float units)
    {
        float dist = units * TurtleManager.instance.CellSize;

        Vector3 start = tr.localPosition;
        Vector3 dir = tr.localRotation * Vector3.forward;
        Vector3 end = start + dir * dist;

        Debug.Log($"Forward({units}) → dist={dist:F3}, from {start} to {end}");

        float duration = dist / moveSpeed;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            tr.localPosition = Vector3.Lerp(start, end, Mathf.Clamp01(t / duration));
            yield return null;
        }
        tr.localPosition = end;
    }


    public IEnumerator Rotate(float x, float y, float z)
    {
        Quaternion start = tr.rotation;
        Quaternion delta = Quaternion.Euler(x, y, z);
        Quaternion end = start * delta;
        float angle = Quaternion.Angle(start, end);
        float duration = angle / rotateSpeed;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            tr.rotation = Quaternion.Slerp(start, end, Mathf.Clamp01(t / duration));
            yield return null;
        }
        tr.rotation = end;
    }
}