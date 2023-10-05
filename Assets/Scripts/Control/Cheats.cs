using UnityEngine;

public class Cheats : MonoBehaviour
{
    private Player currentPlayer;

    private int ressourceAddAmount = 1;

    private bool areCheatsActive = false;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)){
            areCheatsActive = !areCheatsActive;
            return;
        }

        if(!areCheatsActive)
            return;

        currentPlayer = PlayerManager.instance.CurrentPlayer;

        // Add ressources on F1-F5
        if(Input.GetKeyDown(KeyCode.F1)){
            currentPlayer.PlayerRessources.AddRessource(RessourcesManager.RessourceType.WOOD, ressourceAddAmount);
        }
        
        if(Input.GetKeyDown(KeyCode.F2)){
            currentPlayer.PlayerRessources.AddRessource(RessourcesManager.RessourceType.CLAY, ressourceAddAmount);
        }
        
        if(Input.GetKeyDown(KeyCode.F3)){
            currentPlayer.PlayerRessources.AddRessource(RessourcesManager.RessourceType.WHEAT, ressourceAddAmount);
        }
        
        if(Input.GetKeyDown(KeyCode.F4)){
            currentPlayer.PlayerRessources.AddRessource(RessourcesManager.RessourceType.SHEEP, ressourceAddAmount);
        }
        
        if(Input.GetKeyDown(KeyCode.F5)){
            currentPlayer.PlayerRessources.AddRessource(RessourcesManager.RessourceType.ORE, ressourceAddAmount);
        }
    }
}
