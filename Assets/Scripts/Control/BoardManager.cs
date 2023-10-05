using UnityEngine;
using System.Collections.Generic;
using System;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    [Tooltip("GameObject where each child represents a spawnpoint for a tile.")]
    [SerializeField] private GameObject tileSpawnPointsParent;

    // Every tile gets assigned a type. The 'int' representation is often used, e.g. for the correct instantiation of the tile
    public enum TileType:int{
        FOREST,
        PASTURE,
        FARM,
        CLAYPIT,
        MOUNTAIN,
        DESERT
    };

    // Prefabs of the different tile-types
    [SerializeField] private GameObject ForestPrefab;
    [SerializeField] private GameObject PasturePrefab;
    [SerializeField] private GameObject FarmPrefab;
    [SerializeField] private GameObject ClaypitPrefab;
    [SerializeField] private GameObject MountainPrefab;
    [SerializeField] private GameObject DesertPrefab;

    // Put in prefabs in the following order: forest, pasture, farm, claypit, mountain, desert
    private GameObject[] prefabArray;

    // For each dice result (int between 2 and 12) a list of tiles assigned to this int stored
    private Dictionary<int, List<Tile>> tilesForDiceResult = new Dictionary<int, List<Tile>>();

    private Tile[] allTiles;

    private List<Tile>[] tilesByRessourceType;

    private Tile[] tilesAppendentToDesert;

    private Tile[] tilesOnCoast;

    public Tile CurrentBanditTile => _currentBanditTile;
    private Tile _currentBanditTile;

    // 2D array of size 6x7 is used, so that board is centered with empty rows/columns above/below/left/right.
    // To achieve that behaviour every position indexing with board data from the tile-position-gameobjects is modified by +1.
    // This should prevent indexOutOfRangeExceptions
    // 7 is needed as middle row is has size of 5 tiles
    private Tile[,] tilePositions = new Tile[7,7];
    void Awake(){
        if(Instance){
            Destroy(this);
            Debug.LogError("Multiple BoardManager instances found on " + gameObject.name);
            return;
        }
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        prefabArray = new GameObject[6] {ForestPrefab, PasturePrefab, FarmPrefab, ClaypitPrefab, MountainPrefab, DesertPrefab};

        tilesByRessourceType = new List<Tile>[Enum.GetNames(typeof(TileType)).Length-1];
        for(int index = 0; index < tilesByRessourceType.Length; ++index){
            tilesByRessourceType[index] = new List<Tile>();
        }
    }

    /*
    Return a list of Vector2 where elements have:
        - first elemente: Range of 0 to 5 (for tiletype), 4x0 (forests), 4x1 (pastures), 4x2 (fields), 3x3 (hills), 3x4 (mountains), 1x5 (desert)
        - second elemente: Range of 2 to 12 (for tilenumbers), numbers from 3 to 11 occur twice
    */
    private List<Vector2> GenerateShuffledList(){
        List<Vector2> lst = new List<Vector2>();
        Vector2 desertProperties = new Vector2(5, -1);

        // Initialise all types and numbers
        List<int> types = new List<int>(){0,0,0,0,1,1,1,1,2,2,2,2,3,3,3,4,4,4};
        List<int> numbers = new List<int> {2,3,3,4,4,5,5,6,6,8,8,9,9,10,10,11,11,12};

        int tmpType;
        int tmpNumber;
        int random;

        // Shuffle lists
        for(int shuffleStep = 0; shuffleStep<types.Count; shuffleStep++){
            tmpType = types[shuffleStep];
            random = UnityEngine.Random.Range(shuffleStep, types.Count);
            types[shuffleStep] = types[random];
            types[random] = tmpType;
        }

        for(int shuffleStep = 0; shuffleStep<numbers.Count; shuffleStep++){
            tmpNumber = numbers[shuffleStep];
            random = UnityEngine.Random.Range(shuffleStep, numbers.Count);
            numbers[shuffleStep] = numbers[random];
            numbers[random] = tmpNumber;
        }

        // Add desert (5,1)
        random = UnityEngine.Random.Range(0, types.Count);
        tmpNumber = numbers[random];
        tmpType = types[random];
        numbers[random] = 0;
        types[random] = 5;
        numbers.Add(tmpNumber);
        types.Add(tmpType);


        Vector2 tmp = new Vector2();
        for(int i = 0; i<types.Count; i++){
            tmp = new Vector2(types[i], numbers[i]);
            lst.Add(tmp);
        }
        return lst;
    }

    public void SpawnRoads(){
        NodeUtilities[] nodesUtilities = GameObject.Find("Nodes").GetComponentsInChildren<NodeUtilities>();

        for(int index = 0; index < nodesUtilities.Length; ++index){
            string fstNamePart = nodesUtilities[index].name.Split('_')[0];

            if((int.Parse(fstNamePart) % 2) == 0){
                nodesUtilities[index].SpawnRoadsToNeighbours();
            }
        }
    }


    public void SpawnBoard(){
        // listElemen.x := type, listElemenent.y := number
        // childs: spawnpoints
        // tileList: type and number


        // für graph: 
        // - node := tile-reference, tileposition, neighbours

        List<Vector2> tileList = GenerateShuffledList();
        allTiles = new Tile[tileList.Count];
        Vector3 tilePosition;
        Tile currentTile;

        var childs = new List<Transform>();
        foreach(Transform child in tileSpawnPointsParent.transform){
            childs.Add(child);
        }

        if(childs.Count != tileList.Count){
            Debug.LogError("Critical! Different length between spawnpoints and list of tiles to spawn.");
            Debug.LogError("Spawnpoints: " + childs.Count);
            Debug.LogError("Tile list: " + tileList.Count);
        }

        int tileType;
        int tileNumber;
        for(int tileCounter = 0; tileCounter < tileList.Count; ++tileCounter)
        {
            // Get Tile Properties: x=tiletype, y=tilenumber
            tileType = (int) tileList[tileCounter].x;
            tileNumber =  (int) tileList[tileCounter].y;
            tilePosition = childs[tileCounter].transform.position;
            // Get Tile-Component from current tile
            currentTile = childs[tileCounter].GetComponent<Tile>();

            if(!tilesForDiceResult.ContainsKey(tileNumber)){
                tilesForDiceResult.Add(tileNumber, new List<Tile>());
            }
            tilesForDiceResult[tileNumber].Add(currentTile);

            var spawnPointNameSplit = childs[tileCounter].gameObject.name.Split("_");
            var arrayPosition = new int[2]{
                int.Parse(spawnPointNameSplit[0]), 
                int.Parse(spawnPointNameSplit[1])
                };

            // Set tile reference in tile position array
            tilePositions[arrayPosition[0]+1, arrayPosition[1]+1] = currentTile;

            SpawnTile(tileType, tileNumber, tilePosition, currentTile);
            
            // Set desert as initial bandit tile
            if(tileType == (int) TileType.DESERT){
                _currentBanditTile = currentTile;

                tilesAppendentToDesert = GetSurroudingTiles(arrayPosition[0]+1, arrayPosition[1]+1);
            } else {
                tilesByRessourceType[(int) tileType].Add(currentTile);
            }
            allTiles[tileCounter] = currentTile;
        }

        // add +1 to every x/y indexing (remember: 6x6 array for a 4x5 board)
        tilesOnCoast = new Tile[]{
            tilePositions[0+1,1+1],
            tilePositions[0+1,2+1],
            tilePositions[0+1,3+1],
            tilePositions[1+1,1+1],
            tilePositions[1+1,4+1],
            tilePositions[2+1,0+1],
            tilePositions[2+1,4+1],
            tilePositions[3+1,1+1],
            tilePositions[3+1,4+1],
            tilePositions[4+1,2+1],
            tilePositions[4+1,3+1],
            tilePositions[4+1,4+1]
        };
    }


    void SpawnTile(int tileType, int tileNumber, Vector3 tilePosition, Tile tileReference)
    {
        GameObject currentTile = Instantiate(prefabArray[tileType],  
                                            new Vector3(tilePosition.x, 0, tilePosition.z), 
                                            prefabArray[tileType].transform.rotation);

        // Set type and dice number of tile and add to tiletype-list
        tileReference.GetComponent<Tile>().SetNumberAndType((TileType) tileType, tileNumber);

        tileReference.SetTilesVisualEffects(currentTile.GetComponent<TileVisualEffects>());
        currentTile.GetComponent<TileVisualEffects>().SetNumberTextValue(tileNumber);

    }

    public void ProcessDiceResult(int diceResult){
        if(diceResult == 7){
            for(int index = 1; index <= PlayerManager.instance.PlayerFromID.Keys.Count; ++index){
                PlayerManager.instance.PlayerFromID[index].PlayerRessources.BanditRessourcePileDiscarding();
            }

            SetCollidersForBanditPlacement(true);
            return;
        }
        
        // Search for tiles with the dice-result as number and let them yield ressources
        List<Tile> yieldingTiles;
        tilesForDiceResult.TryGetValue(diceResult, out yieldingTiles);
        if(yieldingTiles == null){
            Debug.LogError("No tiles found for dice result " + diceResult);
            return;
        }
        for(int tileIndex = 0; tileIndex < yieldingTiles.Count; ++tileIndex){
            yieldingTiles[tileIndex].YieldRessources(true);
        }
    }

    // At the end of the preparation round, all tiles yield one time
    public void YieldAllTiles(){
        for(int index = 0; index < allTiles.Length; ++index){
                allTiles[index].YieldRessources();
        }
    }

    // Enables/Disables colliders of tiles necessary for mouse detection
    void SetCollidersForBanditPlacement(bool isActive){
        for(int index = 0; index < allTiles.Length; ++index){
            if(allTiles[index] != _currentBanditTile)
                allTiles[index].SetCollider(isActive);
        }
    }

    // Don't add +1 to xPos and yPos here! Do it in call
    Tile[] GetSurroudingTiles(int xPos, int yPos){
        var tmp = new List<Tile>();
        // left and right
        if(tilePositions[xPos, yPos + 1] != null){
            tmp.Add(tilePositions[xPos, yPos + 1]);
        }
        if(tilePositions[xPos, yPos - 1] != null){
            tmp.Add(tilePositions[xPos, yPos - 1]);
        }

        // Up and down        
        if(tilePositions[xPos + 1, yPos] != null){
            tmp.Add(tilePositions[xPos + 1, yPos]);
        }        
        if(tilePositions[xPos - 1, yPos] != null){
            tmp.Add(tilePositions[xPos - 1, yPos]);
        }
        

        int offset = xPos == 2 ? -1 : +1;
        if(tilePositions[xPos + 1, yPos + offset] != null){
            tmp.Add(tilePositions[xPos + 1, yPos + offset]);
        }

        offset = xPos == 3 ? +1 : -1;        
        if(tilePositions[xPos - 1, yPos + offset] != null){
            tmp.Add(tilePositions[xPos - 1, yPos - offset]);
        }
        
        return tmp.ToArray();
    }

    Tile[] GetUpperAppendentTiles(int xPos, int yPos){
        var tmp = new List<Tile>();
        
        
        if(tilePositions[xPos+1, yPos] != null){
            tmp.Add(tilePositions[xPos+1, yPos]);
        }

        var offset = xPos == 2? -1 : +1;

        if(tilePositions[xPos+1, yPos + offset] != null){
            tmp.Add(tilePositions[xPos+1, yPos + offset]);
        }

        return tmp.ToArray();
    }
    
    Tile[] GetLowerAppendentTiles(int xPos, int yPos){
        var tmp = new List<Tile>();

        if(tilePositions[xPos-1, yPos] != null){
            tmp.Add(tilePositions[xPos-1, yPos]);
        }

        var offset = xPos == 3? +1 : -1 ;

        
        if(tilePositions[xPos-1, yPos + offset] != null){
            tmp.Add(tilePositions[xPos-1, yPos + offset]);
        }

        return tmp.ToArray();
    }

    Tile[] GetLeftAppendentTiles(int xPos, int yPos){
        var tmp = new List<Tile>();

        // Add neighbour directly to the left
        if(tilePositions[xPos, yPos-1] != null){
            tmp.Add(tilePositions[xPos, yPos-1]);
        }


        switch(xPos){
            case 1:
                if(tilePositions[xPos+1, yPos] != null){
                    tmp.Add(tilePositions[xPos+1, yPos]);
                }
                break;
            case 2:
                if(tilePositions[xPos+1, yPos-1] != null){
                    tmp.Add(tilePositions[xPos+1, yPos-1]);
                }
                if(tilePositions[xPos-1, yPos-1] != null){
                    tmp.Add(tilePositions[xPos-1, yPos-1]);
                }
                break;
            case 3:
                if(tilePositions[xPos+1, yPos] != null){
                    tmp.Add(tilePositions[xPos+1, yPos]);
                }
                if(tilePositions[xPos-1, yPos] != null){
                    tmp.Add(tilePositions[xPos-1, yPos]);
                }
                break;
            case 4:
                if(tilePositions[xPos+1, yPos] != null){
                    tmp.Add(tilePositions[xPos+1, yPos]);
                }
                if(tilePositions[xPos-1, yPos-1] != null){
                    tmp.Add(tilePositions[xPos-1, yPos-1]);
                }
                break;
            case 5:
                if(tilePositions[xPos-1, yPos-1] != null){
                    tmp.Add(tilePositions[xPos-1, yPos-1]);
                }
                break;
        }

        return tmp.ToArray();
    }

    Tile[] GetRightAppendentTiles(int xPos, int yPos){
        var tmp = new List<Tile>();

        // Add neighbour directly to the right
        if(tilePositions[xPos, yPos+1] != null){
            tmp.Add(tilePositions[xPos, yPos+1]);
        }

        switch(xPos){
            case 1:
                if(tilePositions[xPos+1, yPos+1] != null){
                    tmp.Add(tilePositions[xPos+1, yPos+1]);
                }
                break;
            case 2:
                if(tilePositions[xPos+1, yPos] != null){
                    tmp.Add(tilePositions[xPos+1, yPos]);
                }
                if(tilePositions[xPos-1, yPos] != null){
                    tmp.Add(tilePositions[xPos-1, yPos]);
                }
                break;
            case 3:
                if(tilePositions[xPos+1, yPos+1] != null){
                    tmp.Add(tilePositions[xPos+1, yPos+1]);
                }
                if(tilePositions[xPos-1, yPos+1] != null){
                    tmp.Add(tilePositions[xPos-1, yPos+1]);
                }
                break;
            case 4:
                if(tilePositions[xPos+1, yPos+1] != null){
                    tmp.Add(tilePositions[xPos+1, yPos+1]);
                }
                if(tilePositions[xPos-1, yPos] != null){
                    tmp.Add(tilePositions[xPos-1, yPos]);
                }
                break;
            case 5:
                if(tilePositions[xPos-1, yPos] != null){
                    tmp.Add(tilePositions[xPos-1, yPos]);
                }
                break;
        }

        return tmp.ToArray();
    }


    // Set new tile occupied by bandit
    public void SetBanditTile(Tile tile){
        SetCollidersForBanditPlacement(false);
        _currentBanditTile = tile;
    }

    public List<Tile> GetTilesByType(TileType type){
        return tilesByRessourceType[(int) type];
    }

    public List<Tile> GetTilesAroundDesert(){
        var tmp = new List<Tile>();
        tmp.AddRange(tilesAppendentToDesert);
        return tmp;
    }

    public List<Tile> GetTilesAffectedByTornado(){
        var startPoints = new Vector2Int[]{
            new Vector2Int(5,4), // Northern starting point
            new Vector2Int(1,3), // Southern starting point
            new Vector2Int(3,1), // Western starting point
            new Vector2Int(3,5) // Eastern starting point
        };

        int randomStartingPointIndex = UnityEngine.Random.Range(0, 4);
        Vector2Int randomStartingPoint = startPoints[randomStartingPointIndex];

        Tile[] tmp;
        var affectedTiles = new List<Tile>();

        int randomNeighbourIndex;
        string[] neighbourIndices;

        int minimumAmount = 5;

        switch(randomStartingPointIndex){
            case 0: // Northern
                Debug.Log("Tornado from north");

                tmp = GetLowerAppendentTiles(randomStartingPoint.x, randomStartingPoint.y);
                affectedTiles.Add(tilePositions[randomStartingPoint.x, randomStartingPoint.y]);

                while(tmp.Length > 0){
                    if(tmp.Length < 3 && minimumAmount <= 0){
                        float f = (3 - tmp.Length) * 0.33f;
                        float rand = UnityEngine.Random.Range(0f, 1f);
                        if(rand <= f){
                            break;
                        }
                    }

                    minimumAmount--;

                    randomNeighbourIndex = UnityEngine.Random.Range(0, tmp.Length);

                    affectedTiles.Add(tmp[randomNeighbourIndex]);

                    neighbourIndices = tmp[randomNeighbourIndex].gameObject.name.Split("_");

                    tmp = GetLowerAppendentTiles(int.Parse(neighbourIndices[0])+1, int.Parse(neighbourIndices[1])+1);
                }

                break;
            case 1: // Southern
                Debug.Log("Tornado from south");

                tmp = GetUpperAppendentTiles(randomStartingPoint.x, randomStartingPoint.y);
                affectedTiles.Add(tilePositions[randomStartingPoint.x, randomStartingPoint.y]);

                while(tmp.Length > 0){
                    if(tmp.Length < 3 && minimumAmount <= 0){
                        float f = (3 - tmp.Length) * 0.33f;
                        float rand = UnityEngine.Random.Range(0f, 1f);
                        if(rand <= f){
                            break;
                        }
                    }

                    minimumAmount--;

                    randomNeighbourIndex = UnityEngine.Random.Range(0, tmp.Length);

                    affectedTiles.Add(tmp[randomNeighbourIndex]);

                    neighbourIndices = tmp[randomNeighbourIndex].gameObject.name.Split("_");

                    tmp = GetUpperAppendentTiles(int.Parse(neighbourIndices[0])+1, int.Parse(neighbourIndices[1])+1);
                }

                break;
            case 2: // Western
                Debug.Log("Tornado from west");

                tmp = GetRightAppendentTiles(randomStartingPoint.x, randomStartingPoint.y);
                affectedTiles.Add(tilePositions[randomStartingPoint.x, randomStartingPoint.y]);

                while(tmp.Length > 0){
                    if(tmp.Length < 3 && minimumAmount <= 0){
                        float f = (3 - tmp.Length) * 0.33f;
                        float rand = UnityEngine.Random.Range(0f, 1f);
                        if(rand <= f){
                            break;
                        }
                    }

                    minimumAmount--;

                    randomNeighbourIndex = UnityEngine.Random.Range(0, tmp.Length);

                    affectedTiles.Add(tmp[randomNeighbourIndex]);

                    neighbourIndices = tmp[randomNeighbourIndex].gameObject.name.Split("_");

                    tmp = GetRightAppendentTiles(int.Parse(neighbourIndices[0])+1, int.Parse(neighbourIndices[1])+1);
                }

                break;
            case 3: // Southern
                Debug.Log("Tornado from east");

                tmp = GetLeftAppendentTiles(randomStartingPoint.x, randomStartingPoint.y);
                affectedTiles.Add(tilePositions[randomStartingPoint.x, randomStartingPoint.y]);

                while(tmp.Length > 0){
                    if(tmp.Length < 3 && minimumAmount <= 0){
                        float f = (3 - tmp.Length) * 0.33f;
                        float rand = UnityEngine.Random.Range(0f, 1f);
                        if(rand <= f){
                            break;
                        }
                    }

                    minimumAmount--;

                    randomNeighbourIndex = UnityEngine.Random.Range(0, tmp.Length);

                    affectedTiles.Add(tmp[randomNeighbourIndex]);

                    neighbourIndices = tmp[randomNeighbourIndex].gameObject.name.Split("_");

                    tmp = GetLeftAppendentTiles(int.Parse(neighbourIndices[0])+1, int.Parse(neighbourIndices[1])+1);
                }

                break;
        }



        return affectedTiles;
    }

    public List<Tile> GetTilesAffectedByTsunami(){
        // Random amount between 
        var amt = UnityEngine.Random.Range(tilesOnCoast.Length / 2, tilesOnCoast.Length);

        List<Tile> tiles = new List<Tile>();
        tiles.AddRange(tilesOnCoast);

        tiles = ListUtilities.ShuffleList(tiles);

        Debug.Log("Size of tiles list (tsunami) is: " + tiles.Count);

        tiles.RemoveRange(0, tilesOnCoast.Length - amt);

        Debug.Log("Size of tiles list (tsunami) after deleting: " + tiles.Count);

        return tiles;
    }




}
