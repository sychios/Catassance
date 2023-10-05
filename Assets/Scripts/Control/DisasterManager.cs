
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class DisasterManager : MonoBehaviour
{
    private class Disaster{
        public string name{get; private set;}
        public readonly int disasterID;
        public readonly int duration;
        public readonly bool grantsImmunity;
        public readonly float probability;
    
        public Disaster(int ID, string pName, int pDuration, bool pImmunity, float pProbability){
            disasterID = ID;
            name = pName;
            duration = pDuration;
            grantsImmunity = pImmunity;
            probability = pProbability;
        }
    }
    
    private Disaster[] disasters = new Disaster[]{
        new Disaster(0, "Wolfsrudel", 3, false, 0.16f), // 0.16
        new Disaster(1, "Borkenkäfer", 3, false, 0.16f),// 0.16
        new Disaster(2, "Schimmelpilz", 3, false, 0.16f),// 0.16
        new Disaster(3, "Vulkanausbruch", 4, true, 0.12f), // 0.12
        new Disaster(4, "Grubenunglück", 4, true, 0.12f), // 0.12
        new Disaster(5, "Sandsturm", 5, true, 01f), // 0.1
        new Disaster(6, "Tornado", 6, true, 0.09f), // 0.09
        new Disaster(7, "Tsunami", 6, true, 0.09f) // 0.09
    };

    /* 
        Occuring environmental disasters:
        - name, affected tiles, duration, immunity of affected tiles after disaster ended, probability

        Wolfsrudel:     Jedes Weideland,     3 Züge, keine Immunität,  0.18
        Borkenkäfer:    Jedes Waldstück,     3 Züge, keine Immunität,  0.18
        Schimmelpilz:   Jedes Weizenfeld,    3 Züge, keine Immunität,  0.18
        Vulkanausbruch: Einzelnes Eisenfeld, 4 Züge, danach Immunität, 0.13
        Grubenunglück:  Einzelnes Lehmfeld,  4 Züge, danach Immunität, 0.13

        Sandsturm:      Ringförmig um die Wüste,  4 Züge, danach Immunität, 0.10
        Tornado:        Spur durchs Spielfeld,    5 Züge, danach Immunität, 0.05
        Tsunami:        Vereinzelte Küstenfelder, 5 Züge, danach Immunität, 0.05
    */

    // TODO: Modify later, depending on whether disaster occure with probability or everytime
    private float overallDisasterProbability = 1f;

    private readonly int lowerBoundTurnsBetweenDisasters =  4;

    private readonly int upperBoundTurnsBetweenDisasters = 8;
    private int turnsUntilNextDisaster = 4;

    private Disaster currentDisaster;
    private int currentDisasterTurnsLeft;

    private bool disasterIsActive = false;

    private List<Tile> currentAffectedTiles;

    private BoardManager boardManager;

    public static DisasterManager instance;

    void Awake(){
        if(instance){
            Debug.LogError("Multiple instances of DisasterManager found on " + gameObject.name + ".");
            Destroy(this);
            return;
        }
        
        instance = this;
    }

    void Start(){
        boardManager = BoardManager.Instance;
    }

    // TODO: rename
    // "game-loop" of the disaster manager
    // called every time a player turn ended and before next player starts
    public void ProcessGameTurn(){
        // if disaster is currently active, reduce duration. If there is no duration left, remove disaster effects and set "break" between disasters
        if(disasterIsActive){
            --currentDisasterTurnsLeft;
            if(currentDisasterTurnsLeft < 0){
                RemoveCurrentDisasterEffects();
                turnsUntilNextDisaster = GetRandomTurnsBetweenDisasters();
            }
        } else {
            // If no disaster is active, reduce timer between disasters and initialise new disaster, if timer ends
            --turnsUntilNextDisaster;
            if(turnsUntilNextDisaster == 0){
                InitialiseDisaster();
            }
        }

        PlayerManager.instance.SetDisasterIsProcessed(true);
    }


    void InitialiseDisaster(){
        float rand = Random.Range(0f, 1f);

        // Does a disaster occure? 
        // When rand is smaller/equal probability, initialise disaster, otherwise wait again for a few moves
        if(rand > overallDisasterProbability){
            turnsUntilNextDisaster = GetRandomTurnsBetweenDisasters();
            return;
        }

        // If no disaster was active get random disaster, else repeat random selection if previous disaster would be repeated
        if(currentDisaster == null){
            currentDisaster = GetRandomDisaster();
        } else {

            var previousDisaster = currentDisaster;
            while(currentDisaster == previousDisaster){
                currentDisaster = GetRandomDisaster();
            }
        }

        ApplyCurrentDisasterEffects();
    }

    void ApplyCurrentDisasterEffects(){
        Debug.Log(string.Format("Applying new disaster: {0}", currentDisaster.name));

        /* 
        new Disaster(0, "Wolfsrudel", 3, false, 0.18f),
        new Disaster(1, "Borkenkäfer", 3, false, 0.18f),
        new Disaster(2, "Schimmelpilz", 3, false, 0.18f),
        new Disaster(3, "Vulkanausbruch", 4, true, 0.13f),
        new Disaster(4, "Grubenunglück", 4, true, 0.13f),
        new Disaster(5, "Sandsturm", 5, true, 0.1f),
        new Disaster(6, "Tornado", 6, true, 0.05f),
        new Disaster(7, "Tsunami", 6, true, 0.05f)*/

        switch(currentDisaster.disasterID){
            case 0: 
                    currentAffectedTiles = boardManager.GetTilesByType(BoardManager.TileType.PASTURE);
                break;
            case 1:

                    currentAffectedTiles = boardManager.GetTilesByType(BoardManager.TileType.FOREST);
                break;
            case 2:

                    currentAffectedTiles = boardManager.GetTilesByType(BoardManager.TileType.FARM);
                break;    
            case 3:

                    currentAffectedTiles = boardManager.GetTilesByType(BoardManager.TileType.MOUNTAIN);
                break;    
            case 4:

                    currentAffectedTiles = boardManager.GetTilesByType(BoardManager.TileType.CLAYPIT);
                break;    
            case 5:
                    // Sandsturm
                    currentAffectedTiles = boardManager.GetTilesAroundDesert();
                break;    
            case 6:
                    // Tornado
                    currentAffectedTiles = boardManager.GetTilesAffectedByTornado();
                break;    
            case 7:
                    // Tsunami
                    currentAffectedTiles = boardManager.GetTilesAffectedByTsunami();
                break;
            default:
                Debug.LogError("Critical! Actions for the ID of current disaser is not defined.");
                turnsUntilNextDisaster = GetRandomTurnsBetweenDisasters();
                return;
        }

        StartCoroutine(ProhibitTilesFromYielding(currentAffectedTiles));

        /*for(int index = 0; index < currentAffectedTiles.Count; ++index){
            currentAffectedTiles[index].IsYieldingProhibitedByDisaster(true);
        }
        
        currentDisasterTurnsLeft = currentDisaster.duration;
        
        disasterIsActive = true;*/
    }

    IEnumerator ProhibitTilesFromYielding(List<Tile> tiles){
        for(int index = 0; index < currentAffectedTiles.Count; ++index){
            currentAffectedTiles[index].IsYieldingProhibitedByDisaster(true);
            yield return new WaitForSeconds(1f);
        }
        
        currentDisasterTurnsLeft = currentDisaster.duration;
        
        disasterIsActive = true;
    }

    void RemoveCurrentDisasterEffects(){
        Debug.Log("Removing current disaster effects.");
        
        for(int index = 0; index < currentAffectedTiles.Count; ++index){
            currentAffectedTiles[index].IsYieldingProhibitedByDisaster(false);
        }

        disasterIsActive = false;
    }

    

    


    #region Getter/Setter

    int GetRandomTurnsBetweenDisasters(){
        return Random.Range(lowerBoundTurnsBetweenDisasters, upperBoundTurnsBetweenDisasters);
    }

    #endregion

    #region Utilities

    Disaster GetRandomDisaster(){
        float rand = Random.Range(0f,1f);
        float addedProbability = 0;

        for(int index = 0; index < disasters.Length; ++index){
            addedProbability += disasters[index].probability;

            if(addedProbability >= rand)
                return disasters[index];
        }

        // if for-loop doesnt return a disaster, return the last one
        Debug.LogError("Could get a random disaster. Per default the last disaster in list is returned.");
        return disasters[disasters.Length-1];
    }

    #endregion
}
