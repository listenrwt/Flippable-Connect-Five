using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameEventsSO", menuName = "ScriptableObject/GameEventsSO")]
public class GameEventsSO : ScriptableObject
{
    #region Event Args
    public class GameEventArgs : EventArgs
    {
        public EnumsBase.gameEventType eventType {  get; set; }
        public int column { get; set; }
        public EnumsBase.rotateDir dir { get; set; }
    }
    #endregion

    public delegate void GameActionHandler(GameEventArgs e);
    private bool gameActionStarted = false;
    private bool tokenDropped = false;
    private bool boardRotated = false;

    #region Game Action Start Event
    public event GameActionHandler OnGameActionStarted;

    public void GameActionStart(GameEventArgs e)
    {
        if (!gameActionStarted && !tokenDropped && e.eventType == EnumsBase.gameEventType.TokenDrop)
        {
            gameActionStarted = true;
            tokenDropped = true;
            TokenDrop(e.column);
        }else if (!gameActionStarted && !boardRotated && e.eventType == EnumsBase.gameEventType.BoardRotate)
        {
            gameActionStarted = true;
            boardRotated = true;
            BoardRotate(e.dir);
        }


    } 
    private void TokenDrop(int _column)
    {
        OnGameActionStarted?.Invoke(new GameEventArgs
        {
            eventType = EnumsBase.gameEventType.TokenDrop,
            column = _column
        });
    }

    private void BoardRotate(EnumsBase.rotateDir _dir)
    {
        OnGameActionStarted?.Invoke(new GameEventArgs
        {
            eventType = EnumsBase.gameEventType.BoardRotate,
            dir = _dir
        });
    }
    #endregion

    #region Game Action End Event
    public event GameActionHandler OnGameActionEnded;
    
    public void GameActionEnded(GameEventArgs e) => OnGameActionEnded?.Invoke(e);

    private void NextTurn(GameEventArgs e)
    {
        if (e.eventType != EnumsBase.gameEventType.@Null) return;

        gameActionStarted = false;
        if(tokenDropped && boardRotated)
        {
            tokenDropped = false;
            boardRotated = false;
            GameActionEnded(new GameEventArgs { eventType = EnumsBase.gameEventType.SwapTurn });
        }
    }

    private void IgnoreMove(GameEventArgs e)
    {
        if (e.eventType == EnumsBase.gameEventType.InvalidMove)
        {
            gameActionStarted = false;
            tokenDropped = false;
        }
    }
    #endregion

    #region Token Pool Event
    public delegate GameObject ObjectPoolOuted(EnumsBase.pool key, bool active = true);
    public event ObjectPoolOuted OnObjectPoolOuted;
    public GameObject ObjectPoolOut(EnumsBase.pool key, bool active = true)
    {
        return OnObjectPoolOuted?.Invoke(key, active);
    }

    public delegate void ObjectPoolIned(ref GameObject targetObject, EnumsBase.pool key);
    public event ObjectPoolIned OnObjectPoolIned;
    public void ObjectPoolIn(ref GameObject targetObject, EnumsBase.pool key)
    {
        OnObjectPoolIned?.Invoke(ref targetObject, key);
    }

    #endregion

    #region Win Check Event
    public delegate EnumsBase.state WinChecked();
    public event WinChecked OnWinChecked;
    public EnumsBase.state WinCheck()
    {
        if (OnWinChecked != null)
            return OnWinChecked();
        else
            return EnumsBase.state.empty;
    }
    #endregion

    private void OnEnable()
    {
        #region initiation
        gameActionStarted = false;
        tokenDropped = false;
        boardRotated = false;
        #endregion

        #region Event Subscriptabtion 
        OnGameActionEnded += NextTurn;
        OnGameActionEnded += IgnoreMove;
        #endregion
    }

    private void OnDisable()
    {
        #region Event Unsubscriptabtion 
        OnGameActionEnded -= NextTurn;
        OnGameActionEnded -= IgnoreMove;
        #endregion
    }
}
