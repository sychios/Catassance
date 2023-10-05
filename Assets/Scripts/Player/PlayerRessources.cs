using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerRessources : MonoBehaviour
{
    private PlayerUI playerUI;

    private Player correspondingPlayer;

    // Holds the conversion rate for which players can trade some of their cards against another one(s)
    public int[] RessourceConversionRates {get; private set;}

    // Base conversion rate: 4 ressource can be traded for one ressource of choice
    [SerializeField]
    private int baseConversionRate = 4;

    // Holds the amount of ressource player owns
    public Dictionary<int, int> AmountPerRessourceType {get; private set;} = new Dictionary<int, int>();

    void Start(){
        // Setup array for conversion rates
        RessourceConversionRates = new int[Enum.GetNames(typeof(RessourcesManager.RessourceType)).Length-1];

        playerUI = GetComponent<PlayerUI>();
        correspondingPlayer = GetComponent<Player>();


        int ressourceAsInt;
        // Declare dictionary to hold the amount for each ressource, where (int) ressource is used as key
        // and the ressource-amount as value. Along the conversion-rate array is filled with values
        foreach(RessourcesManager.RessourceType ressource in Enum.GetValues(typeof(RessourcesManager.RessourceType))){
            if(ressource == RessourcesManager.RessourceType.NONE)
                continue;
            ressourceAsInt = (int) ressource;
            AmountPerRessourceType.Add(ressourceAsInt, 0);
            RessourceConversionRates[ressourceAsInt] = baseConversionRate;
        }
    }

    int GetTotalAmountOfRessources(){
        var amount = 0;
        int ressourceAsInt;
        foreach(RessourcesManager.RessourceType ressource in Enum.GetValues(typeof(RessourcesManager.RessourceType))){
            if(ressource == RessourcesManager.RessourceType.NONE){
                continue;
            };
            ressourceAsInt = (int) ressource;
            amount = AmountPerRessourceType[ressourceAsInt];
        }

        return amount;
    }

    public bool IsRessourceAmountSatisfied(Dictionary<RessourcesManager.RessourceType, int> necessaryRessources){
        int ressourceAsInt;
        foreach(RessourcesManager.RessourceType ressource in necessaryRessources.Keys){
            ressourceAsInt = (int) ressource;
            // TODO: remember all ressources with shortage, and make little highlight (red numbers/red "ring" around ressource?) on player canvas
            if(AmountPerRessourceType[ressourceAsInt] < necessaryRessources[ressource])
                return false;
        }

        return true;
    }

    public void AddRessource(RessourcesManager.RessourceType ressource, int amountToAdd){
        AmountPerRessourceType[(int) ressource] += amountToAdd;
        playerUI.UpdateRessourceAmount(ressource, AmountPerRessourceType[(int) ressource]);
        correspondingPlayer.ShowBuildableStructuresIfRessourcesSatisfied();
    }

    // TODO: can be deleted?
    public void AddRessources(Dictionary<RessourcesManager.RessourceType, int> ressourcesToAdd){
        foreach(RessourcesManager.RessourceType ressource in ressourcesToAdd.Keys){
            AddRessource(ressource, ressourcesToAdd[ressource]);
        }
    }

    // Discard amount for given ressource. If ressource amount should drop below 0 an error is logged.
    public void DiscardRessource(RessourcesManager.RessourceType ressource, int amountToDiscard){
        int ressourceAsInt = (int) ressource;
        int currentAmount = AmountPerRessourceType[ressourceAsInt];

        if(amountToDiscard > currentAmount){
            Debug.LogError(
                string.Format("Tried to discard more ressources than available on Player <{0}> for ressource {1}", 
                    this.GetComponent<Player>().PlayerName, ressource));
        }

        //AmountPerRessourceType[ressourceAsInt] = Mathf.Max(currentAmount - amountToDiscard, 0);
        AmountPerRessourceType[ressourceAsInt] = currentAmount - amountToDiscard;
        playerUI.UpdateRessourceAmount(ressource, AmountPerRessourceType[ressourceAsInt]);
        correspondingPlayer.ShowBuildableStructuresIfRessourcesSatisfied();
    }

    public void DiscardRessources(Dictionary<RessourcesManager.RessourceType, int> ressourcesToDiscard){
        int ressourceAsInt;
        int currentRessourceAmount;
        int amountToDiscard;

        foreach(RessourcesManager.RessourceType ressource in ressourcesToDiscard.Keys){
            ressourceAsInt = (int) ressource;
            currentRessourceAmount = AmountPerRessourceType[ressourceAsInt];
            amountToDiscard = ressourcesToDiscard[ressource];

            if(amountToDiscard > currentRessourceAmount){
                Debug.LogError(
                    string.Format("Tried to discard more ressources than available on Player <{0}> for ressource {1}", 
                        this.GetComponent<Player>().PlayerName, ressource));
            }
            
            AmountPerRessourceType[ressourceAsInt] = Mathf.Max(currentRessourceAmount - amountToDiscard, 0);
            playerUI.UpdateRessourceAmount(ressource, AmountPerRessourceType[ressourceAsInt]);
        }
        correspondingPlayer.ShowBuildableStructuresIfRessourcesSatisfied();
    }

    // Shuffles the ressource indices and walks through them.
    // As soon as ressource amount is greater than 0, reduce amount by 1 and return.
    public void DiscardRandomRessource(){
        List<int> ressourceTypesAsInt = AmountPerRessourceType.Keys.ToList();
        ressourceTypesAsInt = ListUtilities.ShuffleList(ressourceTypesAsInt);

        for(int index = 0; index < ressourceTypesAsInt.Count; ++index){
            if(AmountPerRessourceType[ressourceTypesAsInt[index]] > 0){
                DiscardRessource((RessourcesManager.RessourceType) ressourceTypesAsInt[index], 1);
                //AmountPerRessourceType[keys[index]] -= 1;
                //PlayerUI.UpdateRessourceAmount((RessourcesManager.RessourceType) keys[index], AmountPerRessourceType[keys[index]]);
                //player.ShowBuildableStructuresIfRessourcesSatisfied();
                return;
            }
        }
    }

    public void SetRessourceToAmount(RessourcesManager.RessourceType ressource, int amount){
        if(amount < 0){
            Debug.LogError("Cannot set amount of ressource below zero. Proceeding without action.");
            return;
        }

        AmountPerRessourceType[(int) ressource] = amount;
        playerUI.UpdateRessourceAmount(ressource, amount);
        correspondingPlayer.ShowBuildableStructuresIfRessourcesSatisfied();
    }

    // TODO: can be deleted?
    public void SetRessourcesToAmount(Dictionary<RessourcesManager.RessourceType, int> amountPerRessource){
        foreach(RessourcesManager.RessourceType ressource in amountPerRessource.Keys){
            SetRessourceToAmount(ressource, amountPerRessource[ressource]);
        }
    }

    // When the bandit gets activated, players with more than 7 cards must discard half of their ressorces
    public void BanditRessourcePileDiscarding(){
        int ressourceAmount = GetTotalAmountOfRessources();
        if(!(ressourceAmount > 7))
            return;

        int amountToDiscard = (ressourceAmount % 2) == 1 ? (ressourceAmount+1) / 2 :  ressourceAmount / 2;
        
        for(int index = amountToDiscard; index > 0; --index){
            DiscardRandomRessource();
        }
    }
}
