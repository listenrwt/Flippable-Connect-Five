using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameRulesSO", menuName = "ScriptableObject/GameRulesSO")]
public class GameRulesSO : ScriptableObject
{
    [Header("Scriptable Object")]
    [SerializeField] private GameEventsSO gameEvents;

    [Header("Rules Settings")]
    [SerializeField] private int _columnNo = 6;
    public int columnNo { get { return _columnNo; }}

    [SerializeField] private int _rowNo = 6;
    public int rowNo { get { return _rowNo; }}

    [SerializeField] private EnumsBase.state _firstPlayer;

    [SerializeField] private int _minConnectionNo = 4;
    public int minConnectionNo { get { return _minConnectionNo; } }

    private EnumsBase.state _currentPlayer;
    public EnumsBase.state currentPlayer { get { return _currentPlayer; } }

    #region Board Info
    public class Cell
    {
        public GameObject token = null;
        public EnumsBase.state state = EnumsBase.state.empty;
        public Vector2 localPos = Vector2.zero;

        public static implicit operator Cell(Grid v)
        {
            throw new NotImplementedException();
        }
    }
    public Cell[,] board;
    #endregion

    private void SwapTurn(GameEventsSO.GameEventArgs e)
    {
        if (e.eventType != EnumsBase.gameEventType.SwapTurn) return;

        _currentPlayer = currentPlayer == EnumsBase.state.red ? 
            EnumsBase.state.yellow : EnumsBase.state.red;
    }

    private void OnEnable()
    {
        _currentPlayer = _firstPlayer;

        gameEvents.OnGameActionEnded += SwapTurn;

        #region Initiation
        board = new Cell[columnNo, rowNo];
        for (int i = 0; i < columnNo; i++)
        {
            for (int j = 0; j < rowNo; j++)
                board[i, j] = new Cell();
        }
        #endregion
    }

    private void OnDisable()
    {
        gameEvents.OnGameActionEnded -= SwapTurn;
    }

}
