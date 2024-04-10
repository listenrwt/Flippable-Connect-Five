using System.Collections.Generic;
using UnityEngine;
using static GameRulesSO;
using static EnumsBase;
using System.Linq;
using UnityEngine.SocialPlatforms.Impl;

public class AIManager : MonoBehaviour
{
    [SerializeField] private int maxDepth = 8;
    [SerializeField] private state ai;
    private state human
    {
        get { return ai == state.red ? state.yellow : state.red; }
    }

    [SerializeField]private BoardManager boardManager;
    [SerializeField]private GameRulesSO gameRules;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.W))
        {
            Node node = FindBestMove(ref gameRules.board, false, false);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            Node node = FindBestMove(ref gameRules.board, false, true);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Node node = FindBestMove(ref gameRules.board, true, false);
        }
    }

    private class Node
    {
        public Node(int column, rotateDir rotateDir, bool dropped, bool rotated)
        {
            this.column = column;
            this.rotateDir = rotateDir;
            this.dropped = dropped;
            this.rotated = rotated;
        }

        public int column;
        public rotateDir rotateDir;
        public bool dropped, rotated;
    }

    private Node FindBestMove(ref Cell[,] board, bool dropped, bool rotated)
    {
        state currentPlayer = ai;
        int bestScore = int.MinValue;

        List<Node> possibleMoves = GetPossibleMoves(ref board, dropped, rotated);

        List<Node> bestMoves = new List<Node>();
        foreach (Node move in possibleMoves)
        {
            bool _dropped = move.dropped || dropped;
            bool _rotated = move.rotated || rotated;

            if (_dropped && _rotated)
            {
                _dropped = false;
                _rotated = false;
            }
            int score = 0;
            if (move.dropped)
            {
                MakeMove(ref board, move, currentPlayer);
                score = Minimax(ref board, maxDepth, int.MinValue, int.MaxValue, !(_dropped && _rotated), _dropped, _rotated);
                TakeToken(ref board, move.column);
            }
            else
            {
                Cell[,] newBoard = board.Copy();
                MakeMove(ref newBoard, move, currentPlayer);
                score = Minimax(ref newBoard, maxDepth, int.MinValue, int.MaxValue, !(_dropped && _rotated), _dropped, _rotated);
            }

            if (move.dropped)
            {
                Debug.Log("Drop Column " + (move.column + 1) + ": " + score);
            }else if(move.rotated)
            {
                Debug.Log("Rotate Direction " + move.rotateDir + ": " + score);
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestMoves.Clear();
                bestMoves.Add(move);
            }
            else if (score == bestScore)
            {
                bestMoves.Add(move);
            }
        }
        Node bestmove = bestMoves[Random.Range(0, bestMoves.Count)];

        if (bestmove.dropped)
        {
            Debug.Log("Best Move = Drop Column " + (bestmove.column + 1));
        }
        else if (bestmove.rotated)
        {
            Debug.Log("Best Move = Rotate Direction to " + bestmove.rotateDir);
        }

        return bestmove;
    }

    private int Minimax(ref Cell[,] board, int depth, int alpha, int beta, bool maximizingPlayer, bool dropped, bool rotated)
    {
        state currentPlayer = maximizingPlayer ? ai : human;

        state winState = boardManager.GetWinPlayer(ref board, currentPlayer);
        if(depth == 0 || winState != state.empty)
        {
            int evaluation_ai = Evaluation(board, ai, winState) * (int)Mathf.Pow(10, depth);

            return evaluation_ai;
        }

        if (maximizingPlayer)
        {
            int maxEval = int.MinValue;
            List<Node> possibleMoves = GetPossibleMoves(ref board, dropped, rotated);
            foreach (Node move in possibleMoves)
            {
                bool _dropped = dropped || move.dropped;
                bool _rotated = rotated || move.rotated;
                if (_dropped && _rotated)
                {
                    _dropped = false;
                    _rotated = false;
                    maximizingPlayer = false;
                }

                int eval = 0;
                if (move.dropped)
                {
                    MakeMove(ref board, move, currentPlayer);
                    eval = Minimax(ref board, depth - 1, alpha, beta, maximizingPlayer, _dropped, _rotated);
                    TakeToken(ref board, move.column);
                }
                else
                {
                    Cell[,] newBoard = board.Copy();
                    MakeMove(ref newBoard, move, currentPlayer);
                    eval = Minimax(ref newBoard, depth - 1, alpha, beta, maximizingPlayer, _dropped, _rotated);
                }

                maxEval = Mathf.Max(maxEval, eval);

                alpha = Mathf.Max(alpha, maxEval);
                if (alpha >= beta)
                {
                    break;
                }
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            List<Node> possibleMoves = GetPossibleMoves(ref board, dropped, rotated);
            foreach (Node move in possibleMoves)
            {
                bool _dropped = dropped || move.dropped;
                bool _rotated = rotated || move.rotated;
                if (_dropped && _rotated)
                {
                    _dropped = false;
                    _rotated = false;
                    maximizingPlayer = true;
                }

                int eval = 0;
                if (move.dropped)
                {
                    MakeMove(ref board, move, currentPlayer);
                    eval = Minimax(ref board, depth - 1, alpha, beta, maximizingPlayer, _dropped, _rotated);
                    TakeToken(ref board, move.column);
                }
                else
                {
                    Cell[,] newBoard = board.Copy();
                    MakeMove(ref newBoard, move, currentPlayer);
                    eval = Minimax(ref newBoard, depth - 1, alpha, beta, maximizingPlayer, _dropped, _rotated);
                }

                minEval = Mathf.Min(minEval, eval);

                beta = Mathf.Min(beta, minEval);
                if (alpha >= beta)
                {
                    break;
                }
            }
            return minEval;
        }
    }

    private List<Node> GetPossibleMoves(ref Cell[,] board, bool dropped, bool rotated)
    {
        if (dropped && rotated)
        {
           Debug.LogWarning("No possible moves obtained!"); return null;
        }

        List<Node> nodes = new List<Node>();

        if (!dropped)
        {
            for (int i = 0; i < gameRules.columnNo; i++)
            {
                //Check if the column is full
                if (board[i, gameRules.rowNo - 1].state != state.empty)
                    continue;

                nodes.Add(new Node(i, rotateDir.@null, true, false));
            }
        }

        if(!rotated)
        {
            for (int i = 0; i < 4; i++)
            {
                rotateDir dir = rotateDir.@null;
                switch (i)
                {
                    case 0: dir = rotateDir.anticlockwise; break;
                    case 1: dir = rotateDir.clockwise; break;
                    case 2: dir = rotateDir.upsidedown; break;
                    default: break;
                }
                nodes.Add(new Node(-1, dir, false, true));
            }
        }

        return nodes;
    }

    private void MakeMove(ref Cell[,] board, Node move, state currentplayer)
    {
        if(move.dropped)
        {
            DropToken(ref board, move.column, currentplayer);
        }else if (move.rotated)
        {
            RotateBoard(ref board, move.rotateDir);
        }
    }

    private void DropToken(ref Cell[,] board, int column, state currentPlayer)
    {
        int row = 0;
        for (row = gameRules.rowNo - 1; row >= 0; row--)
        {
            if (board[column, row].state != state.empty)
                break;
        }
        board[column, row + 1].state = currentPlayer;
    }

    private void TakeToken(ref Cell[,] board, int column)
    {
        int row = 0;
        for (row = gameRules.rowNo - 1; row >= 0; row--)
        {
            if (board[column, row].state != state.empty)
                break;
        }

        board[column, row].state = state.empty;
    }

    private void RotateBoard(ref Cell[,] board, rotateDir dir)
    {
        #region Array Rotation
        switch (dir)
        {
            case rotateDir.anticlockwise:
                board = board.LeftRotate();
                break;
            case rotateDir.clockwise:
                board = board.RightRotate();
                break;
            case rotateDir.upsidedown:
                board = board.FlipUpsideDown();
                break;
            case rotateDir.@null:
                //aniEvents.AnimationEnd();
                return;
            default:
                break;
        }
        #endregion

        //aniEvents.AnimationStart(dir);
        FallToken(ref board);
    }

    private void FallToken(ref Cell[,] board)
    {
        for (int column = 0; column < gameRules.columnNo; column++)
        {
            for (int row = 1; row < gameRules.rowNo; row++)
            {
                if (board[column, row].state != state.empty)
                {
                    Fall(ref board,board[column, row], column, row, out int row_end);
                }
            }
        }
    }

    private void Fall(ref Cell[,] board, Cell token, int column, int row, out int row_end)
    {
        row_end = row;
        if (row - 1 < 0)
            return;

        if (board[column, row - 1].state == state.empty)
        {
            board[column, row - 1].state = token.state;

            board[column, row].state = state.empty;

            Fall(ref board, board[column, row - 1],column, row - 1, out row_end);
        }
        else
            return;
    }

    const int SCORE_MUL = 9;
    private int ScoreCal(int input)
    {
        if (input <= 1) return 0;

        return SCORE_MUL * (int)Mathf.Pow(10f, input - 1) + ScoreCal(input - 1);
    }
    private int EvaluateWindow(state[] window, state piece, ref bool pieceWin, ref bool oppoWin)
    {
        int score = 0;
        state oppPiece = (piece == state.red) ? state.yellow : state.red;
        state empty = state.empty;


        for (int i = 1; i < gameRules.minConnectionNo - 1; i++)
        {
            if (window.Count(p => p == piece) == gameRules.minConnectionNo - i
            && window.Count(p => p == empty) == i)
            {
                score += ScoreCal(gameRules.minConnectionNo - i);
            }
        }

        for (int i = 1; i < gameRules.minConnectionNo - 1; i++)
        {
            if (window.Count(p => p == oppPiece) == gameRules.minConnectionNo - i
                && window.Count(p => p == empty) == i)
            {
                score -= ScoreCal(gameRules.minConnectionNo - i);
            }
        }


        if (window.Count(p => p == piece) >= gameRules.minConnectionNo)
        {
            score += ScoreCal(gameRules.minConnectionNo);
            pieceWin = true;
        }
        if (window.Count(p => p == oppPiece) >= gameRules.minConnectionNo)
        {
            score -= ScoreCal(gameRules.minConnectionNo);
            oppoWin = true;
        }
           
        return score;
    }

    private int Evaluation(Cell[,] board, state piece, state winState)
    {
        int score = 0;
        int minConnectionNo = gameRules.minConnectionNo;
        bool pieceWin = false, oppoWin = false;
        /*
        // Score center column
        state[] centerArray = new state[gameRules.rowNo];
        for (int r = 0; r < gameRules.rowNo; r++)
        {
            centerArray[r] = board[r, gameRules.columnNo / 2 + 1].state;
        }
        int centerCount = centerArray.Count(p => p == piece);
        score += (int)Mathf.Pow(SCORE_MUL, centerCount);
        */
        // Score Horizontal
        for (int r = 0; r < gameRules.rowNo; r++)
        {
            state[] rowArray = new state[gameRules.columnNo];
            for (int c = 0; c < gameRules.columnNo; c++)
            {
                rowArray[c] = board[r, c].state;
            }
            for (int c = 0; c < gameRules.columnNo - minConnectionNo + 1; c++)
            {
                state[] window = rowArray.Skip(c).Take(minConnectionNo).ToArray();
                score += EvaluateWindow(window, piece, ref pieceWin, ref oppoWin);
            }
        }

        // Score Vertical
        for (int c = 0; c < gameRules.columnNo; c++)
        {
            state[] colArray = new state[gameRules.rowNo];
            for (int r = 0; r < gameRules.rowNo; r++)
            {
                colArray[r] = board[r, c].state;
            }
            for (int r = 0; r < gameRules.rowNo - minConnectionNo + 1; r++)
            {
                state[] window = colArray.Skip(r).Take(minConnectionNo).ToArray();
                score += EvaluateWindow(window, piece, ref pieceWin, ref oppoWin);
            }
        }

        // Score positive sloped diagonal
        for (int r = 0; r < gameRules.rowNo - minConnectionNo + 1; r++)
        {
            for (int c = 0; c < gameRules.columnNo - minConnectionNo + 1; c++)
            {
                state[] window = new state[minConnectionNo];
                for (int i = 0; i < minConnectionNo; i++)
                {
                    window[i] = board[r + i, c + i].state;
                }
                score += EvaluateWindow(window, piece, ref pieceWin, ref oppoWin);
            }
        }

        // Score negative sloped diagonal
        for (int r = 0; r < gameRules.rowNo - minConnectionNo + 1; r++)
        {
            for (int c = 0; c < gameRules.columnNo - minConnectionNo + 1; c++)
            {
                state[] window = new state[minConnectionNo];
                for (int i = 0; i < minConnectionNo; i++)
                {
                    window[i] = board[r + minConnectionNo - 1 - i, c + i].state;
                }
                score += EvaluateWindow(window, piece, ref pieceWin, ref oppoWin);
            }
        }

        if(pieceWin && oppoWin)
        {
            if (winState == ai)
                score += ScoreCal(gameRules.minConnectionNo);
            else if (winState == human)
                score -= ScoreCal(gameRules.minConnectionNo);
        }
    
        return score;
    }
}
