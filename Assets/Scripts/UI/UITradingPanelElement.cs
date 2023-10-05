using UnityEngine;
using UnityEngine.EventSystems;

public class UITradingPanelElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private TradingManager tradingManager;

    [SerializeField]
    private RessourcesManager.RessourceType ressourceType;

    [SerializeField]
    private PlayerRessources playerRessources;
    private TMPro.TMP_Text ressourceTextField;

    private Color currentRessourceAmountColor;

    void Start(){
        ressourceTextField = GetComponent<TMPro.TMP_Text>();
    }

    //TODO: amount of ressources displayed in trading panel differs from the actual amount in the player ressources 
    // (e.g. when a ressource is selected to discard for trading). Temporarily store ressources in trading panel and access there?

    // When hovering over ressource image, displayer the conversion rate
    public void OnPointerEnter(PointerEventData data){
        currentRessourceAmountColor = ressourceTextField.color;
        ressourceTextField.color = Color.white;
        ressourceTextField.SetText(playerRessources.RessourceConversionRates[(int) ressourceType].ToString() + ":1");
    }

    // At default, the current amount of the ressource is displayed
    public void OnPointerExit(PointerEventData data){
        ressourceTextField.color = currentRessourceAmountColor;
        ressourceTextField.SetText(tradingManager.GetCurrentRessourceAmount((int) ressourceType).ToString());
    }
}
