using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public PlayerRessources PlayerRessources {get; private set;}



    [Header("UI-Elements")]
    [SerializeField]
    private Canvas PlayerCanvas;
    [SerializeField]
    private Button CastDiceButton;
    [SerializeField]
    private Button FinishTurnButton;
    [SerializeField]
    private Button TradeButton;

    [SerializeField]
    private TMPro.TMP_Text nameText;
    [SerializeField]
    private TMPro.TMP_Text diceResultText;
    [SerializeField]
    private GameObject diceResultBackground;
    [SerializeField]
    private GameObject diceResult;

    [SerializeField]
    private GameObject ressourceRow;


    [SerializeField]
    private GameObject ressourcesContainer;

    [SerializeField]
    private TMPro.TMP_Text woodAmountText;
    [SerializeField]
    private TMPro.TMP_Text clayAmountText;
    [SerializeField]
    private TMPro.TMP_Text wheatAmountText;
    [SerializeField]
    private TMPro.TMP_Text sheepAmountText;
    [SerializeField]
    private TMPro.TMP_Text oreAmountText;
    [SerializeField]
    private TMPro.TMP_Text noRessourceText;


    [SerializeField]
    private TMPro.TMP_Text playerScoreText;

    [SerializeField]
    private TMPro.TMP_Text gameFinishedText;
    [SerializeField]
    private GameObject gameFinishedTextContainer;


    [SerializeField]
    private GameObject developmentCardsContainer;
    [SerializeField]
    private GameObject developmentCardPrefab;
    private int developmentCardsAmount = 0;

    [SerializeField]
    private GameObject developmentCardDetailsContainer;

    private TMPro.TMP_Text cardDetailsHeader;
    private TMPro.TMP_Text cardDetailsDescription;
    private TMPro.TMP_Text cardDetailsActionDescription;


    // TODO: implement so that arr[(int) ressource] returns textfield of the specific ressource, better than switch-casing through it
    // TODO: also rename array
    private TMPro.TMP_Text[] textFieldArray;

    private List<GameObject> developmentCards = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        PlayerRessources = GetComponent<PlayerRessources>();

        // Set references to textfields of ressources-amounts
        textFieldArray  = new TMPro.TMP_Text[System.Enum.GetValues(typeof(RessourcesManager.RessourceType)).Length];
        textFieldArray[(int) RessourcesManager.RessourceType.WOOD] = woodAmountText;
        textFieldArray[(int) RessourcesManager.RessourceType.CLAY] = clayAmountText;
        textFieldArray[(int) RessourcesManager.RessourceType.WHEAT] = wheatAmountText;
        textFieldArray[(int) RessourcesManager.RessourceType.SHEEP] = sheepAmountText;
        textFieldArray[(int) RessourcesManager.RessourceType.ORE] = oreAmountText;
        textFieldArray[(int) RessourcesManager.RessourceType.NONE] = noRessourceText;

        // Deactivate the game ending overlay
        gameFinishedTextContainer.SetActive(false);

        // Deactivate the development card details overlay
        developmentCardDetailsContainer.SetActive(false);

        // Set references to the elements of development card details
        foreach(Transform child in developmentCardDetailsContainer.transform){
            if(child.name == "Header")
                cardDetailsHeader = child.GetComponent<TMPro.TMP_Text>();
            if(child.name == "Description")
                cardDetailsDescription = child.GetComponent<TMPro.TMP_Text>();
            if(child.name == "ActionDescription")
                cardDetailsActionDescription = child.GetComponent<TMPro.TMP_Text>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateRessourceAmount(RessourcesManager.RessourceType ressource, int amount){
        if((int) ressource >= textFieldArray.Length){
            Debug.LogError("Ressource amount cannot be changed, as ressource index is out of bounds.");
            return;
        }
        textFieldArray[(int) ressource].SetText(amount.ToString());         
    }

    public void SetRessourcesVisibility(bool visible){
        ressourceRow.SetActive(visible);
    }

    public void SetCanvasVisibility(bool visible){
        //PlayerCanvas.gameObject.SetActive(false);
        PlayerCanvas.enabled = visible;
    }

    public void SetDiceButtonEnabled(bool enable){
        CastDiceButton.enabled = enable;
    }

    public void SetDiceButtonInteractable(bool interactable){
        CastDiceButton.interactable = interactable;
    }

    public void SetTradeButtonActiveAndInteractable(bool active, bool interactable){
        TradeButton.gameObject.SetActive(active);
        TradeButton.interactable = interactable;
    }

    public void SetFinishTurnButtonEnabled(bool enable){
        FinishTurnButton.enabled = enable;
    }

    public void SetFinishTurnButtonInteractable(bool interactable){
        FinishTurnButton.interactable = interactable;
    }

    public void SetDiceResultText(string text){
        diceResultText.SetText(text);        
    }

    public void SetNameText(string text){
        nameText.SetText(text);
    }

    public void SetNameTextColor(Color color){
        nameText.color = color;
    }

    public void SetPlayerScore(int score){
        if(score < 0){
            Debug.LogError("Negative player score is not possible. No actions done.");
            return;
        }

        if(score == int.Parse(playerScoreText.text)){
            Debug.LogError("New player score is equal to new score. No actions done.");
            return;
        }

        playerScoreText.SetText(score.ToString());
    }

    public void DisplayGameFinished(bool playerHasWon){
        ressourcesContainer.SetActive(false);
        SetDiceButtonEnabled(false);
        SetFinishTurnButtonEnabled(false);
        diceResultBackground.SetActive(false);
        diceResult.SetActive(false);

        gameFinishedTextContainer.SetActive(true);
        
        if(playerHasWon){
            gameFinishedText.SetText("You won!");
            gameFinishedText.color = Color.green;
        } else {
            gameFinishedText.SetText("You lost!");
            gameFinishedText.color = Color.red;
        }
    }

    public void UpdateDevelopmentCardsContainer(DevelopmentCardsManager.DevelopmentCardType developmentCard){
        if(developmentCard == DevelopmentCardsManager.DevelopmentCardType.NONE)
            return;

        GameObject newCard = Instantiate(developmentCardPrefab, developmentCardsContainer.transform);
        var devCardComponent = newCard.GetComponent<UIDevelopmentCard>();
        devCardComponent.SetPlayerUI(this);

        devCardComponent.SetColorAndType(developmentCard);
        devCardComponent.SetTitle(developmentCard.ToString());

        // append newest card
        developmentCards.Add(newCard);

        // horizonal space between cards
        Vector3 horizontalCardOffset = developmentCards.Count <= 4? new Vector3(35f, 0, 0): new Vector3(25f, 0, 0);

        Vector3 centerXValue = developmentCards.Count % 2 == 0? horizontalCardOffset / 2: horizontalCardOffset;

        int cardAmountHalf = 1;
        if(developmentCards.Count > 1)
            cardAmountHalf = developmentCards.Count / 2;

        Vector3 cardPosition = Vector3.zero - horizontalCardOffset * cardAmountHalf;

        for(int index = 0; index < developmentCards.Count; index++){
            developmentCards[index].transform.localPosition = cardPosition;
            devCardComponent = developmentCards[index].GetComponent<UIDevelopmentCard>();
            devCardComponent.SetDefaultPosition(cardPosition);


            //TODO: set position of "buy card"-button

            cardPosition += horizontalCardOffset;
        }
    }

    public void SetDevelopmentCardAsFirstSibling(Transform t){
        t.SetAsFirstSibling();
    }
    public void SetDevelopmentCardSiblingIndex(Transform t, int index){
        t.SetSiblingIndex(index);
    }

    public void ShowDevelopmentCardDetails(UIDevelopmentCard card){
        developmentCardDetailsContainer.SetActive(true);

        DevCardDetails details = DevelopmentCardsManager.Instance.GetDevCardDetails((int) card.type);

        // Header
        cardDetailsHeader.SetText("Development: " + details.Name);
        // Description
        cardDetailsDescription.SetText(details.Description);
        // ActionDescription
        cardDetailsActionDescription.SetText(details.ActionDescription);
    }


    #region Buttons methods
    public void UIButtonBuyDevelopmentCard(){
        if(!PlayerRessources.IsRessourceAmountSatisfied(RessourcesManager.instance.RessourceAmountPerDevelopmentCard)){
            Debug.LogError("Trying to buy development card without sufficient ressources.");
            return;
        }
        
        DevelopmentCardsManager.DevelopmentCardType card = DevelopmentCardsManager.Instance.FetchDevelopmentCard();

        UpdateDevelopmentCardsContainer(card);

        PlayerRessources.DiscardRessources(RessourcesManager.instance.RessourceAmountPerDevelopmentCard);
    }

    public void UIButtonPlayDevelopmentCard(UIDevelopmentCard card){
        if(developmentCards.Count == 0)
            return;


        Destroy(card.transform.gameObject);
    }

    #endregion
}
