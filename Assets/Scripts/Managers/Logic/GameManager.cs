using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Scriptable Objects")]
    [SerializeField] private GameEventsSO gameEvents;
    [SerializeField] private AnimationEventsSO aniEvents;
    [SerializeField] private GameRulesSO gameRules;

    [Header("Instances")]
    [SerializeField] private TextMeshProUGUI turn;
    private void ChangeTurnText(EnumsBase.state state, string content)
    {
        switch (state)
        {
            case EnumsBase.state.red:
                turn.text = "Red" + content;
                turn.color = Color.red;
                break;
            case EnumsBase.state.yellow:
                turn.text = "Yellow" + content;
                turn.color = Color.yellow;
                break;
            default:
                break;
        }
    }

    private bool animationEnded = true;

    private void Start()
    {
        gameEvents.OnGameActionStarted += WaitAudioVisualsEnd;
        aniEvents.OnAnimationEnded += AnimationEnd;

        ChangeTurnText(gameRules.currentPlayer, "'s Turn");
    }

    #region Wait Animation and Audio Ends
    private void WaitAudioVisualsEnd(GameEventsSO.GameEventArgs e = null) 
        => animationEnded = false;

    private void AnimationEnd() 
    {
        animationEnded = true;
        EndState();
    }

    private void EndState()
    {
        if (!animationEnded)
            return;

        EnumsBase.state winPlayer = gameEvents.WinCheck();
        if(winPlayer != EnumsBase.state.empty)
        {
            ChangeTurnText(winPlayer, " Wins");
        }
        else
        {
            gameEvents.GameActionEnded(new GameEventsSO.GameEventArgs { 
                eventType = EnumsBase.gameEventType.Null
            });
            ChangeTurnText(gameRules.currentPlayer, "'s Turn");
        }

    }
    #endregion

    private void OnDisable()
    {
        gameEvents.OnGameActionStarted -= WaitAudioVisualsEnd;
        aniEvents.OnAnimationEnded -= AnimationEnd;
    }
}
