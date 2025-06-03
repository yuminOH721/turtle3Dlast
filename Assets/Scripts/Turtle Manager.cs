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
    private readonly Dictionary<string, object> variables = new();
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

        // ────────────────────────────────────────────────────────────────────────────
        // 0) “거북이 생성” 구문 처리: a = Turtle()
        var turtleMatch = Regex.Match(lower, @"^([a-zA-Z_]\w*)\s*=\s*turtle\(\)\s*$");
        if (turtleMatch.Success)
        {
            string varName = turtleMatch.Groups[1].Value;
            GameObject go = GetTurtleFromPool();
            if (go != null)
            {
                go.SetActive(true);
                var t = go.GetComponent<Turtle3D>();
                t.Initialize(varName, spawnPosition, spawnRotation);
                namedTurtles[varName] = t;
                t.GetComponentInChildren<TurtleDrawer>().StartDrawing();
            }
            else
            {
                PrintError("[TurtleManager] 풀에 남은 거북이 없음.");
            }
            yield break;
        }

        // ────────────────────────────────────────────────────────────────────────────
        // 1) “변수 대입” 구문 처리 (각종 리터럴 및 기존 변수 복사)
        var assignMatch = Regex.Match(raw, @"^([a-zA-Z_]\w*)\s*=\s*(.+)$");
        if (assignMatch.Success)
        {
            string varName = assignMatch.Groups[1].Value;
            string rhsText = assignMatch.Groups[2].Value;

            object value = null;
            string trimmed = rhsText.Trim();

            // -- 문자열 리터럴 "…"
            if (trimmed.Length >= 2 && trimmed.StartsWith("\"") && trimmed.EndsWith("\""))
            {
                value = trimmed.Substring(1, trimmed.Length - 2);
            }
            // -- char 리터럴 'x'
            else if (trimmed.Length >= 3 && trimmed.StartsWith("'") && trimmed.EndsWith("'"))
            {
                string inner = trimmed.Substring(1, trimmed.Length - 2);
                if (inner.Length == 1)
                    value = inner[0];
                else
                    value = inner;
            }
            // -- 불리언 리터럴 true/false
            else if (trimmed.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                     trimmed.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                value = trimmed.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
            // -- 실수 리터럴 (소수점 포함)
            else if (float.TryParse(trimmed.TrimEnd('f', 'F'),
                                    NumberStyles.Float | NumberStyles.AllowThousands,
                                    CultureInfo.InvariantCulture,
                                    out float fVal))
            {
                if (trimmed.EndsWith("f", StringComparison.OrdinalIgnoreCase))
                    value = fVal;
                else
                    value = (double)fVal;
            }
            // -- 정수 리터럴
            else if (int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out int iVal))
            {
                value = iVal;
            }
            // -- 기존에 저장된 변수 복사
            else if (variables.TryGetValue(trimmed, out object existing))
            {
                value = existing;
            }
            else
            {
                PrintError($"[TurtleManager] 대입 실패: '{trimmed}'를 파싱할 수 없음");
                yield break;
            }

            variables[varName] = value;
            Debug.Log($"[변수 저장] {varName} = ({value} : {value.GetType().Name})");
            yield break;
        }
        // ────────────────────────────────────────────────────────────────────────────

        // 2) normalized 생성
        string normalized = Regex.Replace(raw, @"\s+", " ").Trim();

        // ────────────────────────────────────────────────────────────────────────────
        // print(...) 문법 검사: 반드시 print( ... ) 형태여야 함
        if (lower.StartsWith("print"))
        {
            // 올바른 구문: print( ... )
            var printSyntaxMatch = Regex.Match(normalized, @"^print\(.+\)$");
            if (!printSyntaxMatch.Success)
            {
                PrintError("[TurtleManager] print 문법 오류");
                yield break;
            }

            // 괄호 안 전체 내용 추출
            string inside = raw.Substring(raw.IndexOf('(') + 1,
                                          raw.LastIndexOf(')') - raw.IndexOf('(') - 1);

            // 호출된 print 로직: 여러 인수와 표현식 평가 지원
            string[] parts = inside.Split(',');
            List<string> evaluated = new List<string>();
            foreach (var part in parts)
            {
                string expr = part.Trim();
                string resultStr;

                // 문자열 리터럴
                if (expr.Length >= 2 && expr.StartsWith("\"") && expr.EndsWith("\""))
                {
                    resultStr = expr.Substring(1, expr.Length - 2);
                }
                else if (expr.Length >= 2 && expr.StartsWith("'") && expr.EndsWith("'"))
                {
                    string inner = expr.Substring(1, expr.Length - 2);
                    resultStr = inner;
                }
                // 변수 참조
                else if (variables.TryGetValue(expr, out object objVal))
                {
                    resultStr = objVal.ToString();
                }
                // 불리언 리터럴
                else if (expr.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                         expr.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    resultStr = expr.ToLower();
                }
                // 숫자 표현식 (TryParseExpression으로 수식 평가)
                else if (TryParseExpression(expr, out float numVal))
                {
                    resultStr = numVal.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    // 알 수 없는 표현식은 그대로 출력
                    resultStr = expr;
                }

                evaluated.Add(resultStr);
            }

            // 공백 한 칸으로 이어붙여 출력
            string finalOutput = string.Join(" ", evaluated);
            terminalText.text = finalOutput;
            Debug.Log($"[print] {finalOutput}");
            yield break;
        }
        // ────────────────────────────────────────────────────────────────────────────

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
            string condition = raw
                .Substring(raw.IndexOf("if", StringComparison.Ordinal) + 2)
                .TrimEnd(':')
                .Trim();

            var bodyCmds = DequeueBlock(indent);
            if (!EvaluateCondition(condition))
                yield break;

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
            if (namedTurtles.TryGetValue(name, out var t)
                && float.TryParse(rgb[0], out float r)
                && float.TryParse(rgb[1], out float g)
                && float.TryParse(rgb[2], out float b))
            {
                t.GetComponentInChildren<TurtleDrawer>().SetPenColor(new Color(r, g, b));
            }
            else
                PrintError($"[TurtleManager] pencolor 파싱 실패: {raw}");
            yield break;
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

        // 1) “+” 또는 “-” 처리 (뎁스 0에서, 앞부분이 음수 부호인지 아닌지 고려)
        int depth = 0;
        for (int i = s.Length - 1; i >= 0; i--)
        {
            char c = s[i];
            if (c == ')') depth++;
            else if (c == '(') depth--;
            else if (depth == 0 && (c == '+' || c == '-'))
            {
                // 맨 앞에 있으면 부호일 뿐 연산자가 아니므로 건너뜀
                if (i == 0)
                    continue;
                char prev = s[i - 1];
                // 앞 문자가 연산자거나 ‘(’이면 부호로 간주하고 건너뜀
                if (prev == '(' || prev == '+' || prev == '-' || prev == '*' || prev == '/' || prev == '%')
                    continue;

                // “왼쪽”과 “오른쪽”을 재귀로 파싱
                string left = s.Substring(0, i);
                string right = s.Substring(i + 1);
                if (TryParseExpression(left, out float leftVal) &&
                    TryParseExpression(right, out float rightVal))
                {
                    result = (c == '+') ? leftVal + rightVal : leftVal - rightVal;
                    return true;
                }

                result = 0f;
                return false;
            }
        }

        // 2) “*”, “/”, “%” 처리 (뎁스 0)
        depth = 0;
        for (int i = s.Length - 1; i >= 0; i--)
        {
            char c = s[i];
            if (c == ')') depth++;
            else if (c == '(') depth--;
            else if (depth == 0 && (c == '*' || c == '/' || c == '%'))
            {
                string left = s.Substring(0, i);
                string right = s.Substring(i + 1);
                if (TryParseExpression(left, out float leftVal) &&
                    TryParseExpression(right, out float rightVal))
                {
                    switch (c)
                    {
                        case '*':
                            result = leftVal * rightVal;
                            return true;
                        case '/':
                            if (rightVal == 0f)
                            {
                                // 0으로 나누면 실패로 처리
                                result = 0f;
                                return false;
                            }
                            result = leftVal / rightVal;
                            return true;
                        case '%':
                            if (rightVal == 0f)
                            {
                                result = 0f;
                                return false;
                            }
                            result = leftVal % rightVal;
                            return true;
                    }
                }

                result = 0f;
                return false;
            }
        }

        // 3) sqrt(...) 함수 지원
        var sqrtMatch = Regex.Match(s, @"^\s*sqrt\((.+)\)\s*$", RegexOptions.IgnoreCase);
        if (sqrtMatch.Success)
        {
            string inner = sqrtMatch.Groups[1].Value;
            if (TryParseExpression(inner, out float innerVal))
            {
                result = Mathf.Sqrt(innerVal);
                return true;
            }
            result = 0f;
            return false;
        }

        // 4) 변수 참조: 변수 딕셔너리에서 숫자형(int, float, double) 꺼내기
        if (variables.TryGetValue(s, out object varObj))
        {
            switch (varObj)
            {
                case int iVal: result = iVal; return true;
                case float fVal: result = fVal; return true;
                case double dVal: result = (float)dVal; return true;
            }
        }

        // 5) 접미사 f/F가 붙은 실수 리터럴
        if (s.EndsWith("f", StringComparison.OrdinalIgnoreCase))
        {
            string numericPart = s.Substring(0, s.Length - 1).Trim();
            if (float.TryParse(numericPart,
                               NumberStyles.Float | NumberStyles.AllowThousands,
                               CultureInfo.InvariantCulture,
                               out float fLiteral))
            {
                result = fLiteral;
                return true;
            }
        }

        // 6) 일반 실수/정수 리터럴
        if (float.TryParse(s,
                           NumberStyles.Float | NumberStyles.AllowThousands,
                           CultureInfo.InvariantCulture,
                           out float fVal2))
        {
            result = fVal2;
            return true;
        }

        // 7) 모두 해당하지 않으면 실패
        result = 0f;
        return false;
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

        // 1) Boolean 리터럴
        if (condition.Equals("true", StringComparison.OrdinalIgnoreCase))
            return true;
        if (condition.Equals("false", StringComparison.OrdinalIgnoreCase))
            return false;

        // 2) 숫자 (기존 로직)
        if (float.TryParse(condition, out float num))
            return num != 0;

        // 3) 변수 값 검사
        if (variables.TryGetValue(condition, out var obj))
        {
            if (obj is bool b) return b;
            if (obj is float f) return f != 0;
            if (obj is Vector3 v) return v.x != 0; // 기존 방식
            if (obj is string s) return !string.IsNullOrEmpty(s);
        }

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

