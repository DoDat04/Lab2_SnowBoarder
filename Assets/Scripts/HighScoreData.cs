using System.Collections.Generic;
using System;

[System.Serializable]
public class HighScoreData
{
    public int highScore;

}

[System.Serializable]
public class HighScoreEntry
{
    public string playerName;
    public int score;
}

[System.Serializable]
public class HighScoreList
{
    public List<HighScoreEntry> highScores = new List<HighScoreEntry>();
}