using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuBoardManager : MonoBehaviour
{
    // GameObject containing the spawnpoints of the tiles as children
    [SerializeField] private GameObject tileSpawnpointsParent;

    [SerializeField] private GameObject surroundingWater;

    [SerializeField] private GameObject ForestPrefab;
    [SerializeField] private GameObject PasturePrefab;
    [SerializeField] private GameObject FarmPrefab;
    [SerializeField] private GameObject ClaypitPrefab;
    [SerializeField] private GameObject MountainPrefab;
    [SerializeField] private GameObject DesertPrefab;

    private GameObject[] tilePrefabsArray;

    // Start is called before the first frame update
    void Start()
    {
        tilePrefabsArray = new GameObject[6] {ForestPrefab, PasturePrefab, FarmPrefab, ClaypitPrefab, MountainPrefab, DesertPrefab};

        StartCoroutine(SpawnMockUpBoard());
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private List<int> GenerateShuffledListOfTileTypes(){
        // List containing the frequence of types, here by their int-representation, appearing on the board
        List<int> tileTypes = new List<int> {0,0,0,0,1,1,1,1,2,2,2,2,3,3,3,4,4,4,5};

        // Shuffle tileTypesList
        tileTypes = ListUtilities.ShuffleList(tileTypes);

        return tileTypes;
    }

    // Spawn tiles but remove elements such as the VisualEffects, as in the menu scene a broken down version of the board is shown
    private IEnumerator SpawnMockUpBoard(){

        var shuffledTileList = GenerateShuffledListOfTileTypes();

        if(tileSpawnpointsParent.transform.childCount != shuffledTileList.Count){
            Debug.LogError("Different length between number of spawnpoints and tiles to be spawned. No further action taking place.");
            yield return null;
        }


        Vector3 currentTilePosition;
        int currentTileTypeAsInt;
        GameObject currentTile;

        List<Transform> spawnpoints = new List<Transform>();
        foreach(Transform child in tileSpawnpointsParent.transform){
            spawnpoints.Add(child);
        }

        for(int index = 0; index < shuffledTileList.Count; ++index){
            currentTilePosition = spawnpoints[index].position;
            currentTileTypeAsInt = shuffledTileList[index];
            currentTile = Instantiate(tilePrefabsArray[currentTileTypeAsInt], currentTilePosition, Quaternion.identity);
            currentTile.GetComponent<TileVisualEffects>().HideNumberText();

            yield return new WaitForSeconds(0.5f);
        }

        surroundingWater.SetActive(true);
    }
}
