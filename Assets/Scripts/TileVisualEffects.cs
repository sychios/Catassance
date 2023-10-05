using UnityEngine;
using System.Collections;
using TMPro;

public class TileVisualEffects : MonoBehaviour
{
    [SerializeField]
    private GameObject bandit;

    private Transform tileTransform;

    public TMP_Text numberText;

    [SerializeField]
    private MeshRenderer meshRenderer;
    private Color defaultMeshColor;

    // Animations
    public enum TILESTATE:int{
        IDLE,
        ACTIVE,
        BANDIT_OCCUPIED,
        DISASTER_OCCUPIED
    }

    private TILESTATE currentState;
    private Animator animator;

    void Start(){
        currentState = TILESTATE.IDLE;
        animator = GetComponent<Animator>();

        defaultMeshColor = meshRenderer.material.color;

        tileTransform = gameObject.transform;
    }

    public void SetNumberTextValue(int tileNumber)
    {
        if(tileNumber < 2 || tileNumber > 12){
            if(tileNumber == 0){
                // Handle desert
                Destroy(numberText);
                return;
            }
            Debug.LogError(string.Format("Tile number tried to set to {0}, must be between 2 and 12", tileNumber));
        } else {
            numberText.SetText(tileNumber.ToString());
        }
    }

    public void SetNumberTextColor(Color color){
        numberText.color = color;
    }

    public void SetBanditVisibility(bool visible){
        bandit.SetActive(visible);
    }

    public void SetTintingTileMaterial(bool isTinted){
        if(isTinted){
            meshRenderer.material.color /= 2;
        } else {
            meshRenderer.material.color = defaultMeshColor;
        }
    }

    public void SetActiveAnimation(bool isActive){
        if(!animator)
            return;


        if(isActive){
            if(currentState == TILESTATE.IDLE){
                animator.SetBool("Animate", true);
                currentState = TILESTATE.ACTIVE;
            }
        } else {
            animator.SetBool("Animate", false);
            currentState = TILESTATE.IDLE;
        }       
    }

    public void StartYieldAnimation(){
        StartCoroutine(yieldAnimation());
    }

    private IEnumerator yieldAnimation(){
        tileTransform.position += new Vector3(0f, .2f, 0f);

        yield return new WaitForSeconds(1f);

        tileTransform.position -= new Vector3(0f, .2f, 0f);
    }
}
