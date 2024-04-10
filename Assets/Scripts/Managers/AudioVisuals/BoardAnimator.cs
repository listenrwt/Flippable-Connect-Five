using DG.Tweening;
using UnityEngine;

public class BoardAnimator : MonoBehaviour
{
    #region Scriptable Objects
    [Header("Scriptable Onjects")]
    [SerializeField] private AnimationEventsSO aniEvents;
    [SerializeField] private GameRulesSO gameRules;
    [SerializeField] private AudioEventsSO audioEvents;
    #endregion

    #region GameObjects/Prefabs
    private GameObject board;
    #endregion

    #region Animation Settings
    [SerializeField] private float tokenDropSpeed = 0.2f; //Drop Time per Grid
    [SerializeField] private float boardRotateTime = 1f;
    #endregion

    #region Board Settings
    [Header("Board Settings")]
    [SerializeField] private Transform zeroPt;

    [SerializeField] private Vector2 tokenOffset;
    private bool swap90deg = false;
    private Vector3 dropOffset
    {
        get
        {
            return swap90deg ? new Vector3(0f, tokenOffset.x, 0f) : new Vector3(0f, tokenOffset.y, 0f);
        }
    }
    #endregion

    private void Start()
    {
        board = GameObject.FindGameObjectWithTag("Board"); //Get Board Instance

        #region initiate token position
        for (int i = 0; i < gameRules.columnNo; i++)
        {
            for (int j = 0; j < gameRules.rowNo; j++)
            {
                gameRules.board[i, j].localPos = 
                    zeroPt.localPosition + new Vector3(tokenOffset.x * i, tokenOffset.y * j, 0f);
            }
        }
        #endregion

        aniEvents.OnAnimationStarted += AnimationStart; //Subscribe animation event

    }

    private void AnimationStart(ref GameObject token, AnimationEventsSO.AnimationEventArgs e)
    {
        switch (e.eventType)
        {
            case EnumsBase.animationEventType.TokenDrop:
                AnimateToken(ref token, e.column, e.row);
                break;
            case EnumsBase.animationEventType.TokenFall:
                AnimateToken(ref token, e.column, e.row_start, e.row_end);
                break;
            case EnumsBase.animationEventType.BoardRotate:
                AnimateBoard(e.dir);
                break;
            default:
                Debug.LogError("No such animation type!");
                break;
        }
        DOTweenNum++;
    }

    private int DOTweenNum = 0;
    private void DOTweenFinished()
    {
        DOTweenNum--;
        if(DOTweenNum == 0) aniEvents.AnimationEnd();
    }

    #region Token Animation
    private void AnimateToken(ref GameObject token, int column, int row)
    {
        token.transform.parent = board.transform;
        token.transform.localScale = board.transform.localScale;
        token.transform.localPosition = gameRules.board[column, gameRules.rowNo - 1].localPos;
        token.transform.position += dropOffset;
        token.transform.rotation = Quaternion.identity;

        int gridHeight = gameRules.rowNo - row;
        token.transform.DOLocalMove(gameRules.board[column, row].localPos, tokenDropSpeed * gridHeight)
            .SetEase(Ease.OutBounce).OnComplete(DOTweenFinished);

        audioEvents.AudioSourcePlay(ref token, new AudioEventsSO.AudioEventsArgs
        {
            audio = EnumsBase.audio.TokenDrop,
            durationPercent = (float)(gridHeight * 2) / gameRules.rowNo
        });
    }
    #endregion

    #region Board Animation
    private void AnimateToken(ref GameObject token, int column, int row_start, int row_end)
    {
        int gridHeight = row_start - row_end;
        token.transform.DOLocalMove(gameRules.board[column, row_end].localPos, tokenDropSpeed * gridHeight)
            .SetEase(Ease.OutBounce).OnComplete(DOTweenFinished);

        audioEvents.AudioSourcePlay(ref token, new AudioEventsSO.AudioEventsArgs
        {
            audio = EnumsBase.audio.TokenDrop,
            durationPercent = (float)(gridHeight * 2) / gameRules.rowNo
        });
    }

    private void AnimateBoard(EnumsBase.rotateDir dir)
    {
        Vector3 rotateDir = Vector3.zero;
        switch (dir)
        {
            case EnumsBase.rotateDir.clockwise:
                rotateDir = new Vector3(0f, 0f, -90f);
                swap90deg = !swap90deg;
                break;
            case EnumsBase.rotateDir.anticlockwise:
                rotateDir = new Vector3(0f, 0f, 90f);
                swap90deg = !swap90deg;
                break;
            case EnumsBase.rotateDir.upsidedown:
                rotateDir = new Vector3(180f, 0f, 0f);
                break;
            default:
                return;
        }

        board.transform.DOBlendableRotateBy(rotateDir, boardRotateTime, RotateMode.WorldAxisAdd)
            .OnComplete(DOTweenFinished);

    }
    #endregion

    private void OnDisable()
    {
        aniEvents.OnAnimationStarted -= AnimationStart;
    }

}
