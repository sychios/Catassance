using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    // Can be built on? Possible only if 1 node or more away from another building
    public bool IsBuildable {get; private set;} = true;

    // Necessary for managing the buildability of roads. A node can be not buildable, 
    // but enemy players can build along it as long as theres no building on it
    public bool HasBuilding {get; private set;} = false;

    // Can building on this tile be upgraded to town
    public bool IsUpgradeable {get; private set;} = false;

    // Tiles appendent to this node: buildings on this node are effecting the yield players get
    [SerializeField]
    public List<Tile> AffectedTiles = new List<Tile>();

    // 
    public NodeUtilities NodeUtility {get; private set;}

    // Player ID
    public int OwnerID {get; private set;} = -1;


    // Prefabs (store in a single entity (GameManager/RessourceManager?) instead?)
    private GameObject prefabSettlement;
    private GameObject prefabTown;

    private MeshRenderer meshRenderer;

    // Hovering
    private bool isHovered = false;

    private Color mouseOverColor = Color.red;
    private Color originalColor = Color.white;


    private GameObject currentBuilding;

    void Start(){
        AddNodeToAffectedTiles();
        NodeUtility = GetComponent<NodeUtilities>();

        meshRenderer = GetComponent<MeshRenderer>();

        prefabSettlement = Resources.Load<GameObject>("Prefabs/Settlement");
        prefabTown = Resources.Load<GameObject>("Prefabs/Town");
    }

    void Update(){        
        if(!isHovered) return;

        if(Input.GetKeyDown(KeyCode.Mouse0)){
            PlaceVillage();
            isHovered = false;
        }
    }



    void AddNodeToAffectedTiles(){
        for(int index = 0; index < AffectedTiles.Count; ++index){
            AffectedTiles[index].AddDependentNode(this);
        }
    }

    void IncreaseYieldOfAffectedTiles(int increaseAmount, int playerID){
        if(increaseAmount <= 0){
            Debug.LogError("Trying to increase tile yield by 0 or less on " + this.gameObject.name);
            return;
        }

        for(int index = 0; index < AffectedTiles.Count; ++index){
            AffectedTiles[index].IncreaseYieldForPlayerID(increaseAmount, playerID);
        }
    }

    public void PlaceVillage(){
        if(!IsBuildable){
            Debug.LogError(string.Format("Trying to build on a node not buildable ({0})", this.gameObject.name));
            return;
        }

        Player currentPlayer = PlayerManager.instance.CurrentPlayer;

        // After the preparation round do a final check for viable ressources here
        if(PlayerManager.instance.preparationRoundFinished && !currentPlayer.PlayerRessources.IsRessourceAmountSatisfied(RessourcesManager.instance.RessourceAmountPerVillage)){
            Debug.LogError(string.Format("Critical: Not enough ressources to place a village on node {0} for player {1}.", this.gameObject.name, currentPlayer.PlayerName));
            return;
        }

        // Hide node
        SetVisibilityIfBuildable(false);

        // Hide neighbour nodes and prohibit building on them
        Node neighbourNode;
        foreach(GameObject go in NodeUtility.directNeighbours){
            neighbourNode = go.GetComponent<Node>();
            neighbourNode.SetVisibilityIfBuildable(false);
            neighbourNode.IsBuildable = false;
        }

        // Set owner of the node (the player building on it)
        OwnerID = PlayerManager.instance.CurrentPlayerID;
        
        // Instantiate village-gameobject and set color to owners color
        currentBuilding = Instantiate(prefabSettlement, transform.position, Quaternion.identity);
        currentBuilding.GetComponentInChildren<Settlement>().SetSettlement(currentPlayer.PlayerColor, currentPlayer.PlayerID, this);
        IsBuildable = false;
        IsUpgradeable = true;
        HasBuilding = true;

        // Add settlement to players list
        currentPlayer.AddSettlement(currentBuilding.GetComponentInChildren<Settlement>());

        // Add point to player
        PlayerManager.instance.CurrentPlayer.AddPointsToPlayer();

        // Add buildable roads to this player, as building village grants access to roads going off this node
        // Show newly added roads
        RoadProperties road;
        for(int index = 0; index < NodeUtility.accessibleRoads.Count; ++index){
            road = NodeUtility.accessibleRoads[index];
            if(road.IsBuildable){
                RoadManager.instance.AddAccessibleRoadForPlayer(OwnerID, road, this);
                road.SetVisibility(true);
                road.SetInteractibility(true);
            }
        }

        // Increase the yield of teh tiles next to this node for the owner
        IncreaseYieldOfAffectedTiles(1, OwnerID);

        // During Preparation phase hide nodes after building a village
        if(!PlayerManager.instance.preparationRoundFinished){
            RoadManager.instance.SetAllNodesVisibility(false);
        } else {
            //Reduce player ressources if out of preparation round and update structure visibility, as ressources were spent
            currentPlayer.PlayerRessources.DiscardRessources(RessourcesManager.instance.RessourceAmountPerVillage);
        }
    }

    public void PlaceTown(){
        if(!IsUpgradeable){
            Debug.LogError(string.Format("Trying upgrade a village node not upgradeable ({0})", this.gameObject.name));
            return;
        }

        var playerID = PlayerManager.instance.CurrentPlayerID;

        if(playerID != OwnerID){
            Debug.LogError(string.Format("Player <{0}> is trying to upgrade building on node with village of player <{1}>.", playerID, OwnerID));
            return;
        }
        
        Player currentPlayer = PlayerManager.instance.CurrentPlayer;
        
        Destroy(currentBuilding);
        currentBuilding = Instantiate(prefabTown, transform.position, Quaternion.identity);

        // Set color of town
        var meshRendererComponents = currentBuilding.GetComponentsInChildren<MeshRenderer>();
        for(int index = 0; index < meshRendererComponents.Length; ++index){
            meshRendererComponents[index].material.color = currentPlayer.PlayerColor;
        }

        IsUpgradeable = false;

        // Increase yield
        IncreaseYieldOfAffectedTiles(1, playerID);

        // Add point to player
        PlayerManager.instance.CurrentPlayer.AddPointsToPlayer();

        // Discard ressources and update visibility of structures, as here were ressources spent
        currentPlayer.PlayerRessources.DiscardRessources(RessourcesManager.instance.RessourceAmountPerTown);
    }

    public void SetVisibilityIfBuildable(bool visible){
        this.gameObject.GetComponent<MeshRenderer>().enabled = IsBuildable && visible;
        this.gameObject.GetComponent<CapsuleCollider>().enabled = IsBuildable && visible;
    }

    void OnMouseOver(){
        meshRenderer.material.color = PlayerManager.instance.CurrentPlayer.PlayerColor;
        isHovered = true;

    	MeshRenderer nMeshRenderer;
        foreach(GameObject go in NodeUtility.directNeighbours.ToArray()){
            nMeshRenderer = go.GetComponent<MeshRenderer>();
            if(nMeshRenderer != null)
                nMeshRenderer.material.color = PlayerManager.instance.CurrentPlayer.PlayerColor * 0.75f;
        }
    }

    void OnMouseExit(){
        meshRenderer.material.color = originalColor;
        isHovered = false;
        
    	MeshRenderer nMeshRenderer;
        foreach(GameObject go in NodeUtility.directNeighbours.ToArray()){
            nMeshRenderer = go.GetComponent<MeshRenderer>();
            if(nMeshRenderer != null)
                nMeshRenderer.material.color = originalColor;
        }
    }

}
