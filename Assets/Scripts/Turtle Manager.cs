using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

//================================================================================
// TurtleManager: 명령 해석, 풀 관리, UI 출력, 바운드 체크, 펜 제어
//================================================================================
public class TurtleManager : MonoBehaviour
{
    [Header("Grid Parent for Turtles")] public Transform gridParent;
    [Header("Prefabs & UI")] public GameObject turtlePrefab;
    public TMP_InputField commandInput;
    [Header("Spawn Settings")] public int maxTurtles = 5;
    public static Vector3 spawnPosition = Vector3.zero;
    public static readonly Quaternion spawnRotation = Quaternion.identity;
    [Header("Turtle Appearance")] public Vector3 turtleScale = Vector3.one;
    [Header("Movement Settings")] public float movementScale = 1f;
    [Header("Timing")][SerializeField] private float stepDelay = 0.5f;
    [Header("UI")] public TMP_Text terminalText;
    [Header("Pen Settings")] public float minPenSize = 0.01f;
    public float maxPenSize = 0.1f;

    private readonly List<GameObject> turtlePool = new();
    private readonly Dictionary<string, Turtle3D> namedTurtles = new();
    private readonly Dictionary<string, Vector3> variables = new();
    private readonly Queue<Command> commandQueue = new Queue<Command>();

    private bool isProcessing;

    public static TurtleManager instance;
    private BoxCollider gridCollider;

    void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);

        if (gridParent == null)
            Debug.LogError("[TurtleManager] gridParent 미할당! Inspector에서 Grid 오브젝트 연결 필요.");
        else
        {
            gridCollider = gridParent.GetComponent<BoxCollider>();
            if (gridCollider != null)
            {
                // 로컬 사이즈(Scale 미반영) × 부모 LossyScale → 진짜 월드 한 칸 크기
                float worldGridSize = gridCollider.bounds.size.x;

                // 2) 한 칸당 월드 크기
                float worldCellSize = worldGridSize / 6f;

                // 3) forward(1) == 1칸 == 1 * worldCellSize 이동
                movementScale = worldCellSize;

                // spawnPosition 계산은 그대로
                Vector3 centerWorld = gridCollider.bounds.center;
                spawnPosition = gridParent.InverseTransformPoint(centerWorld);
            }
        }

        CreateTurtlePool();
        if (commandInput == null) Debug.LogWarning("[TurtleManager] commandInput 미할당.");
        if (terminalText == null) Debug.LogWarning("[TurtleManager] terminalText 미할당.");
    }

    void Start()
    {
        ResetAllTurtles();  // Play 버튼 클릭 시 초기화
    }

    void CreateTurtlePool()
    {
        foreach (var go in turtlePool) Destroy(go);
        turtlePool.Clear();

        for (int i = 0; i < maxTurtles; i++)
        {
            // 부모(transform) 아래 로컬 위치 spawnPosition 에 인스턴스
            var go = Instantiate(turtlePrefab, gridParent);
            go.transform.localPosition = spawnPosition;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = turtleScale;
            go.SetActive(false);
            turtlePool.Add(go);
        }
    }

    public void ExecuteCurrentCommand()
    {
        var lines = commandInput.text.Split('\n');
        foreach (var line in lines)
        {
            // 원본 앞부분의 공백 개수 세기
            int indent = 0;
            while (indent < line.Length && char.IsWhiteSpace(line[indent])) indent++;

            // Trim()으로 양끝 공백만 지운 실제 명령문
            var raw = line.Trim();
            if (raw.Length == 0) continue;

            commandQueue.Enqueue(new Command(raw, indent));
        }
    }


    void Update()
    {
        if (!isProcessing && commandQueue.Count > 0)
            StartCoroutine(ProcessCommand(commandQueue.Dequeue()));
    }

    void PrintError(string msg)
    {
        Debug.LogError(msg);
        if (terminalText != null) terminalText.text = msg;
    }

    private IEnumerator ProcessCommand(Command cmd)
    {
        isProcessing = true;
        string raw = cmd.Raw;
        string lower = raw.ToLowerInvariant();


        yield return StartCoroutine(DispatchCommand(cmd));

        yield return new WaitForSeconds(stepDelay);
        isProcessing = false;
    }

    private IEnumerator DispatchCommand(Command cmd)
    {
        string raw = cmd.Raw;
        int indent = cmd.Indent;
        string lower = raw.ToLowerInvariant();

        // 1) normalized 검사
        string normalized = Regex.Replace(raw, @"\s+", " ").Trim();

        // print("...")
        if (normalized.StartsWith("print(") && normalized.EndsWith(")"))
        {
            string content = raw.Substring(raw.IndexOf('(') + 1,
                                           raw.LastIndexOf(')') - raw.IndexOf('(') - 1);
            terminalText.text = content;
            Debug.Log($"[print] {content}");
            yield break;
        }

        // for i in range(n):
        if (Regex.IsMatch(normalized, @"^for [a-zA-Z_]\w* in range\(\d+\):$"))
        {
            var bodyCmds = DequeueBlock(indent);
            if (bodyCmds.Count == 0)
            {
                PrintError("[TurtleManager] 파이썬 문법 오류: for문 블록 없음");
                yield break;
            }

            string[] tokens = normalized
                .Split(new[] { ' ', '(', ')', ':' }, StringSplitOptions.RemoveEmptyEntries);
            int count = int.Parse(tokens[4]);

            for (int i = 0; i < count; i++)
                foreach (var c in bodyCmds)
                    commandQueue.Enqueue(c);

            yield break;
        }

        // if condition:
        if (Regex.IsMatch(normalized, @"^if .+:$"))
        {
            // 조건식 추출
            string condition = raw
                .Substring(raw.IndexOf("if", StringComparison.Ordinal) + 2)
                .TrimEnd(':')
                .Trim();

            // 블록 모두 꺼내서
            var bodyCmds = DequeueBlock(indent);

            // 조건이 false면 그냥 버리고
            if (!EvaluateCondition(condition))
                yield break;

            // true면 다시 enqueue
            foreach (var c in bodyCmds)
                commandQueue.Enqueue(c);

            yield break;
        }

        // while condition:
        if (Regex.IsMatch(normalized, @"^while .+:$"))
        {
            string condition = raw
                .Substring(raw.IndexOf("while", StringComparison.Ordinal) + 5)
                .TrimEnd(':')
                .Trim();

            var bodyCmds = DequeueBlock(indent);
            if (bodyCmds.Count == 0)
            {
                PrintError("[TurtleManager] while문 블록 없음");
                yield break;
            }

            int maxLoop = 1000;
            bool breakLoop = false;
            while (EvaluateCondition(condition) && maxLoop-- > 0)
            {
                foreach (var c in bodyCmds)
                {
                    if (c.Raw.Trim() == "break")
                    {
                        breakLoop = true;
                        break;
                    }
                    commandQueue.Enqueue(c);
                }
                if (breakLoop) break;
            }
            if (maxLoop <= 0)
                PrintError("[TurtleManager] while문 루프가 너무 깁니다.");

            yield break;
        }

        // 나머지 기본 명령
        yield return StartCoroutine(HandleBuiltinCommands(raw, lower));
    }


    private IEnumerator HandleBuiltinCommands(string raw, string lower)
    {
        // 1) 생성: a=Turtle()
        if (lower.EndsWith("turtle()") && raw.Contains("="))
        {
            var parts = raw.Split('=');
            var go = GetTurtleFromPool();
            if (go != null)
            {
                go.SetActive(true);
                // ===== here we trim so "a = Turtle()" 의 parts[0] "a " → "a" 로
                var name = parts[0].Trim();
                var t = go.GetComponent<Turtle3D>();
                t.Initialize(name, spawnPosition, spawnRotation);
                namedTurtles[name] = t;
                t.GetComponentInChildren<TurtleDrawer>().StartDrawing();
            }
            else PrintError("[TurtleManager] 풀에 남은 거북이 없음.");
        }
        // 2) 위치 저장: v=a.position()
        else if (lower.EndsWith(".position()") && raw.Contains("="))
        {
            var parts = raw.Split('=');
            var varName = parts[0].Trim();                          // 왼쪽 변수
            var rhs = parts[1].Trim();                          // "a.position()"
            var key = rhs.Substring(0, rhs.IndexOf('.', StringComparison.Ordinal)).Trim();
            if (namedTurtles.TryGetValue(key, out var t))
            {
                variables[varName] = t.Position;
                Debug.Log($"{varName}=({t.Position.x:F2},{t.Position.y:F2},{t.Position.z:F2})");
            }
            else PrintError($"[TurtleManager] 위치 저장 실패: {key} 없음.");
        }
        // 3) 이동: forward/fd (바운드 체크)
        else if ((lower.Contains(".forward(") || lower.Contains(".fd(")) && raw.EndsWith(")"))
        {
            var token = lower.Contains(".forward(") ? ".forward(" : ".fd(";
            var idx = lower.IndexOf(token, StringComparison.Ordinal);
            var name = raw.Substring(0, idx);

            int start = idx + token.Length;
            int end = raw.LastIndexOf(')');
            string numText = raw.Substring(start, end - start);

            if (!float.TryParse(numText, out float requestedUnits))
            {
                PrintError($"[TurtleManager] forward 파싱 실패: {raw}");
                yield break;
            }

            if (!namedTurtles.TryGetValue(name, out var t))
            {
                PrintError($"[TurtleManager] forward 대상 거북이 없음: {name}");
                yield break;
            }

            float cellSize = CellSize;
            float requestedDist = requestedUnits * cellSize;

            // 로컬 좌표 기준 이동
            Vector3 localStart = t.transform.localPosition;
            Vector3 localDir = t.transform.localRotation * Vector3.forward;
            Vector3 localTarget = localStart + localDir * requestedDist;

            Vector3 center = gridCollider.center;
            Vector3 size = gridCollider.size;
            Vector3 halfSize = size * 0.5f;

            float epsilon = 1e-4f;  // 허용 오차
            bool insideX = localTarget.x >= (center.x - halfSize.x - epsilon) && localTarget.x <= (center.x + halfSize.x + epsilon);
            bool insideY = localTarget.y >= (center.y - halfSize.y - epsilon) && localTarget.y <= (center.y + halfSize.y + epsilon);
            bool insideZ = localTarget.z >= (center.z - halfSize.z - epsilon) && localTarget.z <= (center.z + halfSize.z + epsilon);

            // 디버깅 로그
            Debug.Log($"[Debug] 🐢 {name}.forward({requestedUnits})");
            Debug.Log($"Start(local): {localStart}, Dir: {localDir.normalized}, Target(local): {localTarget}");
            Debug.Log($"Grid Center(local): {center}, HalfSize: {halfSize}");
            Debug.Log($"Inside Check → X: {insideX}, Y: {insideY}, Z: {insideZ}");

            if (insideX && insideY && insideZ)
            {
                yield return StartCoroutine(t.Forward(requestedUnits));
            }
            else
            {
                PrintError($"[TurtleManager] 이동 범위 벗어남: {name}");
            }
        }
        // 4) 일반 회전: rotate(x,y,z)
        else if (lower.Contains(".rotate(") && raw.EndsWith(")"))
        {
            const string prefix = ".rotate(";
            var idx = lower.IndexOf(prefix, StringComparison.Ordinal);
            var name = raw.Substring(0, idx);
            int start = idx + prefix.Length;
            int end = raw.LastIndexOf(')');
            var parts = raw.Substring(start, end - start).Split(',');
            if (parts.Length == 3
                && namedTurtles.TryGetValue(name, out var t)
                && TryParseExpression(parts[0], out float rx)
                && TryParseExpression(parts[1], out float ry)
                && TryParseExpression(parts[2], out float rz))
            {
                yield return StartCoroutine(t.Rotate(rx, ry, rz));
            }
            else PrintError($"[TurtleManager] rotate 파싱 실패: {raw}");
        }
        // 5) rotatex / rotatey / rotatez
        else if (lower.Contains(".rotatex(") && raw.EndsWith(")"))
        {
            const string prefix = ".rotatex(";
            var idx = lower.IndexOf(prefix, StringComparison.Ordinal);
            var name = raw.Substring(0, idx);
            int start = idx + prefix.Length;
            int end = raw.LastIndexOf(')');
            var arg = raw.Substring(start, end - start);
            if (namedTurtles.TryGetValue(name, out var t) && TryParseExpression(arg, out float x))
                yield return StartCoroutine(t.Rotate(x, 0, 0));
            else PrintError($"[TurtleManager] rotatex 파싱 실패: {raw}");
        }
        else if (lower.Contains(".rotatey(") && raw.EndsWith(")"))
        {
            const string prefix = ".rotatey(";
            var idx = lower.IndexOf(prefix, StringComparison.Ordinal);
            var name = raw.Substring(0, idx);
            int start = idx + prefix.Length;
            int end = raw.LastIndexOf(')');
            var arg = raw.Substring(start, end - start);
            if (namedTurtles.TryGetValue(name, out var t) && TryParseExpression(arg, out float y))
                yield return StartCoroutine(t.Rotate(0, y, 0));
            else PrintError($"[TurtleManager] rotatey 파싱 실패: {raw}");
        }
        else if (lower.Contains(".rotatez(") && raw.EndsWith(")"))
        {
            const string prefix = ".rotatez(";
            var idx = lower.IndexOf(prefix, StringComparison.Ordinal);
            var name = raw.Substring(0, idx);
            int start = idx + prefix.Length;
            int end = raw.LastIndexOf(')');
            var arg = raw.Substring(start, end - start);
            if (namedTurtles.TryGetValue(name, out var t) && TryParseExpression(arg, out float z))
                yield return StartCoroutine(t.Rotate(0, 0, z));
            else PrintError($"[TurtleManager] rotatez 파싱 실패: {raw}");
        }
        // 6) pencolor(r,g,b)
        else if (lower.Contains(".pencolor(") && raw.EndsWith(")"))
        {
            const string prefix = ".pencolor(";
            var idx = lower.IndexOf(prefix, StringComparison.Ordinal);
            var name = raw.Substring(0, idx);
            int start = idx + prefix.Length;
            int end = raw.LastIndexOf(')');
            var rgb = raw.Substring(start, end - start).Split(',');
            if (namedTurtles.TryGetValue(name, out var t) && rgb.Length == 3
                && float.TryParse(rgb[0], out float r)
                && float.TryParse(rgb[1], out float g)
                && float.TryParse(rgb[2], out float b))
            {
                t.GetComponentInChildren<TurtleDrawer>().SetPenColor(new Color(r, g, b));
            }
            else PrintError($"[TurtleManager] pencolor 파싱 실패: {raw}");
        }
        // 7) pensize(n)
        else if (lower.Contains(".pensize(") && raw.EndsWith(")"))
        {
            const string prefix = ".pensize(";
            var idx = lower.IndexOf(prefix, StringComparison.Ordinal);
            var name = raw.Substring(0, idx);
            int start = idx + prefix.Length;
            int end = raw.LastIndexOf(')');
            var num = raw.Substring(start, end - start);
            if (namedTurtles.TryGetValue(name, out var t) && TryParseExpression(num, out float size))
            {
                size = Mathf.Clamp(size, minPenSize, maxPenSize);
                t.GetComponentInChildren<TurtleDrawer>().SetPenSize(size);
            }
            else PrintError($"[TurtleManager] pensize 파싱 실패: {raw}");
        }
        // 8) pendown / pd
        else if (lower.EndsWith(".pendown()") || lower.EndsWith(".pd()"))
        {
            var name = raw.Substring(0, raw.IndexOf('.', StringComparison.Ordinal));
            if (namedTurtles.TryGetValue(name, out var t))
                t.GetComponentInChildren<TurtleDrawer>().StartDrawing();
            else
                PrintError($"[TurtleManager] pendown 실패: {name}");
            yield break;
        }
        // 9) penup / pu
        else if (lower.EndsWith(".penup()") || lower.EndsWith(".pu()"))
        {
            var name = raw.Substring(0, raw.IndexOf('.', StringComparison.Ordinal));
            if (namedTurtles.TryGetValue(name, out var t))
                t.GetComponentInChildren<TurtleDrawer>().StopDrawing();
            else
                PrintError($"[TurtleManager] penup 실패: {name}");
            yield break;
        }
        else
        {
            PrintError($"[TurtleManager] 명령 해석 실패: {raw}");
        }
    }


    private bool TryParseExpression(string s, out float result)
    {
        s = s.Trim();
        var mul = s.Split('*');
        if (mul.Length == 2 && TryParseExpression(mul[0], out var l) && TryParseExpression(mul[1], out var r))
        {
            result = l * r; return true;
        }
        if (s.EndsWith("f", StringComparison.OrdinalIgnoreCase)) s = s[..^1];
        var m = Regex.Match(s, @"^sqrt\((.+)\)$", RegexOptions.IgnoreCase);
        if (m.Success && TryParseExpression(m.Groups[1].Value, out var inner))
        {
            result = Mathf.Sqrt(inner); return true;
        }
        return float.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result);
    }

    private GameObject GetTurtleFromPool()
    {
        foreach (var go in turtlePool)
            if (!go.activeSelf) return go;
        return null;
    }

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
        {
            drawer.ClearAllTrails();     
            drawer.StartDrawing();      
        }
    }
    namedTurtles.Clear();
    variables.Clear();
    commandQueue.Clear();
    isProcessing = false;
    PrintError("[TurtleManager] 완전 초기화");
}

    public float CellSize
    {
        get
        {
            if (gridCollider == null) return 0f;
            return gridCollider.size.x / 6f;
        }
    }

    private bool EvaluateCondition(string condition)
    {
        condition = condition.Trim();

        // 숫자면 0이 아닌 경우 true
        if (float.TryParse(condition, out float result))
            return result != 0;

        // 변수 값이 있다면 x != 0 이면 true
        if (variables.TryGetValue(condition, out var val))
            return val.x != 0;

        // 그 외는 false
        return false;
    }

    private List<Command> DequeueBlock(int parentIndent)
    {
        var block = new List<Command>();
        while (commandQueue.Count > 0 && commandQueue.Peek().Indent > parentIndent)
            block.Add(commandQueue.Dequeue());
        return block;
    }
}


class Command
{
    public readonly string Raw;       // Trim()된 텍스트
    public readonly int Indent;       //  앞 공백(스페이스/탭) 개수
    public Command(string raw, int indent)
    {
        Raw = raw; Indent = indent;
    }


}

