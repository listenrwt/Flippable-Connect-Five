using UnityEngine;

public class EnumsBase 
{
    public enum state { empty, red, yellow } 
    public enum rotateDir { anticlockwise = -90, clockwise = 90, upsidedown = 180, @null = 0}
    public enum gameEventType { TokenDrop, BoardRotate, SwapTurn, InvalidMove,@Null }
    public enum animationEventType { TokenDrop, BoardRotate, TokenFall, Preload }

    public enum audio { TokenDrop, @Null}
    public enum pool { redToken, yellowToken, hand }
}
