using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuBoardManager : MonoBehaviour
{
    // Delay between each spawn of a tile
    [Range(0.01f, 2f)]
    [SerializeField] private float tileSpawnDelayInSeconds = 0.5f;

    // GameObject containing the spawnpoints of the tiles as children
    [SerializeField] private GameObject tileSpawnpointsParent;

    // Water gameobject surrounding the island
    [SerializeField] private GameObject surroundingWater;

    // Tile prefabs to be set from editor
    [SerializeField] private GameObject forestPrefab;
    [SerializeField] private GameObject pasturePrefab;
    [SerializeField] private GameObject farmPrefab;
    [SerializeField] private GameObject claypitPrefab;
    [SerializeField] private GameObject mountainPrefab;
    [SerializeField] private GameObject desertPrefab;

    // Array later on containing the prefabs. 
    // Is not made a serialize-field to keep prefabs in specific order
    private GameObject[] tilePrefabsArray;

    // List containing the frequence of each tile-type, here in form of their int-representation, appearing on the board
    // TODO: framerate dropping rapidly when spawning farms (2 as int)
    // target frequency distribution is: {0,0,0,0,1,1,1,1,2,2,2,2,3,3,3,4,4,4,5}
    private List<int> tileTypeFrequencies = new List<int>{0,0,0,0,1,1,1,1,3,4,3,1,3,3,3,4,4,4,5};

    // Start is called before the first frame update
    void Start()
    {
        tilePrefabsArray = new GameObject[6] {forestPrefab, pasturePrefab, farmPrefab, claypitPrefab, mountainPrefab, desertPrefab};

        StartCoroutine(SpawnMockUpBoard());
    }

    // Spawn tiles but remove elements such as the VisualEffects, as in the menu scene a broken down version of the board is shown
    private IEnumerator SpawnMockUpBoard(){

        var shuffledTileList = ListUtilities.ShuffleList(tileTypeFrequencies);

        if(tileSpawnpointsParent.transform.childCount != shuffledTileList.Count){
            Debug.LogError("Different length between number of spawnpoints and tiles to be spawned. No further action taking place.");
            yield return null;
        }

        List<Transform> spawnpoints = new List<Transform>();
        foreach(Transform child in tileSpawnpointsParent.transform){
            spawnpoints.Add(child);
        }

        for(int index = 0; index < shuffledTileList.Count; ++index){
            SpawnTile(spawnpoints[index].position, shuffledTileList[index]);
            yield return new WaitForSeconds(tileSpawnDelayInSeconds);
        }

        surroundingWater.SetActive(true);
    }

    private void SpawnTile(Vector3 tilePosition, int tileTypeAsInt){
            var currentTile = Instantiate(tilePrefabsArray[tileTypeAsInt], tilePosition, Quaternion.identity);
            currentTile.GetComponent<TileVisualEffects>().HideNumberText();
    }
}
