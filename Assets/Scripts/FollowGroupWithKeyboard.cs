using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FollowGroupWithKeyboard : MonoBehaviour
{
    public Transform keyboardAsset;  // 기준이 되는 키보드 에셋
    public Transform gridCube;
    public Transform window1;
    public Transform window2;
    public Transform window3;

    private Vector3 offsetGridCube;
    private Vector3 offsetWindow1;
    private Vector3 offsetWindow2;
    private Vector3 offsetWindow3;

    private XRGrabInteractable grabInteractable;

    void Start()
    {
        // 초기 상대 위치 계산
        offsetGridCube = gridCube.position - keyboardAsset.position;
        offsetWindow1 = window1.position - keyboardAsset.position;
        offsetWindow2 = window2.position - keyboardAsset.position;
        offsetWindow3 = window3.position - keyboardAsset.position;

        // GrabInteractable 참조
        grabInteractable = keyboardAsset.GetComponent<XRGrabInteractable>();
    }

    void LateUpdate()
    {
        // 키보드를 누군가 잡고 있을 때만 따라감
        if (grabInteractable != null && grabInteractable.isSelected)
        {
            gridCube.position = keyboardAsset.position + offsetGridCube;
            window1.position = keyboardAsset.position + offsetWindow1;
            window2.position = keyboardAsset.position + offsetWindow2;
            window3.position = keyboardAsset.position + offsetWindow3;
        }
    }
}
