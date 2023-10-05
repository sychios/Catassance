using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeUtilities : MonoBehaviour
{
    // Granting special ability when placing a building (town, settlement) on this node?
    private enum nodeSpecialty{
        NONE,
        TRADING_REDUCED,
        WHEAT,
        WOOD,
        WHOOL,
        ORE,
        CLAY
    };
    [SerializeField]
    private nodeSpecialty specialty = nodeSpecialty.NONE;

    // Position information
    private bool nameIsEven;

    public List<GameObject> directNeighbours {get; private set;}= new List<GameObject>();

    // Indicates whether a road to the Node with this NodeUtilities has already been spawned.
    public List<NodeUtilities> connectedNeighbours {get; set;} = new List<NodeUtilities>();

    // All roads accessible from this node
    public List<RoadProperties> accessibleRoads {get; private set;} = new List<RoadProperties>();

    private GameObject roadPrefab;

    // Start is called before the first frame update
    void Start()
    {
        // Nodes are name by schema "row_column", e.g. "3_1" for the second node in the fourth row
        nameIsEven = int.Parse(this.gameObject.name.Split('_')[0]) % 2 == 0;
        //Debug.Log(gameObject.name + " is even? " + nameIsEven);

        roadPrefab = Resources.Load<GameObject>("Prefabs/Road");

        // Direct neighbours
        directNeighbours = GetDirectNeighbours();
    }

    List<GameObject> GetDirectNeighbours(){
        Transform[] nodesTransforms = GameObject.Find("Nodes").transform.GetComponentsInChildren<Transform>();

        int thisNodeRow = int.Parse(transform.gameObject.name.Split('_')[0]);
        int thisNodeColumn = int.Parse(transform.gameObject.name.Split('_')[1]);

        // At most a node can have three neighbours
        string nb1;
        string nb2;
        string nb3;

        // Search for direct neighbours
        if(thisNodeRow <= 5){
            if(nameIsEven){
                nb1 = (thisNodeRow-1).ToString() + "_" + thisNodeColumn;
                nb2 = (thisNodeRow+1).ToString() + "_" + (thisNodeColumn+1).ToString();
                nb3 = (thisNodeRow+1).ToString() + "_" + thisNodeColumn;
            } else {
                nb1 = (thisNodeRow-1).ToString() + "_" + thisNodeColumn;
                nb2 = (thisNodeRow-1).ToString() + "_" + (thisNodeColumn-1).ToString();
                nb3 = (thisNodeRow+1).ToString() + "_" + thisNodeColumn;
            }
        } else {
            if(nameIsEven){
                nb1 = (thisNodeRow-1).ToString() + "_" + thisNodeColumn;
                nb2 = (thisNodeRow+1).ToString() + "_" + (thisNodeColumn-1).ToString();
                nb3 = (thisNodeRow+1).ToString() + "_" + thisNodeColumn;
            } else {
                nb1 = (thisNodeRow-1).ToString() + "_" + thisNodeColumn;
                nb2 = (thisNodeRow-1).ToString() + "_" + (thisNodeColumn+1).ToString();
                nb3 = (thisNodeRow+1).ToString() + "_" + thisNodeColumn;
            }
        }
        
        GameObject nb1_go = null;
        GameObject nb2_go = null;
        GameObject nb3_go = null;


        foreach(Transform t in nodesTransforms){
            if(t.gameObject.name == nb1){
                nb1_go = t.gameObject;
            }
            if(t.gameObject.name == nb2){
                nb2_go = t.gameObject;
            }
            if(t.gameObject.name == nb3){
                nb3_go = t.gameObject;
            }
        }

        List<GameObject> neighbours = new List<GameObject>();

        if(nb1_go != null)
            neighbours.Add(nb1_go);
        if(nb2_go != null)
            neighbours.Add(nb2_go);
        if(nb3_go != null)
            neighbours.Add(nb3_go);
        
        return neighbours;
    }

    RoadProperties SpawnRoadBetweenNeighbour(GameObject neighbour){
        var directionToNeighbour = (neighbour.transform.position - this.transform.position);
        var roadPosition = this.transform.position + directionToNeighbour*0.5f;
        roadPosition.y = 0.6f;
        var road = Instantiate(roadPrefab, roadPosition, Quaternion.identity);
        road.transform.rotation = Quaternion.LookRotation(directionToNeighbour, Vector3.up);
        return road.GetComponent<RoadProperties>();
    }
    
    public void SpawnRoadsToNeighbours(){
        NodeUtilities neighbourUtilities;
        Node neighbourNode;

        for(int index = 0; index < directNeighbours.Count; ++index){
            neighbourNode = directNeighbours[index].GetComponent<Node>();
            neighbourUtilities = directNeighbours[index].GetComponent<NodeUtilities>();
            
            // Spawn road
            var roadProps = SpawnRoadBetweenNeighbour(directNeighbours[index]);

            // Set endpoints of the road, the nodes connected by the road
            roadProps.SetEndNodes(neighbourNode, this.GetComponent<Node>());

            // Add road to the roads accessible by the nodes connected by it
            neighbourUtilities.AddAccessibleRoad(roadProps);
            AddAccessibleRoad(roadProps);
        }
    }

    public void AddAccessibleRoad(RoadProperties roadProps){
        if(!accessibleRoads.Contains(roadProps))
            accessibleRoads.Add(roadProps);
    }
}
