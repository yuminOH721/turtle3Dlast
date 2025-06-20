using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AnswerStage
{
    public int id;
    public Color expectedColor;
    public Vector3[] positions;
}

[Serializable]
public class AnswerSet
{
    public List<AnswerStage> stages = new List<AnswerStage>();
}