using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
public class FreezeObject : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    // 현재 Grab 중인 Interactor 리스트
    private HashSet<IXRSelectInteractor> activeInteractors = new HashSet<IXRSelectInteractor>();

    // 기본 Freeze 설정 (잡고 있지 않을 때 적용)
    private readonly RigidbodyConstraints frozenConstraints =
        RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;

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
        rb.constraints = RigidbodyConstraints.None;  // 제약 해제
    }

    private void OnGrabEnd(SelectExitEventArgs args)
    {
        activeInteractors.Remove(args.interactorObject);

        if (activeInteractors.Count == 0)
        {
            rb.constraints = frozenConstraints;  // 아무도 안 잡고 있으면 다시 Freeze
        }
    }
}