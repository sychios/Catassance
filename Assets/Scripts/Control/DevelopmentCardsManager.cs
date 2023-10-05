using System.Collections.Generic;
using UnityEngine;

public class DevelopmentCardsManager : MonoBehaviour
{
    // !! IMPORTANT !!
    // Order of development card items in json file must be the same as in the enum "developmentCardType"
    [SerializeField]
    private TextAsset developmentCardsJSONFile;

    public static DevelopmentCardsManager Instance;

    public enum DevelopmentCardType:int{
        KNIGHT,
        ROADS,
        DEVELOPMENT,
        MONOPOLY,
        POINTS,
        NONE
    }

    private List<DevCardDetails> developmentCardsDetails;


    private List<DevelopmentCardType> cardTypes = new List<DevelopmentCardType>{
        DevelopmentCardType.KNIGHT, DevelopmentCardType.KNIGHT, DevelopmentCardType.KNIGHT, DevelopmentCardType.KNIGHT, 
        DevelopmentCardType.KNIGHT, DevelopmentCardType.KNIGHT, DevelopmentCardType.KNIGHT, DevelopmentCardType.KNIGHT, 
        DevelopmentCardType.KNIGHT, DevelopmentCardType.KNIGHT, DevelopmentCardType.KNIGHT, DevelopmentCardType.KNIGHT, 
        DevelopmentCardType.ROADS, DevelopmentCardType.ROADS, 
        DevelopmentCardType.DEVELOPMENT, DevelopmentCardType.DEVELOPMENT,
        DevelopmentCardType.MONOPOLY, DevelopmentCardType.MONOPOLY, 
        DevelopmentCardType.POINTS, DevelopmentCardType.POINTS,
        DevelopmentCardType.POINTS, DevelopmentCardType.POINTS, DevelopmentCardType.POINTS, DevelopmentCardType.POINTS, DevelopmentCardType.POINTS, DevelopmentCardType.POINTS,
    };

    // Start is called before the first frame update
    void Start()
    {
        if(Instance){
            Debug.LogError("Multiple instances of developmentcard manager found on " + gameObject.name);
            Destroy(this);
            return;
        }

        Instance = this;

        developmentCardsDetails = ParseDevelopmentCardsDetailsFromJSON();

        cardTypes = ListUtilities.ShuffleList(cardTypes);
    }

    public DevelopmentCardType FetchDevelopmentCard(){
        if(cardTypes.Count == 0){
            //TODO: hide development card button when last card is bought
            Debug.LogError("No development cards left. Returning empty card per default.");
            return DevelopmentCardType.NONE;
        }
        
        if(cardTypes.Count == 1)
            return cardTypes[0];

        int randomIndex = UnityEngine.Random.Range(0, cardTypes.Count);

        DevelopmentCardType tmp = cardTypes[randomIndex];

        cardTypes.RemoveAt(randomIndex);

        Debug.Log("Fetched " + tmp);
        
        return tmp;
    }

    private List<DevCardDetails> ParseDevelopmentCardsDetailsFromJSON(){
        //TextAsset jsonFile = Resources.Load<TextAsset>(pathToFile);
        List<DevCardDetails> details = new List<DevCardDetails>();
        
        if(developmentCardsJSONFile == null){
            Debug.LogError("Critical! File to parse development card details is missing. Returning empty list.");
            return details;
        }

        try
        {
            
        }
        catch (System.Exception)
        {
            
            throw;
        }
        DevCardDetailsList detailsList = (DevCardDetailsList) JsonUtility.FromJson<DevCardDetailsList>(developmentCardsJSONFile.text);

        if(detailsList == null){
            Debug.LogError("Critical! Parisong JSON-file of development card details did not return a result. Proceeding without action.");
            return details;
        }

        details.AddRange(detailsList.CardsDetails);

        return details;
    }

    public DevCardDetails GetDevCardDetails(int index){
        if(index < 0 || index > developmentCardsDetails.Count){
            Debug.LogError("Retrieving information for development card failed. Index " + index + " is out of bounds. Returning NONE-Card information.");
            return developmentCardsDetails[5];
        }
        return developmentCardsDetails[index];
    }
}
