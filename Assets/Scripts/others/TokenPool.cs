using System.Collections.Generic;
using UnityEngine;

public class TokenPool : MonoBehaviour
{
    [Header("Scriptable Objects")]
    [SerializeField] private GameRulesSO gameRules;
    [SerializeField] private GameEventsSO gameEvents;
    [SerializeField] private AudioEventsSO audioEvents;

    [Header("Prefabs")]
    [SerializeField] private GameObject yellowTokenPref;
    [SerializeField] private GameObject redTokenPref;
    [SerializeField] private GameObject handPref;

    private Queue<GameObject> redTokens = new Queue<GameObject> ();
    private Queue<GameObject> yellowTokens = new Queue<GameObject>();
    private Dictionary<EnumsBase.pool, GameObject> dictionaryPool = new Dictionary<EnumsBase.pool, GameObject>();

    private void Start()
    {
        gameEvents.OnObjectPoolOuted += InstantiateObject;
        gameEvents.OnObjectPoolIned += DestroyObject;

        if (gameRules.currentPlayer == EnumsBase.state.red)
            CreateObjectPool(redTokenPref, yellowTokenPref, redTokens, yellowTokens);
        else
            CreateObjectPool(yellowTokenPref, redTokenPref, yellowTokens, redTokens);

        CreateObjectPool(handPref, EnumsBase.pool.hand);
    }

    private void CreateObjectPool(GameObject firstPref, GameObject secondPref,
    Queue<GameObject> firstQueue, Queue<GameObject> secondQueue)
    {
        for (int i = 0; i < gameRules.columnNo * gameRules.rowNo; i++)
        {
            GameObject token = null;
            if (i % 2 == 0)
            {
                token = Instantiate(firstPref, transform);
                firstQueue.Enqueue(token);
                token.SetActive(false);
            }
            else
            {
                token = Instantiate(secondPref, transform);
                secondQueue.Enqueue(token);
                token.SetActive(false);
            }

            audioEvents.AudioSourceLoad(ref token, new AudioEventsSO.AudioEventsArgs 
            { 
                audio = EnumsBase.audio.TokenDrop 
            });
        }

    }

    private void CreateObjectPool(GameObject pref, EnumsBase.pool key)
    {
        GameObject createdObject = Instantiate(pref, transform);
        dictionaryPool.Add(key, createdObject);
        createdObject.SetActive(false);
    }

    public GameObject InstantiateObject(EnumsBase.pool key, bool active)
    {
        GameObject targetObject = null;
        switch (key)
        {
            case EnumsBase.pool.redToken:
                targetObject = redTokens.Dequeue();
                break;
            case EnumsBase.pool.yellowToken:
                targetObject = yellowTokens.Dequeue();
                break;
            default:
                targetObject = dictionaryPool[key];
                break;
        }
        targetObject.SetActive(active);
        targetObject.transform.parent = null;
        return targetObject;
    }

    public void DestroyObject(ref GameObject targetObject, EnumsBase.pool key)
    {
        switch (key)
        {
            case EnumsBase.pool.redToken:
                redTokens.Enqueue(targetObject);
                break;
            case EnumsBase.pool.yellowToken:
                yellowTokens.Enqueue(targetObject);
                break;
            default:
                break;
        }
        targetObject.SetActive(false);
        targetObject.transform.parent = transform;
    }
    private void OnDisable()
    {
        gameEvents.OnObjectPoolOuted -= InstantiateObject;
        gameEvents.OnObjectPoolIned -= DestroyObject;
    }

}
