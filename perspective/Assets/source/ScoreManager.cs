using UnityEngine;
using System.Collections;

public class ScoreManager : MonoBehaviour 
{
  public TextMesh playerAScoreText;
  public TextMesh playerBScoreText;

  private int _playerAScore;
  private int _playerBScore;

  public void AdjustPlayerScore(TileTypes playerType, int scoreAdjustment)
  {
    if (playerType == TileTypes.TypeA)
    {
      _playerAScore += scoreAdjustment;
      playerAScoreText.text = _playerAScore.ToString();
    }
    else if (playerType == TileTypes.TypeB)
    {
      _playerBScore += scoreAdjustment;
      playerBScoreText.text = _playerBScore.ToString();
    }
  }
}
