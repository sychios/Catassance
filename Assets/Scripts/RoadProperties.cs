using System.Collections.Generic;
using UnityEngine;

public class RoadProperties : MonoBehaviour
{
    public Node[] EndNodes {get; private set;} = new Node[2];

    public Dictionary<Node, List<RoadProperties>> AccessibleRoadsByEndNode {get; private set;} = new Dictionary<Node, List<RoadProperties>>();

    private MeshRenderer meshRenderer;
    private BoxCollider boxCollider;

    public int OwnerID {get; private set;}
    public bool IsBuildable {get; private set;} = true;

    private Color mouseOverColor = Color.cyan;
    private Color defaultColor = Color.white;

    private bool isHovered;

    void Start(){
        meshRenderer = GetComponent<MeshRenderer>();
        boxCollider = GetComponent<BoxCollider>();

        SetVisibility(false);
        SetInteractibility(false);
    }

    void Update(){
        if(!isHovered)
            return;

        if(Input.GetKeyDown(KeyCode.Mouse0)){
            PlaceRoad(PlayerManager.instance.CurrentPlayerID);
            isHovered = false;
        }
    }

    void OnMouseOver(){
        if(!IsBuildable)
            return;
        meshRenderer.material.color = mouseOverColor;
        isHovered = true;
    }

    void OnMouseExit(){
        if(isHovered)
            meshRenderer.material.color = defaultColor;
        isHovered = false;
    }

    public void SetVisibility(bool visible){
        meshRenderer.enabled = visible;
    }

    public void SetInteractibility(bool interactable){
        boxCollider.enabled = interactable;
    }

    public void PlaceRoad(int ownerID){
        if(!IsBuildable){
            Debug.LogError(string.Format("Trying to built a road not buidlable at <{0}>", this.gameObject.transform.position));
            return;
        }

        Player currentPlayer = PlayerManager.instance.CurrentPlayer;

        // Final check for viable ressources here
        if(PlayerManager.instance.preparationRoundFinished && !currentPlayer.PlayerRessources.IsRessourceAmountSatisfied(RessourcesManager.instance.RessourceAmountPerRoad)){
            Debug.Log("Not enough ressources to place a road!");
            return;
        }

        // Set owner and color
        OwnerID = ownerID;
        meshRenderer.material.color = currentPlayer.PlayerColor;

        // Prohibit collision detection
        boxCollider.enabled = false;
        IsBuildable = false;

        // TODO: falls end node nicht bebaut ist, füge erreichbare Straßen dem Spieler zum bebauen hinzu
        // TODO: Wenn andere node bebaut wird, entferne erreichbare Straßen
        // TODO: evtl. erreichbare Straßen zusammen mit Liste von "dependent" nodes merken? Wenn Liste leer, entferne Straße

        // For each EndNode:
        // Add node as key and initialize empty List<RoadProperties> as value
        // Add AccessibleRoads from node as values to list
        var fstNode = EndNodes[0];
        var sndNode = EndNodes[1];

        // Remove this road from the accessible roads of the owner and other players
        RoadManager.instance.RemoveAccessibleRoadForPlayers(this);

        // If there is no building on node, we can build roads along it
        // TODO: currently only checking if the node has a building, but not whom it is. If building belongs to player trying to built this road, it can be built
        // Building on node already makes appendent roads accessible, so no need to check if there is a building and who it belongs to
        List<RoadProperties> nodeRoads;
        if(!fstNode.HasBuilding){
            nodeRoads = fstNode.NodeUtility.accessibleRoads;

            currentPlayer.AddAcessibleNode(fstNode);

            // Add roads accessible from node
            foreach(RoadProperties road in nodeRoads){
                if(road != this && road.IsBuildable){
                    RoadManager.instance.AddAccessibleRoadForPlayer(OwnerID, road, fstNode);
                    road.SetInteractibility(true);
                    road.SetVisibility(true);
                }
            }
        }

        if(!sndNode.HasBuilding){
            nodeRoads = sndNode.NodeUtility.accessibleRoads;

            currentPlayer.AddAcessibleNode(sndNode);

            // Add roads accessible from node
            foreach(RoadProperties road in nodeRoads){
                if(road != this && road.IsBuildable){
                    RoadManager.instance.AddAccessibleRoadForPlayer(OwnerID, road, fstNode);
                    road.SetInteractibility(true);
                    road.SetVisibility(true);
                }
            }
        }

        // If still in preparation round: hide all accessible roads after building one and enable continue button for player
        if(!PlayerManager.instance.preparationRoundFinished){
            RoadManager.instance.SetRoadVisibiliatyAndInteractibilityForPlayer(OwnerID, false);
            currentPlayer.PlayerUI.SetFinishTurnButtonInteractable(true);
        } else {
            //Reduce player ressources if out of preparation round and update structure visibility, as ressources were spent
            currentPlayer.PlayerRessources.DiscardRessources(RessourcesManager.instance.RessourceAmountPerRoad);
            //currentPlayer.ShowBuildableStructuresIfRessourcesSatisfied();
        }
    }

    public void SetEndNodes(Node fstNode, Node sndNode){
        EndNodes[0] = fstNode;
        EndNodes[1] = sndNode;
    }

    // When a village is being built on a node by a player not owning this road, the owner of this road can no further access the roads "behind" the node
    public void PlaceBuildingOnEndNode(int playerID, Node node){
        if(OwnerID != playerID){
            if(AccessibleRoadsByEndNode.ContainsKey(node))

                for(int index = 0; index < AccessibleRoadsByEndNode[node].Count; ++index){
                    AccessibleRoadsByEndNode[node][index].SetVisibility(false);
                }

                AccessibleRoadsByEndNode.Remove(node);
        }
    }
}
