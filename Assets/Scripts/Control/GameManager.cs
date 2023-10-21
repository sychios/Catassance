using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField]
    private BoardManager boardManager;

    [SerializeField]
    private PlayerManager playerManager;

    void Awake(){
        if(!instance){
            instance = this;
        } else {
            Debug.LogError("Multiple instances of GameManager found on " + gameObject.name + ". Destroying this instance.");
            Destroy(this);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        boardManager.SpawnBoard();
        boardManager.SpawnRoads();

        InitiateGame();
    }

    void InitiateGame(){
        playerManager.StartPreparationRound();
    }
}
