using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TradingManager : MonoBehaviour
{    
    [SerializeField]
    private Button acceptTradeButton;

    [Header("Wood Trading-Element")]
    [SerializeField]
    private TMPro.TMP_Text woodAmountText;
    [SerializeField]
    private Button woodPlusButton;
    [SerializeField]
    private Button woodSubtractButton;
    
    [Header("Clay Trading-Element")]
    [SerializeField]
    private TMPro.TMP_Text clayAmountText;
    [SerializeField]
    private Button clayPlusButton;
    [SerializeField]
    private Button claySubtractButton;

    
    [Header("Wheat Trading-Element")]
    [SerializeField]
    private TMPro.TMP_Text wheatAmountText;
    [SerializeField]
    private Button wheatPlusButton;
    [SerializeField]
    private Button wheatSubtractButton;

    
    [Header("Sheep Trading-Element")]
    [SerializeField]
    private TMPro.TMP_Text sheepAmountText;
    [SerializeField]
    private Button sheepPlusButton;
    [SerializeField]
    private Button sheepSubtractButton;

    
    [Header("Ore Trading-Element")]
    [SerializeField]
    private TMPro.TMP_Text oreAmountText;
    [SerializeField]
    private Button orePlusButton;
    [SerializeField]
    private Button oreSubtractButton;

    // Button collections
    // Important: Add Buttons in order of RessourceEnum in RessourcesManager
    private Button[] receiveButtons; // +
    private Button[] discardButtons; // -

    // Ressource amount collection
    // Important: Add text fields in order of RessourceEnum in RessourceManager
    private TMPro.TMP_Text[] ressourceAmountTexts;


    // Conversion rate for each ressource in type-order
    private int[] playerConversionRates;
    // Ressources of the player before opening trade panel (or after a completed trade and beginning a new one)
    private Dictionary<int, int> initialPlayerRessources;
    // Dynamic state of player ressources modified by trade decisions (e.g. after choosing the discarding ressource)
    private Dictionary<int, int> currentPlayerRessources; 

    // Discarding/Receiving Ressource
    private RessourcesManager.RessourceType discardingRessource;
    private RessourcesManager.RessourceType receivingRessource;

    private bool receivingRessourceIsChosen;
    private bool discardingRessourceIsChosen;

    // Start is called before the first frame update
    void Start()
    {
        // Remember: Ressources enum is in the following order: Wood, Clay, Wheat, Sheep, Ore (, None)
        receiveButtons = new Button[]{woodPlusButton, clayPlusButton, wheatPlusButton, sheepPlusButton, orePlusButton};
        discardButtons = new Button[]{woodSubtractButton, claySubtractButton, wheatSubtractButton, sheepSubtractButton, oreSubtractButton};
        ressourceAmountTexts = new TMPro.TMP_Text[]{woodAmountText, clayAmountText, wheatAmountText, sheepAmountText, oreAmountText};

        this.gameObject.SetActive(false);
    }

    // TODO: for multiplayer/singleplayer with ai it is preferable to not use the current player of the manager but THIS player
    // Before each trade, get the players ressource and conversion rates. These can change between multiple trades in a single turn
    public void InitialisePanel(){
        this.gameObject.SetActive(true);

        // Set player ressources and their conversion rates
        initialPlayerRessources = CloneDictionary(PlayerManager.instance.CurrentPlayer.PlayerRessources.AmountPerRessourceType);
        currentPlayerRessources = CloneDictionary(PlayerManager.instance.CurrentPlayer.PlayerRessources.AmountPerRessourceType);
        playerConversionRates = PlayerManager.instance.CurrentPlayer.PlayerRessources.RessourceConversionRates;

        discardingRessourceIsChosen = false;
        receivingRessourceIsChosen = false;

        // Set UI texts
        SetRessourceAmountTextsWithColorReset(initialPlayerRessources, true);

        // Disable all plus buttons from interaction, allow subtract buttons only if conversion rate is satisfied
        SetReceiveButtonsInteractable(false);
        SetDiscardButtonsInteractableIfConversionRateIsSatisfied();
        
        acceptTradeButton.interactable = false;
    }

    // TODO: extend dictionary file OR put into utility file OR to player ressource
    private Dictionary<int, int> CloneDictionary(Dictionary<int, int> original){
        var newDict = new Dictionary<int, int>();
        foreach(KeyValuePair<int, int> kv in original){
            newDict.Add(kv.Key, kv.Value);
        }
        return newDict;
    }

    public void UIButtonDiscardRessourceClicked(int ressourceAsInt){
        int updatedRessourceAmount;
            

        // Player wants to annulate the receiving ressource?
        if(receivingRessourceIsChosen && ((int) receivingRessource == ressourceAsInt)){
            if(discardingRessourceIsChosen){
                SetReceiveButtonsInteractableWithException(true, (int) discardingRessource);
            } else {
                SetReceiveButtonsInteractableWithException(true);
            }
            receivingRessourceIsChosen = false;
            receivingRessource = RessourcesManager.RessourceType.NONE;

            updatedRessourceAmount = currentPlayerRessources[ressourceAsInt]-1;
            
            ressourceAmountTexts[ressourceAsInt].text = updatedRessourceAmount.ToString();
            currentPlayerRessources[ressourceAsInt] = updatedRessourceAmount;

            acceptTradeButton.interactable = false;
            
            return;
        }


        // Log error if less ressources than necessary are available for trade
        if(currentPlayerRessources[ressourceAsInt] - playerConversionRates[ressourceAsInt] < 0){
            Debug.LogError("Trying to discard more ressources than available for a trade! No action will be taken.");
            return;
        }
        // Modify the amount of the ressource displayed
        updatedRessourceAmount = currentPlayerRessources[ressourceAsInt] - playerConversionRates[ressourceAsInt];

        ressourceAmountTexts[ressourceAsInt].SetText(updatedRessourceAmount.ToString());
        ressourceAmountTexts[ressourceAsInt].color = Color.red;
        
        currentPlayerRessources[ressourceAsInt] = updatedRessourceAmount;

        discardingRessource = (RessourcesManager.RessourceType) ressourceAsInt;
        discardingRessourceIsChosen = true;

        currentPlayerRessources[ressourceAsInt] = updatedRessourceAmount;

        // Enable buttons to choose receiving ressource
        SetReceiveButtonsInteractableWithException(true, (int) discardingRessource);
        SetDiscardButtonsInteractable(false);
        acceptTradeButton.interactable = discardingRessourceIsChosen && receivingRessourceIsChosen;
    }

    public void UIButtonPerformTradeClicked(){
        if(!(discardingRessourceIsChosen && receivingRessourceIsChosen)){
            Debug.LogError(string.Format("Ressource to discard ({0}) or receive ({1}) has not been chosen. Proceed without action.", discardingRessourceIsChosen, receivingRessourceIsChosen));
            return;
        }
        if(receivingRessource == RessourcesManager.RessourceType.NONE){
            Debug.LogError("Trying to receive ressource of type \"NONE\"! Proceeding without action.");
            return;
        }
        PlayerRessources playerRessourcesComponent = GetComponentInParent<PlayerRessources>();

        // Discard ressources
        playerRessourcesComponent.SetRessourceToAmount(discardingRessource, currentPlayerRessources[(int) discardingRessource]);

        // Add ressources
        playerRessourcesComponent.SetRessourceToAmount(receivingRessource, currentPlayerRessources[(int) receivingRessource]);

        // Initiliase panel for new trade
        InitialisePanel();
    }

    public void UIButtonClosePanelClicked(){
        this.gameObject.SetActive(false);
    }

    public void UIButtonReceiveRessourceClicked(int ressourceAsInt){
        int updatedRessourceAmount;

        if(discardingRessourceIsChosen && (((int) discardingRessource) == ressourceAsInt)){
            discardingRessourceIsChosen = false;
            discardingRessource = RessourcesManager.RessourceType.NONE;

            updatedRessourceAmount = currentPlayerRessources[ressourceAsInt] + playerConversionRates[ressourceAsInt];

            ressourceAmountTexts[ressourceAsInt].text = updatedRessourceAmount.ToString();
            currentPlayerRessources[ressourceAsInt] = updatedRessourceAmount;

            acceptTradeButton.interactable = false;
            SetReceiveButtonsInteractable(false);
            SetDiscardButtonsInteractable(true);

            return;
        }

        updatedRessourceAmount = currentPlayerRessources[ressourceAsInt] + 1;

        ressourceAmountTexts[ressourceAsInt].SetText(updatedRessourceAmount.ToString());
        ressourceAmountTexts[ressourceAsInt].color = Color.green;
        
        currentPlayerRessources[ressourceAsInt] = updatedRessourceAmount;

        receivingRessource = (RessourcesManager.RessourceType) ressourceAsInt;
        receivingRessourceIsChosen = true;

        //SetSubtractButtonsInteractableIfConversionRateSatisfied();
        SetReceiveButtonsInteractable(false);
        SetDiscardButtonsInteractableWithException(false, (int) receivingRessource);
        
        acceptTradeButton.interactable = discardingRessourceIsChosen && receivingRessourceIsChosen;
    }

    void SetRessourceAmountTextsWithColorReset(Dictionary<int, int> ressourceAmountPerType, bool resetColors = false){
        for(int index = 0; index < ressourceAmountPerType.Keys.Count; ++index){
            ressourceAmountTexts[index].SetText(ressourceAmountPerType[index].ToString());

            if(resetColors)
                ressourceAmountTexts[index].color = Color.black;
        }
    }

    void SetReceiveButtonsInteractableWithException(bool isInteractable, int exceptionIndex=-1){
        for(int index = 0; index < receiveButtons.Length; ++index){
            if(exceptionIndex >= 0 && exceptionIndex == index){
                receiveButtons[index].interactable = !isInteractable;
                continue;
            }
            receiveButtons[index].interactable = isInteractable;
        }
    }

    void SetDiscardButtonsInteractableIfConversionRateIsSatisfied(){
        for(int index = 0; index < discardButtons.Length; ++index){
            discardButtons[index].interactable = initialPlayerRessources[index] >= playerConversionRates[index];
        }
    }

    void SetDiscardButtonsInteractable(bool interactable){
        Array.ForEach(discardButtons, button => button.interactable = interactable);
    }
    
    void SetDiscardButtonsInteractableWithException(bool interactable, int exceptionIndex=-1){
        for(int index = 0; index < discardButtons.Length; ++index){
            if(exceptionIndex >= 0 && exceptionIndex == index){
                discardButtons[index].interactable = !interactable;
                continue;
            }
            discardButtons[index].interactable = interactable;
        }
    }

    void SetReceiveButtonsInteractable(bool interactable){
        Array.ForEach(receiveButtons, button => button.interactable = interactable);
    }

    public int GetCurrentRessourceAmount(int ressourceAsInt){
        return currentPlayerRessources[ressourceAsInt];
    }
}
