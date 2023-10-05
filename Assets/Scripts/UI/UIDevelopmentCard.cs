using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDevelopmentCard : MonoBehaviour
{
    private bool isHovered = false;

    [SerializeField]
    private float yOffset = 15f;

    [SerializeField]
    private TMPro.TMP_Text titleText;
    private Image image;

    private Vector3 defaultLocalPosition;

    private int siblingIndex;

    public PlayerUI playerUI {get; private set;}

    public DevelopmentCardsManager.DevelopmentCardType type {get; private set;}

    // Start is called before the first frame update
    void Start()
    {
        image = gameObject.GetComponent<Image>();
        //SetColor(Color.white);
    }

    // Update is called once per frame
    void Update()
    {        
        if(!isHovered) return;

        if(Input.GetKeyDown(KeyCode.Mouse0)){
            ShowDetails();
        }
        
    }

    private void ShowDetails(){
        playerUI.ShowDevelopmentCardDetails(this);
    }


    public void UIOnPointerEnter(){
        if(isHovered)
            return;
        isHovered = true;

        siblingIndex = this.transform.GetSiblingIndex();
        this.transform.SetAsLastSibling();
    
        var pos = transform.position;
        pos.y += yOffset;
        transform.position = pos;
    }

    public void UIOnPointerExit(){
        isHovered = false;

        transform.localPosition = defaultLocalPosition;

        this.transform.SetSiblingIndex(siblingIndex);
    }

    public void SetColorAndType(DevelopmentCardsManager.DevelopmentCardType pType){
        switch(pType){
            case DevelopmentCardsManager.DevelopmentCardType.KNIGHT:
                SetColor(Color.blue);
                break;
            case DevelopmentCardsManager.DevelopmentCardType.DEVELOPMENT:
                SetColor(Color.yellow);
                break;
            case DevelopmentCardsManager.DevelopmentCardType.MONOPOLY:
                SetColor(Color.red);
                break;
            case DevelopmentCardsManager.DevelopmentCardType.POINTS:
                SetColor(Color.green);
                break;
            case DevelopmentCardsManager.DevelopmentCardType.ROADS:
                SetColor(Color.red + Color.yellow);
                break;
            default:
                SetColor(Color.blue);
                break;
        }

        type = pType;
    }

    public void SetDefaultPosition(Vector3 pDefaultPosition){
        defaultLocalPosition = pDefaultPosition;
    }

    private void SetColor(Color color){
        if(!image)
            image = gameObject.GetComponent<Image>();
        image.color = color;
        titleText.color = InvertColor(color);
    }

    public void SetTitle(string text){
        titleText.SetText(text);
    }

    private Color InvertColor(Color color){
        var maxColor = color.maxColorComponent;
        return new Color(maxColor - color.r, maxColor - color.g, maxColor - color.b);
    }

    public void SetPlayerUI(PlayerUI pPlayerUI){
        playerUI = pPlayerUI;
    }
}
