using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadManager : MonoBehaviour
{
    public static RoadManager instance {get; private set;}


    // At index of playerID-1 a Dictionary is stored, with roads as keys and nodes, through which the road can be accessed, as values
    public Dictionary<RoadProperties, List<Node>>[] AccessableRoadsForPlayerID {get; private set;}

    private Node[] AllNodes;

    void Awake(){
        if(instance){
            Debug.LogError("Duplicate RoadManager found on " + this.name);
            Destroy(this);
            return;
        }

        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        AllNodes = GameObject.Find("Nodes").GetComponentsInChildren<Node>();
    }

    public void InitialiseRoadDictionary(List<int> playerIDs){
        
        AccessableRoadsForPlayerID = new Dictionary<RoadProperties, List<Node>>[playerIDs.Count];

        int currentPlayerID;
        for(int index = 0; index < playerIDs.Count; ++index){
            currentPlayerID = playerIDs[index]-1;

            if(currentPlayerID < 0){
                Debug.LogError("Critical! Player ID is zero or less. Array can not be accessed and road management not be initialized!");
                return;
            }

            AccessableRoadsForPlayerID[currentPlayerID] = new Dictionary<RoadProperties, List<Node>>();
        }
    }

    public void AddAccessibleRoadForPlayer(int playerID, RoadProperties road, Node node){
        var dict = AccessableRoadsForPlayerID[playerID-1];

        if(!dict.ContainsKey(road)){
            dict.Add(road, new List<Node>());
            dict[road].Add(node);
        } else if(!dict[road].Contains(node)){
            dict[road].Add(node);
        } else {
            //TODO: Called multiple times?
            Debug.LogWarning("Road-Node pair already existing in dictionary for player" + playerID);
        }
    }
        
    // Remove Road
    public void RemoveAccessibleRoadForPlayers(RoadProperties road){
        Dictionary<RoadProperties, List<Node>> currentPlayerDict;
        for(int index = 0; index < AccessableRoadsForPlayerID.Length; ++index){
            currentPlayerDict = AccessableRoadsForPlayerID[index];
            // Check if player has access to road
            if(currentPlayerDict.ContainsKey(road)){
                currentPlayerDict.Remove(road);
            }
        }
    }
    
    // Remove Road
    public void RemoveAccessibleRoadForPlayers(RoadProperties road, Node node){
        Dictionary<RoadProperties, List<Node>> currentPlayerDict;
        for(int index = 0; index < AccessableRoadsForPlayerID.Length; ++index){
            currentPlayerDict = AccessableRoadsForPlayerID[index];
            // Check if player has access to road
            if(currentPlayerDict.ContainsKey(road)){
                currentPlayerDict[road].Remove(node);
                if(currentPlayerDict[road].Count == 0)
                    currentPlayerDict.Remove(road);
            }
        }
    }

    public void SetAccessableRoadsVisibilityForPlayer(int playerID, bool isShown){
        var roadDict = AccessableRoadsForPlayerID[playerID-1];
        foreach(RoadProperties road in roadDict.Keys){
            road.SetVisibility(isShown);
            road.SetInteractibility(isShown);

            //TODO: Wofür?? Node sichbar machen?
            /*if(PlayerManager.instance.preparationRoundFinished){
                road.EndNodes[0].SetVisibility(isShown);
                road.EndNodes[1].SetVisibility(isShown);
            }*/

        }
    }

    public void HideRoadsAndNodesForAllPlayers(){
        Dictionary<RoadProperties, List<Node>> currentDict;
        for(int index = 0; index < AccessableRoadsForPlayerID.Length; ++index){
            currentDict = AccessableRoadsForPlayerID[index];
            foreach(RoadProperties road in currentDict.Keys){
                road.SetVisibility(false);
                road.SetInteractibility(false);
            }
        }

        SetAllNodesVisibility(false);
    }

    public void SetAllNodesVisibility(bool visible){
        for(int index = 0; index < AllNodes.Length; ++index){
            AllNodes[index].SetVisibilityIfBuildable(visible);
        }
    }

    public void SetRoadVisibiliatyAndInteractibilityForPlayer(int playerID, bool isPossible){
        foreach(RoadProperties road in AccessableRoadsForPlayerID[playerID-1].Keys){
            road.SetVisibility(isPossible);
            road.SetInteractibility(isPossible);
        } 
    }
}
