using UnityEngine;

public static class AnswerLoader
{
    const string RESOURCE_PATH = "answers";  // Resources/answers.json

    public static AnswerSet LoadAll()
    {
        var ta = Resources.Load<TextAsset>(RESOURCE_PATH);
        if (ta == null)
            return new AnswerSet();
        return JsonUtility.FromJson<AnswerSet>(ta.text) ?? new AnswerSet();
    }

    public static AnswerStage GetStage(int id)
    {
        return LoadAll().stages.Find(s => s.id == id);
    }
}