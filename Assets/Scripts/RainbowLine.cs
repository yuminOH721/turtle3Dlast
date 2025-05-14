using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RainbowLine : MonoBehaviour
{
    void Start()
    {
        LineRenderer line = GetComponent<LineRenderer>();

        // ✅ 재질 지정 (Sprites/Default가 가장 무난)
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.widthMultiplier = 0.5f;

        line.textureMode = LineTextureMode.Stretch;

        // ✅ 포지션 설정
        line.positionCount = 2;
        line.SetPosition(0, new Vector3(0, 0, 0));
        line.SetPosition(1, new Vector3(5, 0, 0)); // 길이가 중요

        // ✅ 무지개 그라디언트
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.red, 0.0f),
                new GradientColorKey(Color.yellow, 0.2f),
                new GradientColorKey(Color.green, 0.4f),
                new GradientColorKey(Color.cyan, 0.6f),
                new GradientColorKey(Color.blue, 0.8f),
                new GradientColorKey(Color.magenta, 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(1.0f, 1.0f)
            }
        );
        line.colorGradient = gradient;
    }
}
