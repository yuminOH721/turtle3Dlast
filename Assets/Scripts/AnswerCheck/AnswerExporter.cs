// File: AnswerExporter.cs
using System.IO;
using UnityEngine;

public static class AnswerExporter
{
    // Assets/Resources/answers.json
    static string FilePath =>
        Path.Combine(Application.dataPath, "Resources/answers.json");

    /// <summary>
    /// 외부에서 모아온 positions 배열을 그대로 저장합니다.
    /// </summary>
    public static void ExportStage(int stageId, Vector3[] positions, Color expectedColor)
    {
        // 1) JSON 읽기
        AnswerSet set;
        if (File.Exists(FilePath))
        {
            string txt = File.ReadAllText(FilePath);
            set = JsonUtility.FromJson<AnswerSet>(txt) ?? new AnswerSet();
        }
        else
        {
            set = new AnswerSet();
        }

        // 2) 같은 id가 있으면 덮어쓰기, 없으면 새로 추가
        int existingIndex = set.stages.FindIndex(s => s.id == stageId);
        var newStage = new AnswerStage { id = stageId, expectedColor = expectedColor, positions = positions };
        if (existingIndex >= 0)
            set.stages[existingIndex] = newStage;   // 이미 있던 건 덮어쓰기
        else
            set.stages.Add(newStage);               // 없던 건 새로 추가

        // 3) JSON 쓰기
        string outJson = JsonUtility.ToJson(set, prettyPrint: true);
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
        File.WriteAllText(FilePath, outJson);

        Debug.Log($"[AnswerExporter] Stage {stageId} saved with {positions.Length} points.");
    }

}