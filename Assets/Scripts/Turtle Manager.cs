using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;
using System.Globalization;

public class TurtleManager : MonoBehaviour
{
    /*────────────────────────────────────────────────────
     * 1. 인스펙터 바인딩용 필드
     *───────────────────────────────────────────────────*/
    [Header("Grid Parent for Turtles")]
    [Tooltip("거북이 이동 제한용 부모 Transform (Grid)")]
    public Transform gridParent;

    [Header("Prefabs & UI")]
    [Tooltip("거북이 프리팹")]
    public GameObject turtlePrefab;
    [Tooltip("동일 씬 캔버스에 있는 TextMeshPro Text")]
    public TMP_Text commandInput;

    [Header("Spawn Settings")]
    [Tooltip("최대 스폰 가능한 거북이 수")]
    public int maxTurtles = 5;
    public static Vector3 spawnPosition = Vector3.zero;
    public static readonly Quaternion spawnRotation = Quaternion.identity;

    [Header("Turtle Appearance")]
    [Tooltip("거북이 크기 스케일")]
    public Vector3 turtleScale = Vector3.one;

    [Header("Movement Settings")]
    [Tooltip("forward(1) 당 실제 이동 거리")]
    public float movementScale = 1f;

    [Header("Timing")]
    [Tooltip("한 줄(command) 처리 후 멈춰 있을 시간(초)")]
    [SerializeField] private float stepDelay = 0.5f;

    /*────────────────────────────────────────────────────
     * 2. 내부 풀 및 전역 테이블
     *───────────────────────────────────────────────────*/
    private readonly List<GameObject> turtlePool = new();
    private readonly Dictionary<string, Turtle3D> namedTurtles = new();
    private readonly Dictionary<string, Vector3> variables = new();

    /*────────────────────────────────────────────────────
     * 3. 명령어 큐 및 처리 상태 관리
     *───────────────────────────────────────────────────*/
    private readonly Queue<string> commandQueue = new();
    private bool isProcessing = false;

    /*────────────────────────────────────────────────────
     * 4. 싱글턴 인스턴스 관리
     *───────────────────────────────────────────────────*/
    public static TurtleManager instance;

    /********************************************************
     * ① 초기 설정 : 싱글턴 인스턴스 생성 및 풀 초기화
     ********************************************************/
    void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);

        if (gridParent == null)
            Debug.LogError("[TurtleManager] gridParent 미할당! Inspector에서 Grid 오브젝트를 연결하세요.");

        CreateTurtlePool();

        if (commandInput == null)
            Debug.LogWarning("[TurtleManager] commandInput 필드가 할당되지 않았습니다.");
    }

    /********************************************************
     * ② 거북이 풀 초기 생성
     ********************************************************/
    void CreateTurtlePool()
    {
        for (int i = 0; i < maxTurtles; i++)
        {
            GameObject go = Instantiate(
                turtlePrefab,
                spawnPosition,
                spawnRotation,
                gridParent
            );
            go.transform.localScale = turtleScale;
            go.SetActive(false);
            turtlePool.Add(go);
        }
    }

    /********************************************************
     * ③ 명령어 큐 입력 (텍스트 읽어서 처리)
     ********************************************************/
    public void ExecuteCurrentCommand()
    {
        if (commandInput == null)
        {
            Debug.LogError("[TurtleManager] commandInput is null! 인스펙터에서 연결했는지 확인하세요.");
            return;
        }

        string raw = commandInput.text;
        string[] lines = raw.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            string compact = Regex.Replace(line, "\\s+", "");
            EnqueueCommand(compact);
        }
    }

    void EnqueueCommand(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return;
        commandQueue.Enqueue(raw);
    }

    /********************************************************
     * ④ 매 프레임 명령어 처리
     ********************************************************/
    void Update()
    {
        if (!isProcessing && commandQueue.Count > 0)
            StartCoroutine(ProcessCommand(commandQueue.Dequeue()));
    }

    /********************************************************
     * ⑤ 명령어 해석 및 실행
     ********************************************************/
    private IEnumerator ProcessCommand(string cmd)
    {
        isProcessing = true;

        if (cmd.EndsWith("Turtle()") && cmd.Contains('='))
        {
            var parts = cmd.Split('=');
            string name = parts[0];
            var go = GetTurtleFromPool();
            if (go != null)
            {
                go.SetActive(true);
                var turtle = go.GetComponent<Turtle3D>();
                turtle.Initialize(name, spawnPosition, spawnRotation);
                namedTurtles[name] = turtle;
            }
            else Debug.LogError("[TurtleManager] 풀에 남은 거북이가 없습니다.");
        }
        else if (cmd.EndsWith(".position()") && cmd.Contains('='))
        {
            var parts = cmd.Split('=');
            string varName = parts[0];
            string key = parts[1].Substring(0, parts[1].IndexOf('.'));
            if (namedTurtles.TryGetValue(key, out var turtle))
            {
                variables[varName] = turtle.Position;
                Debug.Log($"{varName}=({turtle.Position.x:F2},{turtle.Position.y:F2},{turtle.Position.z:F2})");
            }
            else Debug.LogError($"[TurtleManager] 존재하지 않는 거북이: {key}");
        }
        else if ((cmd.Contains(".forward(") || cmd.Contains(".fd(")) && cmd.EndsWith(")"))
        {
            var verbs = new[] { ".forward(", ".fd(" };
            foreach (var v in verbs)
            {
                if (!cmd.Contains(v)) continue;
                string name = cmd.Substring(0, cmd.IndexOf(v));
                string numStr = cmd.Substring(
                    cmd.IndexOf(v) + v.Length,
                    cmd.Length - (cmd.IndexOf(v) + v.Length + 1)
                );
                if (namedTurtles.TryGetValue(name, out var turtle))
                {
                    if (TryParseExpression(numStr, out float dist))
                    {
                        var drawer = turtle.GetComponentInChildren<TurtleDrawer>();
                        if (drawer != null) drawer.StartDrawing();

                        yield return StartCoroutine(turtle.Forward(dist));

                        if (drawer != null) drawer.StopDrawing();
                    }
                    else
                        Debug.LogError($"[TurtleManager] 거리 숫자 파싱 실패: '{numStr}'");
                }
                else Debug.LogError($"[TurtleManager] 존재하지 않는 거북이: {name}");
                break;
            }
        }
        else if (cmd.Contains(".rotate(") && cmd.EndsWith(")"))
        {
            string name = cmd.Substring(0, cmd.IndexOf(".rotate("));
            string args = cmd.Substring(
                cmd.IndexOf(".rotate(") + ".rotate(".Length,
                cmd.Length - (cmd.IndexOf(".rotate(") + ".rotate(".Length) - 1
            );
            var parts = args.Split(',');
            if (parts.Length == 3 &&
                TryParseExpression(parts[0], out float x) &&
                TryParseExpression(parts[1], out float y) &&
                TryParseExpression(parts[2], out float z) &&
                namedTurtles.TryGetValue(name, out var turtle)
            )
            {
                var drawer = turtle.GetComponentInChildren<TurtleDrawer>();
                if (drawer != null) drawer.StartDrawing();

                yield return StartCoroutine(turtle.Rotate(x, y, z));

                if (drawer != null) drawer.StopDrawing();
            }
            else
            {
                Debug.LogError($"[TurtleManager] rotate 인자 파싱 실패 또는 거북이 없음: '{args}'");
            }
        }
        else
        {
            Debug.LogError($"[TurtleManager] 명령 해석 실패: {cmd}");
        }

        yield return new WaitForSeconds(stepDelay);
        isProcessing = false;
    }

    private bool TryParseExpression(string s, out float result)
    {
        s = s.Trim();
        var mul = s.Split('*');
        if (mul.Length == 2
            && TryParseExpression(mul[0], out float left)
            && TryParseExpression(mul[1], out float right))
        {
            result = left * right;
            return true;
        }

        // 1) 접미사 f/F 제거
        if (s.EndsWith("f", System.StringComparison.OrdinalIgnoreCase))
            s = s[..^1];

        // 2) sqrt(...) 패턴 처리
        var m = Regex.Match(s, @"^sqrt\((.+)\)$", RegexOptions.IgnoreCase);
        if (m.Success)
        {
            if (TryParseExpression(m.Groups[1].Value, out float inner))
            {
                result = Mathf.Sqrt(inner);
                return true;
            }
        }

        // 3) 일반 실수 파싱 (지수 표기법 포함)
        return float.TryParse(s,
                              NumberStyles.Float | NumberStyles.AllowThousands,
                              CultureInfo.InvariantCulture,
                              out result);
    }

    /********************************************************
     * ⑥ 비활성 개체를 풀에서 가져오기
     ********************************************************/
    private GameObject GetTurtleFromPool()
    {
        foreach (var go in turtlePool)
            if (!go.activeSelf) return go;
        return null;
    }

    /********************************************************
     * ⑦ 모든 거북이 초기화
     ********************************************************/
    public void ResetAllTurtles()
    {
        foreach (var go in turtlePool)
        {
            go.SetActive(false);
            go.transform.SetParent(gridParent, false);
            go.transform.localPosition = spawnPosition;
            go.transform.localRotation = spawnRotation;
            go.transform.localScale = turtleScale;

            var drawer = go.GetComponentInChildren<TurtleDrawer>();
            if (drawer != null)
                drawer.ClearTrail();
        }
        namedTurtles.Clear();
        variables.Clear();
        commandQueue.Clear();
        isProcessing = false;
        Debug.Log("[TurtleManager] ResetAllTurtles() 호출—완전 초기화");
    }
}