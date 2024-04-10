using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationEventsSO", menuName = "ScriptableObject/AnimationEventsSO")]
public class AnimationEventsSO : ScriptableObject
{
    #region Event Args
    public class AnimationEventArgs : EventArgs
    {
        public EnumsBase.animationEventType eventType { get; set; }
        public int column { get; set; }
        public int row { get; set; }
        public int row_start { get; set; }
        public int row_end { get; set; }
        public EnumsBase.rotateDir dir { get; set; }
    }
    #endregion

    #region Animation Start Event
    public delegate void AnimationStarted(ref GameObject token, AnimationEventArgs e);
    public event AnimationStarted OnAnimationStarted;

    #region Token Animation
    public void AnimationStart(ref GameObject _token, int _column, int _row) => TokenDrop(ref _token, _column, _row);
    public void AnimationStart(ref GameObject _token, int _column, int _row_start, int _row_end) => TokenDrop(ref _token, _column, -1, _row_start, _row_end);
    private void TokenDrop(ref GameObject _token, int _column, int _row = -1, int _row_start = -1, int _row_end = -1)
    {
        OnAnimationStarted?.Invoke(ref _token, new AnimationEventArgs
        {
            eventType = _row == -1 ? EnumsBase.animationEventType.TokenFall : EnumsBase.animationEventType.TokenDrop,
            column = _column,
            row = _row,
            row_start = _row_start,
            row_end = _row_end
        }); 
    }
    #endregion

    #region Board Animation
    public void AnimationStart(EnumsBase.rotateDir _dir) => BoardRotate(_dir);
    private void BoardRotate(EnumsBase.rotateDir _dir)
    {
        GameObject nullToken = null;
        OnAnimationStarted?.Invoke(ref nullToken, new AnimationEventArgs
        {
            eventType = EnumsBase.animationEventType.BoardRotate,
            dir = _dir
        });
    }
    #endregion

    #endregion

    #region Animation End Event
    public delegate void AnimationEnded();
    public event AnimationEnded OnAnimationEnded;
    public void AnimationEnd()
    {
        OnAnimationEnded?.Invoke();
    }
    #endregion
}
