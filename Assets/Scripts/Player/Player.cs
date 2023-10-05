using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public string PlayerName {get; private set;}
    public int PlayerID {get; private set;}
    public Color PlayerColor {get; private set;}

    private CameraController cameraController;

    public PlayerUI PlayerUI {get; private set;}
    public PlayerRessources PlayerRessources {get; private set;}

    public List<Node> AccessibleNodes {get; private set;} = new List<Node>();

    // Hold reference to every settlement owned. Necessary to display buildability of towns if ressources are satisfied
    public List<Settlement> AccessibleSettlements {get; private set;} = new List<Settlement>();

    private void Awake(){
        PlayerUI = GetComponent<PlayerUI>();
        PlayerRessources = GetComponent<PlayerRessources>();
        cameraController = GetComponent<CameraController>();
    }

    private void SetVisibilityOfAccessibleRoads(bool visible){
        RoadManager.instance.SetAccessableRoadsVisibilityForPlayer(PlayerID, visible);
    }

    private void SetVisibilityOfAccessibleNodes(bool visible){
        for(int index = 0; index < AccessibleNodes.Count; ++index){
            AccessibleNodes[index].SetVisibilityIfBuildable(visible);
        }
    }

    private void SetVisibilityOfAccessibleTowns(bool visible){
        for(int index = 0; index < AccessibleSettlements.Count; ++index){
            AccessibleSettlements[index].SetVisibilityOfUpgradeability(visible);
        }
    }

    public void AddAcessibleNode(Node node){
        if(!AccessibleNodes.Contains(node)){
            AccessibleNodes.Add(node);
        }
    }

    public void Deactivate(){
        RoadManager.instance.SetAccessableRoadsVisibilityForPlayer(PlayerID, false);
        PlayerUI.SetCanvasVisibility(false);
        cameraController.SetPlayerCameraActive(false);
        cameraController.enabled = false;
    }

    public void Activate(){
        //Dictionary<RessourcesManager.RessourceType, int> tempDict = new Dictionary<RessourcesManager.RessourceType, int>();
        
        // Set UI
        InitialiseUIAfterActivation();

        // Enable Camera
        cameraController.enabled = true;
        cameraController.SetPlayerCameraActive(true);
    }

    private void InitialiseUIAfterActivation(){        
        PlayerUI.SetCanvasVisibility(true); 
        PlayerUI.SetRessourcesVisibility(true);

        PlayerUI.SetFinishTurnButtonEnabled(true); // Finish turn button should be enabled but not interactable, so that player has to cast die first
        PlayerUI.SetFinishTurnButtonInteractable(false);

        PlayerUI.SetDiceButtonEnabled(true);
        PlayerUI.SetDiceButtonInteractable(true);
        PlayerUI.SetDiceResultText("-");

        PlayerUI.SetTradeButtonActiveAndInteractable(true, false);
    }

    public void ActivateDuringPreparationPhase(){
        //RoadManager.instance.SetAccessableRoadsVisibilityForPlayer(PlayerID, true);
        PlayerUI.SetCanvasVisibility(true);

        // Hide buildable structures
        HideBuildableStructures();

        // UI
        // TODO: Create function in PlayerUI and call following methods there in there
        PlayerUI.SetFinishTurnButtonEnabled(true);
        PlayerUI.SetFinishTurnButtonInteractable(false);
        PlayerUI.SetDiceButtonInteractable(false);
        PlayerUI.SetDiceButtonEnabled(false);
        PlayerUI.SetDiceResultText("-");
        PlayerUI.SetTradeButtonActiveAndInteractable(false, false);

        // Set camera
        cameraController.enabled = true;
        cameraController.SetPlayerCameraActive(true);

        RoadManager.instance.SetAllNodesVisibility(true);
    }

    public void DeactivateDuringPreparationPhase(){
        RoadManager.instance.SetAccessableRoadsVisibilityForPlayer(PlayerID, false);
        //RoadManager.instance.SetAccessableRoadsVisibilityForPlayer(PlayerID, false);
        PlayerUI.SetFinishTurnButtonEnabled(false);
        PlayerUI.SetDiceButtonEnabled(false);
        PlayerUI.SetCanvasVisibility(false);
        cameraController.SetPlayerCameraActive(false);
        cameraController.enabled = false;
    }

    // Display the possibility to buy a building of development card if the necessary ressources are there.
    // Roads and villages that can be built are displayed by showing the mesh with a default color.
    // TODO: how to show buildability of town and development card?
    public void ShowBuildableStructuresIfRessourcesSatisfied(){
        if(PlayerManager.instance.CurrentPlayer != this)
            return;

        bool ressourcesSatisfied = false;
        // .. Road
        ressourcesSatisfied = PlayerRessources.IsRessourceAmountSatisfied(RessourcesManager.instance.RessourceAmountPerRoad);
        SetVisibilityOfAccessibleRoads(ressourcesSatisfied);
        // .. Village
        ressourcesSatisfied = PlayerRessources.IsRessourceAmountSatisfied(RessourcesManager.instance.RessourceAmountPerVillage);
        SetVisibilityOfAccessibleNodes(ressourcesSatisfied);

        // .. Town
        ressourcesSatisfied = PlayerRessources.IsRessourceAmountSatisfied(RessourcesManager.instance.RessourceAmountPerTown);
        SetVisibilityOfAccessibleTowns(ressourcesSatisfied);
        
        // TODO: Add defense tower and development card
    }

    public void HideBuildableStructures(){
        //TODO: Put calls in single method and call?
        SetVisibilityOfAccessibleRoads(false);
        SetVisibilityOfAccessibleNodes(false);
        SetVisibilityOfAccessibleTowns(false);

        // TODO: defense tower and development card
    }

    public void AddSettlement(Settlement settlement){
        if(AccessibleSettlements.Contains(settlement)){
            Debug.LogError("Trying to add settlement already existing in players list. Continueing withouth action.");
            return;
        }
        AccessibleSettlements.Add(settlement);
    }

    public void CastDice(){
        PlayerUI.SetDiceButtonInteractable(false);
        
        int diceResult = UnityEngine.Random.Range(2, 12);
        PlayerUI.SetDiceResultText(diceResult.ToString());
        BoardManager.Instance.ProcessDiceResult(diceResult);

        // If player casts a 7, the bandit takes action and player should not immediately be able to end the turn. 
        PlayerUI.SetFinishTurnButtonInteractable(diceResult != 7);
            
        PlayerUI.SetTradeButtonActiveAndInteractable(true, true);
    }

    public void AddPointsToPlayer(int points = 1){
        PlayerManager.instance.UpdatePlayerScore(this, points);
    }

    public void SubtractPointsFromPlayer(int points){
        PlayerManager.instance.UpdatePlayerScore(this, -1*points);
    }

    public void SetProperties(string name, int id, Color color){
        PlayerName = name;
        PlayerID = id;
        PlayerColor = color;

        PlayerUI.SetNameText(PlayerName);
        PlayerUI.SetNameTextColor(color);

        // Disable ressourceInfo from the beginning to activate after preparation phase
        PlayerUI.SetRessourcesVisibility(false);        
    }


    public void DeclareAsWinner(){
        Debug.Log("Player " + PlayerName + " won!");
        PlayerUI.DisplayGameFinished(true);
    }

    public void DeclareAsLoser(){
        Debug.Log("Player " + PlayerName + " lost ...");
        PlayerUI.DisplayGameFinished(false);
    }

    public void UIButtonPlayerFinished(){
        //TODO: double call of deactivate on Deactivate here and in PlayerManager?
        //Deactivate();
        //StartCoroutine(PlayerManager.instance.PlayerFinished());
        PlayerManager.instance.PlayerHasFinished();
    }

}
