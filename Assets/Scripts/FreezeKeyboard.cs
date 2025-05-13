using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

public class FreezeKeyboard : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    // 현재 Grab 중인 Interactor 리스트
    private HashSet<IXRSelectInteractor> activeInteractors = new HashSet<IXRSelectInteractor>();

    // 아무도 안 잡고 있을 때: 위치 & 회전 모두 고정
    private readonly RigidbodyConstraints frozenConstraints =
        RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;

    // 잡고 있을 때: 위치는 자유롭게, 회전은 고정
    private readonly RigidbodyConstraints moveOnlyConstraints =
        RigidbodyConstraints.FreezeRotation;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        grabInteractable.selectEntered.AddListener(OnGrabStart);
        grabInteractable.selectExited.AddListener(OnGrabEnd);

        // 시작 시 Freeze 상태로
        rb.constraints = frozenConstraints;
    }

    private void OnGrabStart(SelectEnterEventArgs args)
    {
        activeInteractors.Add(args.interactorObject);
        rb.constraints = moveOnlyConstraints;  // 이동만 허용, 회전은 고정
    }

    private void OnGrabEnd(SelectExitEventArgs args)
    {
        activeInteractors.Remove(args.interactorObject);

        if (activeInteractors.Count == 0)
        {
            rb.constraints = frozenConstraints;  // 아무도 안 잡고 있으면 전체 고정
        }
    }
}