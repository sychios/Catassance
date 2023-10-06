using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Let camera move in a cinemativ way. 
// Here two modes are implemented, circular movement around the centre of the board and straight movement from point to point.
public class CameraCinematicMovement : MonoBehaviour
{
    [SerializeField] private GameObject cameraGameObject;

    [SerializeField] private GameObject viewTarget;

    [SerializeField] private float cameraHeight;

    [SerializeField] private float cameraDistanceToBoardCentre;

    [Range(0.05f, 0.50f)]
    [SerializeField] private float cameraXRotationValue = 0.1f;

    Vector3 rotatingVector;

    // Start is called before the first frame update
    void Start()
    {
        cameraGameObject.transform.position = Vector3.zero + new Vector3(cameraDistanceToBoardCentre, cameraHeight, 0);
        cameraGameObject.transform.LookAt(viewTarget.transform);
        rotatingVector = cameraGameObject.transform.position;

    }

    private void RotateCameraCircularly(){
        rotatingVector = Quaternion.AngleAxis(cameraXRotationValue * (1-Time.deltaTime), Vector3.up) * rotatingVector;
        cameraGameObject.transform.position = rotatingVector;
        cameraGameObject.transform.LookAt(viewTarget.transform);
    }

    // Update is called once per frame
    void Update()
    {
        RotateCameraCircularly();
    }
}
