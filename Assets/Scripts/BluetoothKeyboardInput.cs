using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class BluetoothKeyboardInput : MonoBehaviour
{
    public TextMeshProUGUI textDisplay;
    private string typedText = "";

    void Update()
    {
        if (Keyboard.current == null) return;

        foreach (var key in Keyboard.current.allKeys)
        {
            if (key.wasPressedThisFrame)
            {
                string k = key.displayName;

                if (k == "Backspace" && typedText.Length > 0)
                    typedText = typedText.Substring(0, typedText.Length - 1);
                else if (k == "Enter")
                    typedText += "\n";
                else if (k == "Space")
                    typedText += " ";
                else if (k.Length == 1)
                    typedText += k;

                textDisplay.text = typedText;
            }
        }
    }
}
