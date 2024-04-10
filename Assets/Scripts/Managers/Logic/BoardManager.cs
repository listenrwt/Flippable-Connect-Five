using UnityEngine;
using static GameRulesSO;

public class BoardManager : MonoBehaviour
{
    #region Scriptable Objects
    [Header("Scriptable Objects")]
    [SerializeField] private GameRulesSO gameRules;
    [SerializeField] private GameEventsSO gameEvents;
    [SerializeField] private AnimationEventsSO aniEvents;
    #endregion

    private void Start()
    {
        #region Event Subscription
        gameEvents.OnGameActionStarted += DropToken;
        gameEvents.OnGameActionStarted += RotateBoard;
        gameEvents.OnWinChecked += GetWinPlayer;
        #endregion
    }

    #region Drop a Token
    private void DropToken(GameEventsSO.GameEventArgs e)
    {
        if (e.eventType == EnumsBase.gameEventType.TokenDrop)
            DropToken(e.column);
    }
    private void DropToken(int column)
    {
        //Check if the column is full
        if (gameRules.board[column, gameRules.rowNo - 1].state != EnumsBase.state.empty)
        {
            Debug.Log("This Column is Full.");
            gameEvents.GameActionEnded(new GameEventsSO.GameEventArgs
            {
                eventType = EnumsBase.gameEventType.InvalidMove,
            });
            return;
        }

        #region Place Token 
        #region Get Token Pos
        int row = 0;
        for (row = gameRules.rowNo - 1; row >= 0; row--)
        {
            if (gameRules.board[column, row].state != EnumsBase.state.empty)
                break;
        }
        #endregion
        EnumsBase.pool key = gameRules.currentPlayer == EnumsBase.state.red ? EnumsBase.pool.redToken : EnumsBase.pool.yellowToken;
        GameObject token = gameEvents.ObjectPoolOut(key);
        aniEvents.AnimationStart(ref token, column, row + 1);
        gameRules.board[column, row + 1].state = gameRules.currentPlayer;
        gameRules.board[column, row + 1].token = token;
        #endregion

    }

   
    #endregion
    

    #region Rotate the Board
    private void RotateBoard(GameEventsSO.GameEventArgs e)
    {
        if (e.eventType == EnumsBase.gameEventType.BoardRotate)
            RotateBoard(e.dir);
    }
    private void RotateBoard(EnumsBase.rotateDir dir)
    {
        #region Array Rotation
        switch (dir)
        {
            case EnumsBase.rotateDir.anticlockwise:
                gameRules.board = gameRules.board.LeftRotate();
                break;
            case EnumsBase.rotateDir.clockwise:
                gameRules.board = gameRules.board.RightRotate();
                break;
            case EnumsBase.rotateDir.upsidedown:
                gameRules.board = gameRules.board.FlipUpsideDown();
                break;
            case EnumsBase.rotateDir.@null:
                aniEvents.AnimationEnd();
                return;
            default:
                break;
        }
        #endregion

        aniEvents.AnimationStart(dir);
        FallToken();
    }
    #endregion

    #region Tokens Fall Control
        private void FallToken()
        {
            for (int column = 0; column < gameRules.columnNo; column++)
            {
                for (int row = 1; row < gameRules.rowNo; row++)
                {
                    if (gameRules.board[column, row].state != EnumsBase.state.empty)
                    {
                        GameObject token = gameRules.board[column, row].token;
                        Fall(gameRules.board[column, row], column, row, out int row_end);
                        aniEvents.AnimationStart(ref token, column, row, row_end);
                    }
                }
            }
        }
       
        private void Fall(Cell token, int column, int row, out int row_end)
        {
            row_end = row;
            if (row - 1 < 0)
                return;

            if (gameRules.board[column, row - 1].state == EnumsBase.state.empty)
            {
                gameRules.board[column, row - 1].state = token.state;
                gameRules.board[column, row - 1].token = token.token;

                gameRules.board[column, row].state = EnumsBase.state.empty;
                gameRules.board[column, row].token = null;

                Fall(gameRules.board[column, row - 1], column, row - 1, out row_end);
            }
            else
                return;
        }

        #endregion

    #region Win Condition Checking 
    private EnumsBase.state GetWinPlayer()
        {
            int maxConnect_Red = 0;
            int maxConnect_Yellow = 0;
            int temp_Red = 0, temp_Yellow = 0;

            #region Check Horizontal
            for (int j = 0; j < gameRules.rowNo; j++)
            {
                temp_Red = temp_Yellow = 0;
                for (int i = 0; i < gameRules.columnNo; i++)
                {
                    if (gameRules.board[i, j].state == EnumsBase.state.red)
                    {
                        temp_Red++;
                        maxConnect_Yellow = Mathf.Max(temp_Yellow, maxConnect_Yellow);
                        temp_Yellow = 0;
                    } else if (gameRules.board[i, j].state == EnumsBase.state.yellow)
                    {
                        temp_Yellow++;
                        maxConnect_Red = Mathf.Max(temp_Red, maxConnect_Red);
                        temp_Red = 0;
                    } else
                    {
                        maxConnect_Red = Mathf.Max(temp_Red, maxConnect_Red);
                        maxConnect_Yellow = Mathf.Max(temp_Yellow, maxConnect_Yellow);
                        temp_Red = temp_Yellow = 0;
                    }
                }
                maxConnect_Red = Mathf.Max(temp_Red, maxConnect_Red);
                maxConnect_Yellow = Mathf.Max(temp_Yellow, maxConnect_Yellow);
            }
            #endregion

            #region Check Vertical
            for (int i = 0; i < gameRules.columnNo; i++)
            {
                temp_Red = temp_Yellow = 0;
                for (int j = 0; j < gameRules.rowNo; j++)
                {
                    if (gameRules.board[i, j].state == EnumsBase.state.red)
                    {
                        temp_Red++;
                        maxConnect_Yellow = Mathf.Max(temp_Yellow, maxConnect_Yellow);
                        temp_Yellow = 0;
                    }
                    else if (gameRules.board[i, j].state == EnumsBase.state.yellow)
                    {
                        temp_Yellow++;
                        maxConnect_Red = Mathf.Max(temp_Red, maxConnect_Red);
                        temp_Red = 0;
                    }
                    else
                        break;
                }
                maxConnect_Red = Mathf.Max(temp_Red, maxConnect_Red);
                maxConnect_Yellow = Mathf.Max(temp_Yellow, maxConnect_Yellow);
            }
            #endregion

            #region Check Diagonal
            #region Buttom Left to Top Right
            for (int j = 0; j < gameRules.rowNo; j++)
            {
                for (int i = 0; i < gameRules.columnNo; i++)
                {
                    temp_Red = temp_Yellow = 0;
                    int row = j, col = i;
                    while (row < gameRules.rowNo && col < gameRules.columnNo)
                    {
                        if (gameRules.board[row, col].state == EnumsBase.state.red)
                        {
                            temp_Red++;
                            maxConnect_Yellow = Mathf.Max(temp_Yellow, maxConnect_Yellow);
                            temp_Yellow = 0;
                        }
                        else if (gameRules.board[row, col].state == EnumsBase.state.yellow)
                        {
                            temp_Yellow++;
                            maxConnect_Red = Mathf.Max(temp_Red, maxConnect_Red);
                            temp_Red = 0;
                        }
                        else
                        {
                            maxConnect_Red = Mathf.Max(temp_Red, maxConnect_Red);
                            maxConnect_Yellow = Mathf.Max(temp_Yellow, maxConnect_Yellow);
                            temp_Red = temp_Yellow = 0;
                        }
                        row++;
                        col++;
                    }
                    maxConnect_Red = Mathf.Max(temp_Red, maxConnect_Red);
                    maxConnect_Yellow = Mathf.Max(temp_Yellow, maxConnect_Yellow);
                }
            }
            #endregion

            #region Buttom Right to Top Left
            for (int j = 0; j < gameRules.rowNo; j++)
            {
                for (int i = 0; i < gameRules.columnNo; i++)
                {
                    temp_Red = temp_Yellow = 0;
                    int row = j, col = i;
                    while (row < gameRules.rowNo && col >= 0)
                    {
                        if (gameRules.board[row, col].state == EnumsBase.state.red)
                        {
                            temp_Red++;
                            maxConnect_Yellow = Mathf.Max(temp_Yellow, maxConnect_Yellow);
                            temp_Yellow = 0;
                        }
                        else if (gameRules.board[row, col].state == EnumsBase.state.yellow)
                        {
                            temp_Yellow++;
                            maxConnect_Red = Mathf.Max(temp_Red, maxConnect_Red);
                            temp_Red = 0;
                        }
                        else
                        {
                            maxConnect_Red = Mathf.Max(temp_Red, maxConnect_Red);
                            maxConnect_Yellow = Mathf.Max(temp_Yellow, maxConnect_Yellow);
                            temp_Red = temp_Yellow = 0;
                        }
                        row++;
                        col--;
                    }
                    maxConnect_Red = Mathf.Max(temp_Red, maxConnect_Red);
                    maxConnect_Yellow = Mathf.Max(temp_Yellow, maxConnect_Yellow);
                }
            }
            #endregion
            #endregion

            #region Return
            if (maxConnect_Red > maxConnect_Yellow)
            {
                return maxConnect_Red >= gameRules.minConnectionNo ? EnumsBase.state.red : EnumsBase.state.empty;
            }
            else if (maxConnect_Red < maxConnect_Yellow)
            {
                return maxConnect_Yellow >= gameRules.minConnectionNo ? EnumsBase.state.yellow : EnumsBase.state.empty;
            }
            else
                return maxConnect_Red >= gameRules.minConnectionNo ? gameRules.currentPlayer : EnumsBase.state.empty;
            #endregion
        }
    public EnumsBase.state GetWinPlayer(ref Cell[,] board, EnumsBase.state currentPlayer)
    {
        int maxConnect_Red = 0;
        int maxConnect_Yellow = 0;
        int temp_Red = 0, temp_Yellow = 0;

        #region Check Horizontal
        for (int j = 0; j < gameRules.rowNo; j++)
        {
            temp_Red = temp_Yellow = 0;
            for (int i = 0; i < gameRules.columnNo; i++)
            {
                if (board[i, j].state == EnumsBase.state.red)
                {
                    temp_Red++;
                    maxConnect_Yellow = Mathf.Max(temp_Yellow, maxConnect_Yellow);
                    temp_Yellow = 0;
                }
                else if (board[i, j].state == EnumsBase.state.yellow)
                {
                    temp_Yellow++;
                    maxConnect_Red = Mathf.Max(temp_Red, maxConnect_Red);
                    temp_Red = 0;
                }
                else
                {
                    maxConnect_Red = Mathf.Max(temp_Red, maxConnect_Red);
                    maxConnect_Yellow = Mathf.Max(temp_Yellow, maxConnect_Yellow);
                    temp_Red = temp_Yellow = 0;
                }
            }
            maxConnect_Red = Mathf.Max(temp_Red, maxConnect_Red);
            maxConnect_Yellow = Mathf.Max(temp_Yellow, maxConnect_Yellow);
        }
        #endregion

        #region Check Vertical
        for (int i = 0; i < gameRules.columnNo; i++)
        {
            temp_Red = temp_Yellow = 0;
            for (int j = 0; j < gameRules.rowNo; j++)
            {
                if (board[i, j].state == EnumsBase.state.red)
                {
                    temp_Red++;
                    maxConnect_Yellow = Mathf.Max(temp_Yellow, maxConnect_Yellow);
                    temp_Yellow = 0;
                }
                else if (board[i, j].state == EnumsBase.state.yellow)
                {
                    temp_Yellow++;
                    maxConnect_Red = Mathf.Max(temp_Red, maxConnect_Red);
                    temp_Red = 0;
                }
                else
                    break;
            }
            maxConnect_Red = Mathf.Max(temp_Red, maxConnect_Red);
            maxConnect_Yellow = Mathf.Max(temp_Yellow, maxConnect_Yellow);
        }
        #endregion

        #region Check Diagonal
        #region Buttom Left to Top Right
        for (int j = 0; j < gameRules.rowNo; j++)
        {
            for (int i = 0; i < gameRules.columnNo; i++)
            {
                temp_Red = temp_Yellow = 0;
                int row = j, col = i;
                while (row < gameRules.rowNo && col < gameRules.columnNo)
                {
                    if (board[row, col].state == EnumsBase.state.red)
                    {
                        temp_Red++;
                        maxConnect_Yellow = Mathf.Max(temp_Yellow, maxConnect_Yellow);
                        temp_Yellow = 0;
                    }
                    else if (board[row, col].state == EnumsBase.state.yellow)
                    {
                        temp_Yellow++;
                        maxConnect_Red = Mathf.Max(temp_Red, maxConnect_Red);
                        temp_Red = 0;
                    }
                    else
                    {
                        maxConnect_Red = Mathf.Max(temp_Red, maxConnect_Red);
                        maxConnect_Yellow = Mathf.Max(temp_Yellow, maxConnect_Yellow);
                        temp_Red = temp_Yellow = 0;
                    }
                    row++;
                    col++;
                }
                maxConnect_Red = Mathf.Max(temp_Red, maxConnect_Red);
                maxConnect_Yellow = Mathf.Max(temp_Yellow, maxConnect_Yellow);
            }
        }
        #endregion

        #region Buttom Right to Top Left
        for (int j = 0; j < gameRules.rowNo; j++)
        {
            for (int i = 0; i < gameRules.columnNo; i++)
            {
                temp_Red = temp_Yellow = 0;
                int row = j, col = i;
                while (row < gameRules.rowNo && col >= 0)
                {
                    if (board[row, col].state == EnumsBase.state.red)
                    {
                        temp_Red++;
                        maxConnect_Yellow = Mathf.Max(temp_Yellow, maxConnect_Yellow);
                        temp_Yellow = 0;
                    }
                    else if (board[row, col].state == EnumsBase.state.yellow)
                    {
                        temp_Yellow++;
                        maxConnect_Red = Mathf.Max(temp_Red, maxConnect_Red);
                        temp_Red = 0;
                    }
                    else
                    {
                        maxConnect_Red = Mathf.Max(temp_Red, maxConnect_Red);
                        maxConnect_Yellow = Mathf.Max(temp_Yellow, maxConnect_Yellow);
                        temp_Red = temp_Yellow = 0;
                    }
                    row++;
                    col--;
                }
                maxConnect_Red = Mathf.Max(temp_Red, maxConnect_Red);
                maxConnect_Yellow = Mathf.Max(temp_Yellow, maxConnect_Yellow);
            }
        }
        #endregion
        #endregion

        #region Return
        if (maxConnect_Red > maxConnect_Yellow)
        {
            return maxConnect_Red >= gameRules.minConnectionNo ? EnumsBase.state.red : EnumsBase.state.empty;
        }
        else if (maxConnect_Red < maxConnect_Yellow)
        {
            return maxConnect_Yellow >= gameRules.minConnectionNo ? EnumsBase.state.yellow : EnumsBase.state.empty;
        }
        else
            return maxConnect_Red >= gameRules.minConnectionNo ? currentPlayer : EnumsBase.state.empty;
        #endregion
    }
    #endregion



    #region Event Unsubscription
    private void OnDisable()
        {
            gameEvents.OnGameActionStarted -= DropToken;
            gameEvents.OnGameActionStarted -= RotateBoard;
            gameEvents.OnWinChecked -= GetWinPlayer;
        }
        #endregion
}
