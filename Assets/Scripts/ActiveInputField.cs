using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ActiveInputField : MonoBehaviour
{
    [Header("탭 들여쓰기 공백 개수")]
    [Tooltip("Tab 키를 누를 때 삽입될 공백 개수")]
    public int tabSize = 4;

    [Header("커서 깜빡임 주기(초)")]
    [Tooltip("커서가 깜빡이는 주기(예: 0.5초)")]
    public float blinkRate = 0.5f;

    // ————————————— 내부 참조 —————————————
    private TextMeshProUGUI inputText;   // 화면에 보이는 TextMeshProUGUI
    private Canvas _parentCanvas;        // 상위 World Space Canvas

    // 커서 관련
    private RectTransform _caretRect;
    private Image _caretImage;
    private bool _caretVisible = true;
    private float _nextBlinkTime = 0f;

    // 최종 확정된(완성된) 문자열
    private string _currentText = "";
    // 커서가 가리키는 문자열 인덱스 (0~_currentText.Length)
    private int _caretPosition = 0;

    // ————————————— 한글 조합 상태 —————————————
    // L_index: 초성 인덱스(0~18), -1은 조합 중 아님
    // V_index: 중성 인덱스(0~20), -1은 아직 없음
    // T_index: 종성 인덱스(0~27), 0은 종성 없음
    private int L_index = -1;
    private int V_index = -1;
    private int T_index = 0;

    // ————————————— 한영토글 모드 —————————————
    // 한영키를 눌러서 한글 입력 모드(On), 영문 모드(Off)를 전환
    private bool isHangulMode = false;

    // ————————————— 두벌식 자모 매핑 —————————————
    // (1) 초성 매핑: Dictionary 키(KeyCode)에 중복을 피하고,
    //    “ㄲ, ㄸ, ㅃ, ㅆ, ㅉ” 등은 Shift+영문키로 처리
    private Dictionary<KeyCode, int> initialMap = new Dictionary<KeyCode, int>()
    {
        { KeyCode.R, 0 },  // ㄱ (0)
        { KeyCode.S, 2 },  // ㄴ (2)
        { KeyCode.E, 3 },  // ㄷ (3)
        { KeyCode.F, 5 },  // ㄹ (5)
        { KeyCode.A, 6 },  // ㅁ (6)
        { KeyCode.Q, 7 },  // ㅂ (7)
        { KeyCode.T, 9 },  // ㅅ (9)
        { KeyCode.D, 11 }, // ㅇ (11)
        { KeyCode.W, 12 }, // ㅈ (12)
        { KeyCode.C, 14 }, // ㅊ (14)
        { KeyCode.Z, 15 }, // ㅋ (15)
        { KeyCode.X, 16 }, // ㅌ (16)
        { KeyCode.V, 17 }, // ㅍ (17)
        { KeyCode.G, 18 }, // ㅎ (18)
        // ㄲ(1) : Shift+R
        // ㄸ(4) : Shift+E
        // ㅃ(8) : Shift+Q
        // ㅆ(10): Shift+T
        // ㅉ(13): Shift+W
    };

    // (2) 중성 매핑
    private Dictionary<KeyCode, int> medialMap = new Dictionary<KeyCode, int>()
    {
        { KeyCode.K, 0 },  // ㅏ (0)
        { KeyCode.O, 1 },  // ㅐ (1)
        { KeyCode.I, 2 },  // ㅑ (2)
        { KeyCode.J, 4 },  // ㅓ (4)
        { KeyCode.P, 5 },  // ㅔ (5)
        { KeyCode.U, 6 },  // ㅕ (6)
        { KeyCode.H, 8 },  // ㅗ (8)
        { KeyCode.Y, 12 }, // ㅛ (12)
        { KeyCode.N, 13 }, // ㅜ (13)
        { KeyCode.B, 17 }, // ㅠ (17)
        { KeyCode.M, 18 }, // ㅡ (18)
        { KeyCode.L, 20 }, // ㅣ (20)
        // ㅒ(3), ㅖ(7), ㅘ(9), ㅙ(10), ㅚ(11), ㅝ(14), ㅞ(15), ㅟ(16), ㅢ(19)
        // 은 아래의 CombineMedial() 함수에서 “조합 규칙”으로 처리합니다.
    };

    // (3) 복합 중성 조합 규칙: “oldV * 100 + newV → 결과 인덱스”
    private Dictionary<int, int> medialCombine = new Dictionary<int, int>()
    {
        { 8 * 100 + 0, 9 },    // ㅗ + ㅏ = ㅘ (9)
        { 8 * 100 + 1, 10 },   // ㅗ + ㅐ = ㅙ (10)
        { 8 * 100 + 11, 11 },  // ㅗ + ㅣ = ㅚ (11)
        { 13 * 100 + 4, 14 },  // ㅜ + ㅓ = ㅝ (14)
        { 13 * 100 + 5, 15 },  // ㅜ + ㅔ = ㅞ (15)
        { 13 * 100 + 20, 16 }, // ㅜ + ㅣ = ㅟ (16)
        { 18 * 100 + 20, 19 }, // ㅡ + ㅣ = ㅢ (19)
    };

    // (4) 종성 매핑
    private Dictionary<KeyCode, int> finalMap = new Dictionary<KeyCode, int>()
    {
        { KeyCode.R, 1 },    // ㄱ (1)
        { KeyCode.S, 4 },    // ㄴ (4)
        { KeyCode.E, 7 },    // ㄷ (7)
        { KeyCode.F, 8 },    // ㄹ (8)
        { KeyCode.A, 16 },   // ㅁ (16)
        { KeyCode.Q, 17 },   // ㅂ (17)
        { KeyCode.T, 19 },   // ㅅ (19)
        { KeyCode.D, 21 },   // ㅇ (21)
        { KeyCode.W, 22 },   // ㅈ (22)
        { KeyCode.C, 23 },   // ㅊ (23)
        { KeyCode.Z, 24 },   // ㅋ (24)
        { KeyCode.X, 25 },   // ㅌ (25)
        { KeyCode.V, 26 },   // ㅍ (26)
        { KeyCode.G, 27 },   // ㅎ (27)
        // 복합 종성(ㄳ,ㄵ,ㄶ,ㄺ,ㄻ,ㄼ,ㄽ,ㄾ,ㄿ,ㅀ,ㅄ) 은 CombineFinal() 에서 처리
    };

    // 화면에 임시로 보여 줄 자모/음절용 배열
    private readonly char[] InitialJamoChars = new char[]
    {
        'ㄱ','ㄲ','ㄴ','ㄷ','ㄸ','ㄹ','ㅁ','ㅂ','ㅃ','ㅅ','ㅆ','ㅇ','ㅈ','ㅉ','ㅊ','ㅋ','ㅌ','ㅍ','ㅎ'
    };
    private readonly char[] MedialJamoChars = new char[]
    {
        'ㅏ','ㅐ','ㅑ','ㅒ','ㅓ','ㅔ','ㅕ','ㅖ','ㅗ','ㅘ','ㅙ','ㅚ','ㅛ','ㅜ','ㅝ','ㅞ','ㅟ','ㅠ','ㅡ','ㅢ','ㅣ'
    };
    private readonly string[] FinalJamoChars = new string[]
    {
        "", "ㄱ","ㄲ","ㄳ","ㄴ","ㄵ","ㄶ","ㄷ","ㄹ","ㄺ","ㄻ","ㄼ","ㄽ","ㄾ","ㄿ","ㅀ","ㅁ","ㅂ","ㅄ","ㅅ","ㅆ","ㅇ","ㅈ","ㅊ","ㅋ","ㅌ","ㅍ","ㅎ"
    };

    void Awake()
    {
        // 1) TextMeshProUGUI 컴포넌트 확보
        inputText = GetComponent<TextMeshProUGUI>();
        if (inputText == null)
        {
            Debug.LogError("[WorldSpaceAdvancedHangulIME] 이 GameObject에 TextMeshProUGUI가 없습니다.");
            enabled = false;
            return;
        }

        // 2) 상위 Canvas 확인 (World Space 모드여야 함)
        _parentCanvas = inputText.GetComponentInParent<Canvas>();
        if (_parentCanvas == null)
        {
            Debug.LogError("[WorldSpaceAdvancedHangulIME] 상위 Canvas가 없습니다.");
            enabled = false;
            return;
        }
        else if (_parentCanvas.renderMode != RenderMode.WorldSpace)
        {
            Debug.LogError("[WorldSpaceAdvancedHangulIME] Render Mode가 World Space가 아닙니다. World Space Canvas 하위에 있어야 합니다.");
            enabled = false;
            return;
        }

        // 3) 커서 이미지 생성 (이 시점에 _caretRect, _caretImage가 만들어짐)
        CreateCaretImage();
    }

    void Start()
    {
        // 커서를 포함한 초기 상태 세팅
        _currentText = "";
        _caretPosition = 0;
        inputText.text = _currentText;
        inputText.ForceMeshUpdate();

        _nextBlinkTime = Time.unscaledTime + blinkRate;
        _caretImage.enabled = true;
    }

    void Update()
    {
        // 0) 한영 토글 키(한/영 모드 전환)
        //    KeyCode.HangulMode 또는 KeyCode.KoreanMode 를 사용
        //    (모든 키보드/플랫폼에서 동일하게 동작하는 건 아닙니다. 
        //     Windows 에서는 KeyCode.Korean? 등으로 잡을 수 있으며, 
        //     필요 시 다른 키로 바꾸셔도 됩니다.)
        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
        {
            isHangulMode = !isHangulMode;
        }

        // 1) 커서 깜빡임 처리
        HandleBlinking();

        // 2) 한영 모드에 따라 입력 처리 분기
        if (isHangulMode)
        {
            // 2-1) 한글 모드: 두벌식 자모 조합 + 기타 입력 처리
            HandleHangulTyping();
        }
        else
        {
            // 2-2) 영문 모드: 완전 수동 한글 조합을 사용하지 않고
            //       Input.inputString 으로 들어오는 문자(영문/숫자/특수)만 삽입
            HandleEnglishTyping();
        }

        // 3) 커서 위치 갱신
        UpdateCaretPosition();
    }

    // ─────────────────────────────────────────────────────────────────
    #region 커서 생성 및 깜빡임

    private void CreateCaretImage()
    {
        GameObject caretGO = new GameObject("Caret", typeof(RectTransform), typeof(Image));
        _caretRect = caretGO.GetComponent<RectTransform>();
        _caretImage = caretGO.GetComponent<Image>();

        // 부모를 World Space Canvas 로 설정
        _caretRect.SetParent(_parentCanvas.transform, worldPositionStays: false);

        // 2px 너비를 World 단위로 환산
        float desiredPixelWidth = 0.001f;
        float widthInUnits = desiredPixelWidth / _parentCanvas.scaleFactor;

        // 한 줄 높이 계산 (fontSize * lossyScale.y)
        float lineHeight = inputText.fontSize * inputText.rectTransform.lossyScale.y;

        _caretRect.sizeDelta = new Vector2(widthInUnits, lineHeight);
        _caretRect.pivot = new Vector2(0f, 1f); // 왼쪽 위 기준

        _caretImage.color = Color.black;
        _caretImage.enabled = true;
    }

    private void HandleBlinking()
    {
        if (Time.unscaledTime >= _nextBlinkTime)
        {
            _caretVisible = !_caretVisible;
            _caretImage.enabled = _caretVisible;
            _nextBlinkTime = Time.unscaledTime + blinkRate;
        }
    }

    #endregion
    // ─────────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────────
    #region 한글 모드 입력 처리

    /// <summary>
    /// 한글 모드일 때 호출되는 메서드입니다.
    /// 1) DetectHangulJamoKeys(): 두벌식 자모(초성·중성·종성) 조합 로직
    /// 2) Input.inputString: Backspace/Enter/영문·숫자·특수문자 삽입
    /// 3) Tab, 방향키 처리
    /// </summary>
    private void HandleHangulTyping()
    {
        // (1) 두벌식 자모 조합 처리
        DetectHangulJamoKeys();

        // (2) Input.inputString 으로 들어오는 문자 처리
        //     → 한글 모드일 때, 알파벳(a~z, A~Z)과 공백(space)은 건너뛰도록 필터링
        string inputThisFrame = Input.inputString;
        if (!string.IsNullOrEmpty(inputThisFrame))
        {
            foreach (char c in inputThisFrame)
            {
                // 2-1) Backspace 처리
                if (c == '\b')
                {
                    HandleBackspace();
                }
                // 2-2) Enter(줄바꿈) 처리
                else if (c == '\n' || c == '\r')
                {
                    // 조합 중이면 우선 확정
                    if (L_index != -1 && V_index != -1)
                        CommitCurrentSyllable();

                    // 줄바꿈 삽입
                    _currentText = _currentText.Insert(_caretPosition, "\n");
                    _caretPosition++;
                    inputText.text = _currentText;
                    inputText.ForceMeshUpdate();
                }
                // 2-3) 스페이스(공백) → DetectHangulJamoKeys()에서 이미 “조합 확정”으로 처리되므로 삽입은 건너뛴다
                else if (c == ' ')
                {
                    continue;
                }
                // 2-4) 알파벳(a~z, A~Z)은 한글 조합용 키이므로 삽입하지 않고 건너뛰기
                else if (char.IsLetter(c))
                {
                    continue;
                }
                // 2-5) 그 외(숫자, 특수문자 등)는 그대로 삽입
                else
                {
                    _currentText = _currentText.Insert(_caretPosition, c.ToString());
                    _caretPosition++;
                    inputText.text = _currentText;
                    inputText.ForceMeshUpdate();
                }
            }
        }

        // (3) Tab: 들여쓰기 (공백 여러 개 삽입) 
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            string spaces = new string(' ', tabSize);
            _currentText = _currentText.Insert(_caretPosition, spaces);
            _caretPosition += tabSize;
            inputText.text = _currentText;
            inputText.ForceMeshUpdate();
        }

        // (4) 방향키: 커서 인덱스 이동
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (_caretPosition > 0) _caretPosition--;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (_caretPosition < _currentText.Length) _caretPosition++;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveCaretVertically(true);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveCaretVertically(false);
        }
    }

    /// <summary>
    /// 두벌식 자모(초성·중성·종성) 조합을 담당합니다.
    /// - finalMap(종성) 처리 → initialMap(초성) 처리 → medialMap(중성) 처리 순서 고정
    /// - “드디”／“안녕하세요” 처럼, CV 다음에 중성이 붙을 때 종성을 새 음절 초성으로 재활용하는 로직을 추가
    /// </summary>
    private void DetectHangulJamoKeys()
    {
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // 1) **종성(final) 처리 우선**
        foreach (var kv in finalMap)
        {
            KeyCode kc = kv.Key;
            int idx = kv.Value; // 붙이려는 종성 인덱스(1~27)

            if (Input.GetKeyDown(kc))
            {
                // (1-0) 초성+중성(C+V)이 갖춰져 있지 않으면 → “종성” 시도 건너뛰기
                if (L_index == -1 || V_index == -1)
                {
                    // return 하지 않고, 이 종성 루프만 넘어가서 아래 초성/중성 검사로 이어지도록
                    continue;
                }

                // (1-1) 아직 종성(T_index)이 없으면 → 단일 종성 설정
                if (T_index == 0)
                {
                    T_index = idx;
                    ShowCurrentComposition();
                    return;
                }
                // (1-2) 이미 종성이 있으면 → 복합 종성 시도
                else
                {
                    int combined = CombineFinal(T_index, idx);
                    if (combined != -1)
                    {
                        // 복합 종성 성공
                        T_index = combined;
                        ShowCurrentComposition();
                        return;
                    }
                    else
                    {
                        // 복합 종성 불가 → 기존 음절을 확정(commit)하고 새 음절로
                        CommitCurrentSyllable();
                        L_index = -1;
                        V_index = -1;
                        T_index = 0;

                        // 이 키가 초성으로도 사용된다면, 새 초성으로 시작
                        if (!shift && initialMap.ContainsKey(kc))
                        {
                            L_index = initialMap[kc];
                            V_index = -1;
                            T_index = 0;
                            ShowCurrentComposition();
                        }
                        return;
                    }
                }
            }
        }

        // 2) **초성(initial) 처리** (Shift+영문으로 겹자음 처리)
        foreach (var kv in initialMap)
        {
            KeyCode kc = kv.Key;
            int idx = kv.Value; // 초성 인덱스(0~18)

            if (Input.GetKeyDown(kc))
            {
                // (2-1) Shift+R → ㄲ(1)
                if (shift && kc == KeyCode.R)
                {
                    if (L_index != -1 && V_index != -1)
                        CommitCurrentSyllable();
                    L_index = 1;  // ㄲ
                    V_index = -1;
                    T_index = 0;
                    ShowCurrentComposition();
                    return;
                }
                // (2-2) Shift+E → ㄸ(4)
                else if (shift && kc == KeyCode.E)
                {
                    if (L_index != -1 && V_index != -1)
                        CommitCurrentSyllable();
                    L_index = 4;  // ㄸ
                    V_index = -1;
                    T_index = 0;
                    ShowCurrentComposition();
                    return;
                }
                // (2-3) Shift+Q → ㅃ(8)
                else if (shift && kc == KeyCode.Q)
                {
                    if (L_index != -1 && V_index != -1)
                        CommitCurrentSyllable();
                    L_index = 8;  // ㅃ
                    V_index = -1;
                    T_index = 0;
                    ShowCurrentComposition();
                    return;
                }
                // (2-4) Shift+T → ㅆ(10)
                else if (shift && kc == KeyCode.T)
                {
                    if (L_index != -1 && V_index != -1)
                        CommitCurrentSyllable();
                    L_index = 10; // ㅆ
                    V_index = -1;
                    T_index = 0;
                    ShowCurrentComposition();
                    return;
                }
                // (2-5) Shift+W → ㅉ(13)
                else if (shift && kc == KeyCode.W)
                {
                    if (L_index != -1 && V_index != -1)
                        CommitCurrentSyllable();
                    L_index = 13; // ㅉ
                    V_index = -1;
                    T_index = 0;
                    ShowCurrentComposition();
                    return;
                }
                // (2-6) 일반 초성 처리
                else if (!shift)
                {
                    if (L_index != -1 && V_index != -1)
                        CommitCurrentSyllable();

                    L_index = idx;
                    V_index = -1;
                    T_index = 0;
                    ShowCurrentComposition();
                    return;
                }
            }
        }

        // 3) **중성(medial) 처리** (종성이 붙어있을 때는 “종성 → 새로운 초성” 전환 로직 추가)
        foreach (var kv in medialMap)
        {
            KeyCode kc = kv.Key;
            int idx = kv.Value; // 중성 인덱스(0~20)

            if (Input.GetKeyDown(kc))
            {
                // (3-0) “종성만 붙여 놓은 상태에서 중성이 들어오면” → CV로 커밋하고  
                //          그 종성을 새로운 초성으로 재활용하여 중성 붙이기
                if (T_index != 0 && L_index != -1 && V_index != -1)
                {
                    int oldFinal = T_index;

                    // (A) 이전 음절을 “CV” (L_index, V_index, T=0)로 커밋
                    int unicodeCV = ComposeHangul(L_index, V_index, 0);
                    if (unicodeCV != -1)
                    {
                        char sylCV = (char)unicodeCV;
                        _currentText = _currentText.Insert(_caretPosition, sylCV.ToString());
                        _caretPosition++;
                        inputText.text = _currentText;
                        inputText.ForceMeshUpdate();
                    }

                    // (B) 새 음절의 초성을 oldFinal(방금 붙였던 종성)으로 설정
                    L_index = oldFinal;
                    // (C) 새 중성은 지금 들어온 idx
                    V_index = idx;
                    T_index = 0;
                    ShowCurrentComposition();
                    return;
                }

                // (3-1) 그 외: 기본 중성 처리 (초성이 먼저 있어야 함)
                if (L_index == -1)
                    return;

                // (3-2) Shift+O → ㅒ(3)
                if (shift && kc == KeyCode.O)
                {
                    if (V_index == -1)
                    {
                        V_index = 3;  // ㅒ
                        T_index = 0;
                        ShowCurrentComposition();
                        return;
                    }
                    else
                    {
                        int combined = CombineMedial(V_index, 3);
                        if (combined != -1)
                        {
                            V_index = combined;
                            ShowCurrentComposition();
                            return;
                        }
                    }
                }
                // (3-3) Shift+P → ㅖ(7)
                else if (shift && kc == KeyCode.P)
                {
                    if (V_index == -1)
                    {
                        V_index = 7;  // ㅖ
                        T_index = 0;
                        ShowCurrentComposition();
                        return;
                    }
                    else
                    {
                        int combined = CombineMedial(V_index, 7);
                        if (combined != -1)
                        {
                            V_index = combined;
                            ShowCurrentComposition();
                            return;
                        }
                    }
                }
                // (3-4) 일반 중성
                else if (!shift)
                {
                    if (V_index == -1)
                    {
                        V_index = idx;
                        T_index = 0;
                        ShowCurrentComposition();
                        return;
                    }
                    else
                    {
                        int combined = CombineMedial(V_index, idx);
                        if (combined != -1)
                        {
                            V_index = combined;
                            ShowCurrentComposition();
                            return;
                        }
                        else
                        {
                            // 중성 단독 재시작: 기존 음절 커밋 후, 새 음절 중성으로 시작
                            CommitCurrentSyllable();
                            L_index = -1;
                            V_index = idx;
                            T_index = 0;
                            ShowCurrentComposition();
                            return;
                        }
                    }
                }
            }
        }

        // 4) Space 또는 Enter → 조합 확정 + 줄바꿈(Enter)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (L_index != -1 && V_index != -1)
                CommitCurrentSyllable();

            if (Input.GetKeyDown(KeyCode.Return))
            {
                _currentText = _currentText.Insert(_caretPosition, "\n");
                _caretPosition++;
                inputText.text = _currentText;
                inputText.ForceMeshUpdate();
            }
        }
    }


    /// <summary>
    /// 복합 중성(ㅗ+ㅏ=ㅘ 등) 규칙 처리
    /// </summary>
    private int CombineMedial(int oldV, int newV)
    {
        int key = oldV * 100 + newV;
        if (medialCombine.ContainsKey(key))
            return medialCombine[key];
        return -1;
    }

    /// <summary>
    /// 복합 종성(ㄱ+ㅅ=ㄳ 등) 규칙 처리
    /// </summary>
    private int CombineFinal(int oldT, int newT)
    {
        // ㄱ(1)+ㅅ(19)=ㄳ(3)
        if (oldT == 1 && newT == 19) return 3;
        // ㄴ(4)+ㅈ(22)=ㄵ(5), ㄴ(4)+ㅎ(27)=ㄶ(6)
        if (oldT == 4 && newT == 22) return 5;
        if (oldT == 4 && newT == 27) return 6;
        // ㄹ(8)+ㄱ(1)=ㄺ(9), ㄹ(8)+ㅁ(16)=ㄻ(10), ㄹ(8)+ㅂ(17)=ㄼ(11), ㄹ(8)+ㅅ(19)=ㄽ(12)
        // ㄹ(8)+ㅌ(25)=ㄾ(13), ㄹ(8)+ㅍ(26)=ㄿ(14), ㄹ(8)+ㅎ(27)=ㅀ(15)
        if (oldT == 8 && newT == 1) return 9;
        if (oldT == 8 && newT == 16) return 10;
        if (oldT == 8 && newT == 17) return 11;
        if (oldT == 8 && newT == 19) return 12;
        if (oldT == 8 && newT == 25) return 13;
        if (oldT == 8 && newT == 26) return 14;
        if (oldT == 8 && newT == 27) return 15;
        // ㅂ(17)+ㅅ(19)=ㅄ(18)
        if (oldT == 17 && newT == 19) return 18;
        return -1;
    }

    /// <summary>
    /// 현재 L_index, V_index, T_index 상태로 임시로 조합된 음절(또는 자모)만 화면에 보여줍니다.
    /// </summary>
    private void ShowCurrentComposition()
    {
        // (1) 초성만 있는 상태
        if (L_index != -1 && V_index == -1)
        {
            char initialChar = InitialJamoChars[L_index];
            string before = _currentText.Substring(0, _caretPosition);
            string after = _currentText.Substring(_caretPosition);
            inputText.text = before + initialChar + after;
            inputText.ForceMeshUpdate();
            return;
        }

        // (2) 초성+중성(+종성) → 완성형 음절 합성
        if (L_index != -1 && V_index != -1)
        {
            int unicode = ComposeHangul(L_index, V_index, T_index);
            if (unicode != -1)
            {
                char syl = (char)unicode;
                string before = _currentText.Substring(0, _caretPosition);
                string after = _currentText.Substring(_caretPosition);
                inputText.text = before + syl + after;
                inputText.ForceMeshUpdate();
            }
        }
    }

    /// <summary>
    /// 현재 L_index, V_index, T_index로 완성된 음절을 _currentText에 삽입하고, 조합 상태 초기화
    /// </summary>
    private void CommitCurrentSyllable()
    {
        int unicode = ComposeHangul(L_index, V_index, T_index);
        if (unicode != -1)
        {
            char syl = (char)unicode;
            _currentText = _currentText.Insert(_caretPosition, syl.ToString());
            _caretPosition++;
            inputText.text = _currentText;
            inputText.ForceMeshUpdate();
        }
        L_index = -1;
        V_index = -1;
        T_index = 0;
    }

    /// <summary>
    /// L_index, V_index, T_index를 유니코드 완성형 한글 음절로 계산
    /// </summary>
    private int ComposeHangul(int L, int V, int T)
    {
        if (L < 0 || V < 0) return -1;
        return 0xAC00 + ((L * 21) + V) * 28 + T;
    }

    /// <summary>
    /// Backspace가 눌렸을 때, 조합 중이면 자모 단계별 해체, 
    /// 아니면 _currentText의 마지막 문자(음절 또는 영문/특수/숫자)를 삭제
    /// </summary>
    private void HandleBackspace()
    {
        // (A) 조합 중 상태이면 자모 해체
        if (L_index != -1)
        {
            if (T_index != 0)
            {
                // 종성만 제거
                T_index = 0;
                ShowCurrentComposition();
                return;
            }
            else if (V_index != -1)
            {
                // 중성만 제거
                V_index = -1;
                ShowCurrentComposition();
                return;
            }
            else
            {
                // 초성만 있는 상태 → 완전 해제
                L_index = -1;
                inputText.text = _currentText;
                inputText.ForceMeshUpdate();
                return;
            }
        }

        // (B) 조합 중이 아니면 _currentText에서 문자 하나 삭제
        if (_caretPosition == 0) return;
        int prevIndex = _caretPosition - 1;
        char prevChar = _currentText[prevIndex];

        // (B1) 이전 문자가 한글 완성형 음절이면 → 분해 후 내부 상태 복원
        if (IsHangulSyllable(prevChar))
        {
            int code = prevChar - 0xAC00;
            int T = code % 28;
            code /= 28;
            int V = code % 21;
            int L = code / 21;

            // 음절 하나를 지우고
            _currentText = _currentText.Remove(prevIndex, 1);
            _caretPosition--;

            // 분해된 자모를 내부 상태에 저장
            L_index = L;
            V_index = V;
            T_index = T;

            // 화면에 조합 상태 보여줌
            ShowCurrentComposition();
        }
        else
        {
            // (B2) 일반 문자(영문/숫자/특수문자 등)면 그냥 삭제
            _currentText = _currentText.Remove(prevIndex, 1);
            _caretPosition--;
            inputText.text = _currentText;
            inputText.ForceMeshUpdate();
        }
    }

    /// <summary>
    /// 주어진 문자가 한글 완성형 음절(0xAC00~0xD7A3)인지 확인
    /// </summary>
    private bool IsHangulSyllable(char c)
    {
        return c >= 0xAC00 && c <= 0xD7A3;
    }

    #endregion
    // ─────────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────────
    #region 영문 모드 입력 처리

    /// <summary>
    /// 한영 모드가 꺼져(isHangulMode == false) 있을 때 호출됩니다.
    /// Input.inputString 으로 들어오는 문자(영문, 숫자, 특수문자)만 삽입하고,
    /// Backspace/Enter/Tab/화살표키를 처리합니다.
    /// </summary>
    private void HandleEnglishTyping()
    {
        string inputThisFrame = Input.inputString;
        if (!string.IsNullOrEmpty(inputThisFrame))
        {
            foreach (char c in inputThisFrame)
            {
                if (c == '\b')
                {
                    // Backspace: 문자열 마지막 문자 삭제
                    if (_caretPosition > 0)
                    {
                        _currentText = _currentText.Remove(_caretPosition - 1, 1);
                        _caretPosition--;
                        inputText.text = _currentText;
                        inputText.ForceMeshUpdate();
                    }
                }
                else if (c == '\n' || c == '\r')
                {
                    // Enter: 줄바꿈
                    _currentText = _currentText.Insert(_caretPosition, "\n");
                    _caretPosition++;
                    inputText.text = _currentText;
                    inputText.ForceMeshUpdate();
                }
                else
                {
                    // 일반 문자 삽입
                    _currentText = _currentText.Insert(_caretPosition, c.ToString());
                    _caretPosition++;
                    inputText.text = _currentText;
                    inputText.ForceMeshUpdate();
                }
            }
        }

        // Tab: 들여쓰기
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            string spaces = new string(' ', tabSize);
            _currentText = _currentText.Insert(_caretPosition, spaces);
            _caretPosition += tabSize;
            inputText.text = _currentText;
            inputText.ForceMeshUpdate();
        }

        // 방향키: 커서 이동
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (_caretPosition > 0) _caretPosition--;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (_caretPosition < _currentText.Length) _caretPosition++;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveCaretVertically(true);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveCaretVertically(false);
        }
    }

    #endregion
    // ─────────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────────
    #region 커서 위치 갱신 (World Space)

    /// <summary>
    /// 현재 _caretPosition 인덱스 기준으로 “문자 오른쪽” 로컬 좌표를 구한 뒤,
    /// inputText.rectTransform.TransformPoint()로 월드 좌표로 바꿔 _caretRect.position에 할당합니다.
    /// </summary>
    private void UpdateCaretPosition()
    {
        inputText.ForceMeshUpdate();
        TMP_TextInfo textInfo = inputText.textInfo;

        Vector2 localPos;
        if (textInfo.characterCount == 0 || _caretPosition == 0)
        {
            if (textInfo.lineCount > 0)
            {
                TMP_LineInfo firstLine = textInfo.lineInfo[0];
                localPos = new Vector2(firstLine.lineExtents.min.x, firstLine.ascender);
            }
            else
            {
                localPos = Vector2.zero;
            }
        }
        else
        {
            int charIdx = Mathf.Clamp(_caretPosition - 1, 0, textInfo.characterCount - 1);
            TMP_CharacterInfo cInfo = textInfo.characterInfo[charIdx];
            localPos = cInfo.topRight;
        }

        Vector3 worldPos = inputText.rectTransform.TransformPoint(localPos);
        _caretRect.position = worldPos;
    }

    /// <summary>
    /// 화살표키 ↑/↓ 를 눌러 멀티라인에서 커서를 이동시킵니다.
    /// </summary>
    private void MoveCaretVertically(bool upward)
    {
        inputText.ForceMeshUpdate();
        TMP_TextInfo textInfo = inputText.textInfo;
        if (textInfo.lineCount == 0) return;

        int charIdx = Mathf.Clamp(_caretPosition - 1, 0, textInfo.characterCount - 1);
        TMP_CharacterInfo charInfo = textInfo.characterInfo[charIdx];
        int currentLine = charInfo.lineNumber;

        int targetLine = upward ? (currentLine - 1) : (currentLine + 1);
        if (targetLine < 0 || targetLine >= textInfo.lineCount) return;

        Vector2 caretLocalPos = GetCaretPositionLocal(_caretPosition);
        float targetX = caretLocalPos.x;

        TMP_LineInfo lineInfo = textInfo.lineInfo[targetLine];
        int start = lineInfo.firstCharacterIndex;
        int end = lineInfo.lastCharacterIndex;

        int best = start;
        float minDiff = Mathf.Abs(GetCaretPositionLocal(start).x - targetX);
        for (int i = start + 1; i <= end; i++)
        {
            float xPos = GetCaretPositionLocal(i).x;
            float diff = Mathf.Abs(xPos - targetX);
            if (diff < minDiff)
            {
                minDiff = diff;
                best = i;
            }
        }

        _caretPosition = best;
    }

    /// <summary>
    /// 주어진 문자열 인덱스(caretIndex)의 “문자 오른쪽” 로컬 좌표를 반환합니다.
    /// </summary>
    private Vector2 GetCaretPositionLocal(int caretIndex)
    {
        inputText.ForceMeshUpdate();
        TMP_TextInfo textInfo = inputText.textInfo;

        if (textInfo.characterCount == 0 || caretIndex == 0)
        {
            if (textInfo.lineCount > 0)
            {
                TMP_LineInfo firstLine = textInfo.lineInfo[0];
                return new Vector2(firstLine.lineExtents.min.x, firstLine.ascender);
            }
            else
            {
                return Vector2.zero;
            }
        }

        int idx = Mathf.Clamp(caretIndex - 1, 0, textInfo.characterCount - 1);
        TMP_CharacterInfo cInfo = textInfo.characterInfo[idx];
        return cInfo.topRight;
    }

    #endregion
    // ─────────────────────────────────────────────────────────────────
}