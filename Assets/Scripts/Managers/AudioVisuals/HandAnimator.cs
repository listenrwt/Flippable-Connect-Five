using UnityEngine;
using DG.Tweening;
using System;

public class HandAnimator : MonoBehaviour
{
    [Header("Scriptable Objects")]
    [SerializeField] private GameEventsSO gameEvent;
    [SerializeField] private GameRulesSO gameRules;

    [Header("Hand")]
    [SerializeField] private float handPreloadTime;
    [SerializeField] private AnimationCurve handAnimationCurve;
    [SerializeField] private Vector3 handOffset = new Vector3(0f, .2f, .1f);

    [Header("Token Offsets")]
    [SerializeField] private Vector3 tokenPosOffset;
    [SerializeField] private Vector3 tokenRotOffset;
    [SerializeField] private Vector3 tokenScaleOffset;

    public void PreloadAnimationStart(Vector3 endPos, Action endAction)
    {
        GameObject hand = gameEvent.ObjectPoolOut(EnumsBase.pool.hand);

        EnumsBase.pool key = gameRules.currentPlayer == EnumsBase.state.red ? EnumsBase.pool.redToken : EnumsBase.pool.yellowToken;
        GameObject token = gameEvent.ObjectPoolOut(key);
        Quaternion tokenWorldRot = token.transform.rotation;
        Vector3 tokenlocalScale = token.transform.localScale;

        token.transform.parent = hand.transform;
        token.transform.localPosition = tokenPosOffset;
        token.transform.localRotation = Quaternion.Euler(tokenRotOffset);
        token.transform.localScale = tokenScaleOffset;

        Vector3 startPos = endPos + handOffset;
        hand.transform.position -= token.transform.position - startPos;

        //hand.transform.rotation *= Quaternion.Euler(new Vector3(0f, 180f, 0f));
        //hand.transform.position = new Vector3(-hand.transform.position.x, hand.transform.position.y, -hand.transform.position.z);

        hand.transform.DOMove(hand.transform.position + endPos - startPos, handPreloadTime).SetEase(handAnimationCurve).OnComplete(
            () =>
            {
                token.transform.parent = null;
                token.transform.position = endPos;
                token.transform.rotation = tokenWorldRot;
                token.transform.localScale = tokenScaleOffset;

                hand.transform.DOMove(hand.transform.position + startPos - endPos, handPreloadTime)
                .SetEase(handAnimationCurve)
                .OnComplete(() => gameEvent.ObjectPoolIn(ref hand, EnumsBase.pool.hand));

                gameEvent.ObjectPoolIn(ref token, key);

                endAction();              
            });
    }
}
