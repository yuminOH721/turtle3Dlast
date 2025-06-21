using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AnswerStage
{
    public int id;
    public Color expectedColor;
    public Vector3[] positions;
    public string requiredKeyword;
}

[Serializable]
public class AnswerSet
{
    public List<AnswerStage> stages = new List<AnswerStage>();
}