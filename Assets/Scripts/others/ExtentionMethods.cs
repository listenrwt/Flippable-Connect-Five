using System;
using UnityEngine;
using static GameRulesSO;

public static class ExtentionMethods 
{
    //An Extention Method to modify an 2D-array / matrix

    //Transpose an array
    private static T[,] Transpose<T>(this T[,] array)
    {
        int rows = array.GetLength(0);
        int columns = array.GetLength(1);
        T[,] result = array;

        //Transpose
        for (int i = 0; i < columns; i++)
        {
            for (int j = i; j < rows; j++)
            {
                T temp = result[i, j];
                result[i, j] = result[j, i];
                result[j, i] = temp;
            }
        }
        return result;
    }

    //Flip an array upside down
    public static T[,] FlipUpsideDown<T>(this T[,] array)
    {
        int rows = array.GetLength(0);
        int columns = array.GetLength(1);
        T[,] result = array;

        for (int i = 0; i < columns; i++)
        {
            int low = 0, high = columns - 1;
            while (low < high)
            {
                T temp = result[i, low];
                result[i, low] = result[i, high];
                result[i, high] = temp;
                low++;
                high--;
            }
        }

        return result;

    }

    //Rotate an array clockwise by 90deg
    public static T[,] RightRotate<T>(this T[,] array)
    {
        T[,] result = array;
        result = array.Transpose();
        result = result.FlipUpsideDown();

        return result;
    }

    //Rotate an array anti-clockwise by 90deg
    public static T[,] LeftRotate<T>(this T[,] array)
    {
        T[,] result = array;
        result = array.RightRotate().RightRotate().RightRotate();

        return result;
    }

    //Rotate a point about a certain point clockwise by ?deg
    public static Vector3 Rotate<Vector>(this Vector3 pt, Vector3 axisPt, float deg)
    {
        Vector3 newPt = pt - axisPt;
        float x = newPt.x * Mathf.Cos(deg) - newPt.y * Mathf.Sin(deg);
        float y = newPt.y * Mathf.Cos(deg) + newPt.x * Mathf.Sin(deg);
        return new Vector3(x + axisPt.x, y + axisPt.y);
    }

    public static Cell[,] Copy(this Cell[,] board)
    {
        int numRows = board.GetLength(1);
        int numCols = board.GetLength(0);
        Cell[,] newBoard = new Cell[numCols, numRows];

        // copy the entire board array to the newBoard array
        for (int i = 0; i < numCols; i++)
        {
            for (int j = 0; j < numRows; j++)
            {
                Cell cell = new Cell();
                cell.state = board[i, j].state;
                newBoard[i, j] = cell;
            }
        }

        return newBoard;
    }
}

