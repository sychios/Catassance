using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RessourcesManager : MonoBehaviour
{
    public enum RessourceType:int{
        WOOD,
        CLAY,
        WHEAT,
        SHEEP,
        ORE,
        NONE // Necessary? for desert?
    }

    // Store ressources necessary for structure:
    // Road: 1 Clay, 1 Wood
    // Village: 1 Clay, 1 Wood, 1 Wheat, 1 Sheep
    // Town: 2 Wheat, 3 Ore
    // (DefenseTower): 2 Ore, 1 Wood, 1 Clay
    // Development Card: 1 Wheat, 1 Sheep, 1 Ore
    
    // Road
    public Dictionary<RessourceType, int> RessourceAmountPerRoad {get; private set;} = new Dictionary<RessourceType, int>{
        {RessourceType.CLAY, 1},
        {RessourceType.WOOD, 1}
    };
    // Village
    public Dictionary<RessourceType, int> RessourceAmountPerVillage {get; private set;} = new Dictionary<RessourceType, int>{
        {RessourceType.CLAY, 1},
        {RessourceType.WOOD, 1},
        {RessourceType.WHEAT, 1},
        {RessourceType.SHEEP, 1}
    };
    // Town
    public Dictionary<RessourceType, int> RessourceAmountPerTown {get; private set;} = new Dictionary<RessourceType, int>{
        {RessourceType.WHEAT, 2},
        {RessourceType.ORE, 3}
    };
    // Development Card
    public Dictionary<RessourceType, int> RessourceAmountPerDevelopmentCard {get; private set;} = new Dictionary<RessourceType, int>{
        {RessourceType.WHEAT, 1},
        {RessourceType.SHEEP, 1},
        {RessourceType.ORE, 1}
    };

    public static RessourcesManager instance;

    // Start is called before the first frame update
    void Start()
    {
        if(instance){
            Debug.LogError("Multiple occurences of ressources manager found on " + gameObject.name);
            Destroy(this);
            return;
        }

        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
