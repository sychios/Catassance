using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class MockUpPlayer{
    public string playerName;
    public int playerID;
}


public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance {get; private set;}

    //TODO: holding a reference <Player> of current player too could reduce looking up in "PlayerFromID" dictionary alot
    public Player CurrentPlayer {get; private set;}
    public int CurrentPlayerID {get; private set;}

    private Dictionary<Player, int> PlayerScores = new Dictionary<Player, int>();

    public Dictionary<int, Player> PlayerFromID {get; private set;} = new Dictionary<int, Player>();

    public bool preparationRoundFinished {get; private set;}
    private bool reverseSettingPlayers = false;

    [Header("Mock-Up Players")]
    [SerializeField]
    private MockUpPlayer player1;
    [SerializeField]
    private MockUpPlayer player2;
    [SerializeField]
    private MockUpPlayer player3;
    [SerializeField]
    private MockUpPlayer player4;

    [Header("Player Prefab")]
    [SerializeField]
    private GameObject playerPrefab;

    [SerializeField]
    private int pointsToWinGame = 8;

    private bool disasterIsProcessed;

    void Awake(){
        if(instance){
            Debug.LogError("Multiple instances of PlayerManager found on " + gameObject.name);
            Destroy(this);
            return;
        }

        instance = this;
    }

    void Start()
    {

        CurrentPlayerID = 1;

        InstantiateMockUpPlayers();

        return;
    }

    void Update()
    {
        if(!preparationRoundFinished)
            return;
    }

    void InstantiateMockUpPlayers(){
        // Initialize road memory system
        RoadManager.instance.InitialiseRoadDictionary(new List<int> {player1.playerID, player2.playerID, player3.playerID, player4.playerID});

        // Mock Player 1
        InitialiseMockUpPlayer(player1.playerID, player1.playerName, new Color(0f,1f,0f,1f));

        // Mock Player 2
        InitialiseMockUpPlayer(player2.playerID, player2.playerName, new Color(1f,0f,0f,1f));

        // Mock Player 3
        InitialiseMockUpPlayer(player3.playerID, player3.playerName, new Color(0f,0f,1f,1f));

        // Mock Player 4
        InitialiseMockUpPlayer(player4.playerID, player4.playerName, new Color(1f,1f,0f,1f));
    }

    void InitialiseMockUpPlayer(int id, string name, Color color){
        GameObject player;

        Player playerComp;

        player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        player.name = name + " (Player)";
        playerComp = player.GetComponent<Player>();
        PlayerFromID.Add(id, playerComp);
        playerComp.SetProperties(name, id, color);
        playerComp.DeactivateDuringPreparationPhase();
        PlayerScores.Add(playerComp, 0);
    }
    
    void ActivatePlayerByID(int activePlayerID){
        CurrentPlayer = PlayerFromID[activePlayerID];
        CurrentPlayer.Activate();
    }

    void ActivatePlayerDuringPreparationRoundByID(int activePlayerID){
        CurrentPlayer = PlayerFromID[activePlayerID];
        CurrentPlayer.ActivateDuringPreparationPhase();
    }

    void DeactivatePlayerByID(int activePlayerID){
                PlayerFromID[activePlayerID].Deactivate();
    }

    void DeactivatePlayerDuringPreparationRoundByID(int activePlayerID){
        PlayerFromID[activePlayerID].DeactivateDuringPreparationPhase();
    }

    public void StartPreparationRound(){
        ActivatePlayerDuringPreparationRoundByID(CurrentPlayerID);
    }

    //TODO: necessary to give player instance? Is it possible that a player receives winning points while not being the active player?
    // might be possible when a road gets interrupted and longest road points are switching players
    //TODO: can there be a draw/multiple winners? E.g. interrupting longest road of player A, longest road goes to player B (12 pts) and player C
    // gets points for placing a building (12 pts)
    //TODO: move logic to player and only alert manager when winning?
    //TODO: If method is called on Player A before Player B while both have winning points, Player A wins
    public void UpdatePlayerScore(Player playerInstance, int pointsToAddOrSubtract){
        if(!PlayerScores.ContainsKey(playerInstance)){
            Debug.LogError(string.Format("Critical! Player {0} is not contained in player scores. Player scores are not updated.", playerInstance));
            return;
        }
        
        PlayerScores[playerInstance] += pointsToAddOrSubtract;
        playerInstance.PlayerUI.SetPlayerScore(PlayerScores[playerInstance]);

        if(PlayerScores[playerInstance] >= pointsToWinGame){
            playerInstance.DeclareAsWinner();
            foreach(Player player in PlayerScores.Keys){
                if(player == playerInstance)
                    continue;
                player.DeclareAsLoser();
            }
        }
    }



    public void PlayerHasFinished(){
        // Handle preparation round
        if(!preparationRoundFinished){
            // Deactivate Current Player
            DeactivatePlayerDuringPreparationRoundByID(CurrentPlayerID);

            // If arrived at the first player again, he can place the last village and end the preparation round. 
            if(reverseSettingPlayers && CurrentPlayerID == 1){
                preparationRoundFinished = true;
                //Debug.LogWarning("Reached Player 1 again after reversing.");
                RoadManager.instance.HideRoadsAndNodesForAllPlayers();
                BoardManager.Instance.YieldAllTiles();
                ActivatePlayerByID(CurrentPlayerID);
                return;
            } 
            
            if(CurrentPlayerID == PlayerFromID.Count && !reverseSettingPlayers){
            // If at the last player, that player repeats and after reverse back
                reverseSettingPlayers = true;
                ActivatePlayerDuringPreparationRoundByID(CurrentPlayerID);
                return;
            }
            
            CurrentPlayerID = reverseSettingPlayers? --CurrentPlayerID : ++CurrentPlayerID;
            ActivatePlayerDuringPreparationRoundByID(CurrentPlayerID);
            return;
        }
        // Deactivate Current Player
        DeactivatePlayerByID(CurrentPlayerID);
        StartCoroutine(ProcessDisaster());
    }

    public IEnumerator ProcessDisaster(){
        // Enable disaster here
        disasterIsProcessed = false;
        
        DisasterManager.instance.ProcessGameTurn();
        
        while(!disasterIsProcessed)
            yield return null;

        ActivateNextPlayer();
    }

    public void ActivateNextPlayer(){
        // Increment player counter
        CurrentPlayerID += 1;
        if(CurrentPlayerID == PlayerFromID.Count+1)
            CurrentPlayerID = 1;

        // Enable next player
        ActivatePlayerByID(CurrentPlayerID);
    }

    public void PlayerFinished(){
        // Handle preparation round
        if(!preparationRoundFinished){
            // Deactivate Current Player
            DeactivatePlayerDuringPreparationRoundByID(CurrentPlayerID);

            // If arrived at the first player again, he can place the last village and end the preparation round. 
            if(reverseSettingPlayers && CurrentPlayerID == 1){
                preparationRoundFinished = true;
                //Debug.LogWarning("Reached Player 1 again after reversing.");
                RoadManager.instance.HideRoadsAndNodesForAllPlayers();
                BoardManager.Instance.YieldAllTiles();
                ActivatePlayerByID(CurrentPlayerID);
                return;
            } 
            
            if(CurrentPlayerID == PlayerFromID.Count && !reverseSettingPlayers){
            // If at the last player, that player repeats and after reverse back
                reverseSettingPlayers = true;
                ActivatePlayerDuringPreparationRoundByID(CurrentPlayerID);
                return;
            }
            
            CurrentPlayerID = reverseSettingPlayers? --CurrentPlayerID : ++CurrentPlayerID;
            ActivatePlayerDuringPreparationRoundByID(CurrentPlayerID);
            return;
        }
        // Deactivate Current Player
        DeactivatePlayerByID(CurrentPlayerID);

        // Enable disaster here
        disasterIsProcessed = false;

        Debug.Log("Disaster manager starts processing..");
        //DisasterManager.instance.ProcessGameTurn();

        //StartCoroutine(ProcessDisaster());

            
        Debug.Log("Wait for disaster manager..");
            
        // TODO: remove this abomination
        //while(!disasterIsProcessed)
        //    yield return null;
        
            
        Debug.Log("Disaster manager has finished!");

        // Increment player counter
        CurrentPlayerID += 1;
        if(CurrentPlayerID == PlayerFromID.Count+1)
            CurrentPlayerID = 1;

        // Enable next player
        ActivatePlayerByID(CurrentPlayerID);
    }

    public void SetDisasterIsProcessed(bool isDisasterProcessed){
        disasterIsProcessed = isDisasterProcessed;
    }
}
