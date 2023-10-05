using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

public class Tile : MonoBehaviour
{   
    public BoardManager.TileType tileType {get; private set;}
    public int tileNumber {get; private set;}

    [SerializeField]
    // Ressource type
    public RessourcesManager.RessourceType ressourceType {get; private set;}

    // Which nodes are affected by this tile
    public List<Node> dependentNodes {get; private set;}= new List<Node>();

    // Is this tile occupied by bandit?
    public bool occupiedByBandits {get; private set;} = false;

    // Defines how much ressource to yield for player ID occuring in the dictionary as keys
    public Dictionary<Player, int> yieldAmountPerPlayer {get; private set;} = new Dictionary<Player, int>();

    // Reference to visual effect script (animation control and displaying bandit)
    public TileVisualEffects TileVisualEffects {get; private set;}

    private bool isHovered;

    private BoxCollider tileCollider;

    private PlayerManager PlayerManager;

    private bool yieldingIsProhibitedByDisaster = false;

    void Awake(){
        tileCollider = GetComponent<BoxCollider>();
    }

    void Start(){
        PlayerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
        SetRessourceType();
    }

    void Update(){
        if(!isHovered)
            return;
        
        if(Input.GetKeyDown(KeyCode.Mouse0)){
            PlaceBandit(PlayerManager.CurrentPlayerID);
            isHovered = false;
        }
    }

    void SetRessourceType(){
        switch(tileType){
            case BoardManager.TileType.FOREST:
                ressourceType = RessourcesManager.RessourceType.WOOD;
                break;
            case BoardManager.TileType.CLAYPIT:
                ressourceType = RessourcesManager.RessourceType.CLAY;
                break;
            case BoardManager.TileType.FARM:
                ressourceType = RessourcesManager.RessourceType.WHEAT;
                break;
            case BoardManager.TileType.PASTURE:
                ressourceType = RessourcesManager.RessourceType.SHEEP;
                break;
            case BoardManager.TileType.MOUNTAIN:
                ressourceType = RessourcesManager.RessourceType.ORE;
                break;
            case BoardManager.TileType.DESERT:
                ressourceType = RessourcesManager.RessourceType.NONE;
                break;
        }
    }

    void OnMouseEnter(){
        isHovered = true;
        TileVisualEffects.SetTintingTileMaterial(true);
    }

    void OnMouseExit(){
        isHovered = false;
        TileVisualEffects.SetTintingTileMaterial(false);
    }

    public void SetCollider(bool isActive){
        tileCollider.enabled = isActive;
    }

    // PlayerID: player placing the bandit
    public void PlaceBandit(int playerID){
        BoardManager.Instance.CurrentBanditTile.RemoveBandit();
        TileVisualEffects.SetBanditVisibility(true);
        BoardManager.Instance.SetBanditTile(this);
        occupiedByBandits = true;
        SetCollider(false); 

        // Remove one random card from all players who built on this tile
        var players = yieldAmountPerPlayer.Keys.ToList();
        if(players.Count > 0){
            // If there is only 1 player, remove ressource
            if(players.Count == 1){
                players[0].PlayerRessources.DiscardRandomRessource();
            } else {
                // TODO: else let player choose who to steal from. Until then the current player is removed from random choosing
                players.Remove(PlayerManager.PlayerFromID[playerID]);
                int randomIndex = Random.Range(0, players.Count);
                players[randomIndex].PlayerRessources.DiscardRandomRessource();
            }
        }
        
        // Player can still trade and build, but not throw die
        PlayerManager.instance.CurrentPlayer.PlayerUI.SetDiceButtonInteractable(false);
        PlayerManager.instance.CurrentPlayer.PlayerUI.SetFinishTurnButtonInteractable(true);
    }

    public void RemoveBandit(){
        TileVisualEffects.SetBanditVisibility(false);
        occupiedByBandits = false;
    }


    public void SetNumberAndType(BoardManager.TileType typeOfTile, int number){
        tileType = typeOfTile;
        tileNumber = number;
    }

    public void IncreaseYieldForPlayerID(int yieldAmount, int playerID){
        Player player = PlayerManager.PlayerFromID[playerID];

        if(!yieldAmountPerPlayer.ContainsKey(player)){
            yieldAmountPerPlayer.Add(player, yieldAmount);
        } else {
            yieldAmountPerPlayer[player] += yieldAmount;
        }

        TileVisualEffects.SetActiveAnimation(true);
    }

    public void YieldRessources(bool animate = false){
        // yielding prohibited by disaster effects or bandits
        if(yieldingIsProhibitedByDisaster || occupiedByBandits)
            return;
        
        // animate even if no player owns node of tile
        if(animate){
            TileVisualEffects.StartYieldAnimation();
        }

        // No player has built anything on this tile yet
        if(yieldAmountPerPlayer.Keys.Count == 0)
            return;

        // get players from PlayerManager and individually yield ressource to him
        foreach(Player player in PlayerManager.PlayerFromID.Values){
            if(yieldAmountPerPlayer.ContainsKey(player)){
                if(!(ressourceType == RessourcesManager.RessourceType.NONE))
                    player.PlayerRessources.AddRessource(ressourceType, yieldAmountPerPlayer[player]);
            }
        }
    }

    public void AddDependentNode(Node node){
        if(!dependentNodes.Contains(node))
            dependentNodes.Add(node);
    }

    public void SetTilesVisualEffects(TileVisualEffects vs){
        TileVisualEffects = vs;
    }

    public void IsYieldingProhibitedByDisaster(bool yieldingIsProhibited){
        yieldingIsProhibitedByDisaster = yieldingIsProhibited;
        if(yieldingIsProhibited) {
            TileVisualEffects.SetNumberTextColor(Color.red);
        } else {
            TileVisualEffects.SetNumberTextColor(Color.black);
        }
    }
}
