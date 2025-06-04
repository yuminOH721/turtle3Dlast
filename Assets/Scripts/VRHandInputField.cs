using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(TMP_InputField))]
public class VRHandInputField : MonoBehaviour
{
    private TMP_InputField inputField;
    public UnityEvent onHandTouch;

    private void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            inputField.ActivateInputField();
            onHandTouch?.Invoke();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Hand") && !inputField.isFocused)
        {
            inputField.ActivateInputField();
        }
    }
}
