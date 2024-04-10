using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [Header("Scriptable Objects")]
    [SerializeField] private GameRulesSO gameRules;
    [SerializeField] private GameEventsSO gameEvents;
    [SerializeField] private float buttonOffset;
    [SerializeField] private HandAnimator hand;

    [Header("Button Pref")]
    [SerializeField] private GameObject redButtonPref;
    [SerializeField] private GameObject yellowButtonPref;
    [SerializeField] private GameObject rotateButtonPref;

    private GameObject[] redButtons;
    private GameObject[] yellowButtons;
    private GameObject[] rotateButtons;
    private GameObject[] buttons
    {
        get
        {
            return gameRules.currentPlayer == EnumsBase.state.red ? 
                redButtons.Concat(rotateButtons).ToArray() 
                : yellowButtons.Concat(rotateButtons).ToArray();
        }
    }
    private ButtonBehavior lastButton;
    private GameObject board;

    private bool startPreload = false;

    private void Start()
    {
        #region Event Subscriptabtion 
        gameEvents.OnGameActionStarted += HideButtons;
        gameEvents.OnGameActionEnded += PlaceDropButtons;
        gameEvents.OnGameActionEnded += PlaceRotateButtons;
        #endregion

        #region initiation

            board = GameObject.FindGameObjectWithTag("Board");
            lastState_drop = gameRules.currentPlayer;
            lastState_rotate = gameRules.currentPlayer;

            //Instantiate buttons after Start()
            StartCoroutine(LateStart(() => {
                int n = Mathf.Max(gameRules.columnNo, gameRules.rowNo);

                redButtons = new GameObject[n];
                yellowButtons = new GameObject[n];
                rotateButtons = new GameObject[4];

                for (int i = 0; i < n; i++)
                {
                    yellowButtons[i] = Instantiate(yellowButtonPref, board.transform);
                    yellowButtons[i].SetActive(false);
                    redButtons[i] = Instantiate(redButtonPref, board.transform);
                    redButtons[i].SetActive(false);
                }

                for (int i = 0; i < 4; i++)
                {
                    rotateButtons[i] = Instantiate(rotateButtonPref, board.transform);
                }

                PlaceDropButtons(null);
                PlaceRotateButtons(null);
            }));
            #endregion
    }

    private IEnumerator LateStart(Action action)
    {
        yield return null;  
        action();
    }

    private void Update()
    {
        if (startPreload) return;

        #region check button inputs / set animation
        // Check button inputs and set animation
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            ButtonBehavior button = hit.collider.GetComponent<ButtonBehavior>();
            if (button != null)
            {
                // Highlight button if it is different from the last one
                if (button != lastButton)
                {
                    button.Highlighted();
                    if (lastButton != null)
                    {
                        lastButton.Idle();
                    }
                    lastButton = button;
                }

                // If the button is clicked, determine the type of action and call the appropriate game event
                if (Input.GetMouseButtonDown(0))
                {
                    startPreload = true;
                    button.Pressed();

                    int index = Array.IndexOf(buttons, button.gameObject);
                    int buttonCount = buttons.Length - 4;
                    EnumsBase.rotateDir dir = EnumsBase.rotateDir.@null;
                    EnumsBase.gameEventType type = EnumsBase.gameEventType.TokenDrop;
                    if (index >= buttonCount)
                    {
                        type = EnumsBase.gameEventType.BoardRotate;
                        if (index == buttonCount) dir = EnumsBase.rotateDir.anticlockwise;
                        else if (index == buttonCount + 1) dir = EnumsBase.rotateDir.clockwise;
                        else if (index == buttonCount + 2) dir = EnumsBase.rotateDir.upsidedown;
                        else if (index == buttonCount + 3) dir = EnumsBase.rotateDir.@null;
                        rotated = true;
                    }
                    else dropped = true;

                    Action endAction =
                    () =>
                    {
                        gameEvents.GameActionStart(new GameEventsSO.GameEventArgs
                        {
                            column = index,
                            eventType = type,
                            dir = dir,
                        });
                        startPreload = false;
                    };
                    if (index >= buttonCount)
                        endAction();
                    else 
                        hand.PreloadAnimationStart(button.transform.position, endAction);

                }
            }
            // If the mouse is not over a button, reset the last button
            else
            {
                if (lastButton != null)
                {
                    lastButton.Idle();
                    lastButton = null;
                }
            }
        }

        #endregion
    }

    #region buttons placement
    private bool rotated = false;
    private bool dropped = false;
    private EnumsBase.state lastState_drop;
    private EnumsBase.state lastState_rotate;
    private void PlaceDropButtons(GameEventsSO.GameEventArgs e)
    {
        if (lastState_drop != gameRules.currentPlayer)
        {
            lastState_drop = gameRules.currentPlayer;
            dropped = false;
        }
        if (dropped == true) return;

        for (int i = 0; i < gameRules.board.GetLength(0); i++)
        {
            PlaceButton(ref buttons[i], 
                board.transform.localScale,
                gameRules.board[i, gameRules.board.GetLength(0) - 1].localPos,
                new Vector3(0f, buttonOffset, 0f),
                Quaternion.identity);

            buttons[i].SetActive(true);
        }

    }
    private void HideButtons(GameEventsSO.GameEventArgs e)
    {
        if(dropped == true || dropped == false && rotated == true)
        {
            for (int i = 0; i < buttons.Length - 4; i++)
            {
                buttons[i].SetActive(false);
            }
        }
        if(rotated == true)
        {
            for (int i = buttons.Length - 4; i < buttons.Length; i++)
            {
                buttons[i].SetActive(false);
            }
        }
    }
    private void PlaceRotateButtons(GameEventsSO.GameEventArgs e)
    {
        if (lastState_rotate != gameRules.currentPlayer)
        {
            lastState_rotate = gameRules.currentPlayer;
            rotated = false;
        }
        if (rotated == true) return;

        int n = buttons.Length;

        PlaceButton(ref buttons[n - 4],
            board.transform.localScale,
            gameRules.board[0, gameRules.board.GetLength(0) / 2].localPos,
            new Vector3(-buttonOffset, 0f, 0f),
            Quaternion.identity);

        PlaceButton(ref buttons[n - 3],
           board.transform.localScale,
           gameRules.board[gameRules.board.GetLength(1) - 1, gameRules.board.GetLength(0) / 2].localPos,
           new Vector3(buttonOffset, 0f, 0f),
           Quaternion.identity);

        PlaceButton(ref buttons[n - 2],
           board.transform.localScale,
           gameRules.board[gameRules.board.GetLength(1) - 1, gameRules.board.GetLength(0) - 1].localPos,
           new Vector3(buttonOffset, 0f, 0f),
           Quaternion.identity);

        PlaceButton(ref buttons[n - 1],
           board.transform.localScale,
           gameRules.board[gameRules.board.GetLength(1) / 2, 0].localPos,
           new Vector3(0f, -buttonOffset, 0f),
           Quaternion.identity);

    }
    private void PlaceButton(ref GameObject button, Vector3 localScale,Vector3 localPos, Vector3 offset, Quaternion rotation)
    {
        button.transform.localScale = localScale;
        button.transform.localPosition = localPos;
        button.transform.position += offset;
        button.transform.rotation = rotation;
        button.SetActive(true);
    }
    #endregion

    private void OnDisable()
    {
        #region Event Unsubscription
        gameEvents.OnGameActionStarted -= HideButtons;
        gameEvents.OnGameActionEnded -= PlaceDropButtons;
        gameEvents.OnGameActionEnded -= PlaceRotateButtons;
        #endregion
    }

}
