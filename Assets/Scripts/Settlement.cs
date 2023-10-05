using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settlement : MonoBehaviour
{
    private Node parentNode;
    private int ownerID;

    private MeshRenderer meshRenderer;

    private Color originalColor;

    private bool isHovered;

    // Refers to displaying upgradeability when hovered, which is only possible when enough ressources are owned by player
    private bool showUpgradeability;

    void Start(){
        meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!isHovered) return;

        if(Input.GetKeyDown(KeyCode.Mouse0)){
            parentNode.PlaceTown();
            isHovered = false;
        }
    }

    public void SetSettlement(Color color, int id, Node node){
        originalColor = color;
        if(!meshRenderer)
            meshRenderer = GetComponentInChildren<MeshRenderer>();
        meshRenderer.material.color = color;
        ownerID = id;
        parentNode = node;
    }

    public void SetVisibilityOfUpgradeability(bool visible){
        showUpgradeability = visible;
    }

    void OnMouseOver(){
        if(!showUpgradeability || PlayerManager.instance.CurrentPlayerID != ownerID || !PlayerManager.instance.preparationRoundFinished)
            return;

        meshRenderer.material.color = Color.cyan;
        isHovered = true;
    }

    void OnMouseExit(){
        if(!showUpgradeability || PlayerManager.instance.CurrentPlayerID != ownerID || !PlayerManager.instance.preparationRoundFinished)
            return;

        meshRenderer.material.color = originalColor;
        isHovered = false;
    }
}
