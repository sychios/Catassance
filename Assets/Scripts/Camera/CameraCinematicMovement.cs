using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


// Let camera move in a cinemativ way. 
// Here two modes are implemented, circular movement around the centre of the board and straight movement from point to point.
public class CameraCinematicMovement : MonoBehaviour
{
    [SerializeField] private GameObject cameraGameObject;

    [SerializeField] private GameObject viewTarget;

    [Range(0f, 10f)]
    [SerializeField] private float cameraHeight = 2;

    [Range(2f, 10f)]
    [SerializeField] private float cameraDistanceToBoardCenter = 5f;

    [Range(0.05f, 0.50f)]
    [SerializeField] private float cameraXRotationValue = 0.1f;

    [Range(0.01f, 2f)]
    [SerializeField] private float cameraMovementSpeed = 1f;

    [SerializeField] private Transform line1MarkerA;
    [SerializeField] private Transform line1MarkerB;
    [SerializeField] private Transform line2MarkerA;
    [SerializeField] private Transform line2MarkerB;
    [SerializeField] private Transform line3MarkerA;
    [SerializeField] private Transform line3MarkerB;

    private Vector3 rotatingVector;
    private Transform cameraTransform;

    // Duration of how long a mode (e.g. circular movement of the camera around the center) lasts before switching to another mode
    private float movementModeDuration = 10f;
    private float currentModeDuration;

    private bool isCameraCircling;
    private Vector3 previousCameraPositionOfCircularMovement;

    // Variables necessary for straight camera movement
    private Vector3[] worldPointsOfMovingCameraStraight;
    private int worldPointsArrayIndex;
    private Vector3 startPoint;
    private Vector3 endPoint;
    private float distanceBetweenPoints;
    private float startTime;


    // Delegate holding the current camera movement method (circular or straight)
    private delegate void cameraMovementDelegate();
    cameraMovementDelegate movementMethod;

    void Awake(){
        cameraTransform = cameraGameObject.transform;
        currentModeDuration = 0f;
        
        worldPointsOfMovingCameraStraight = new Vector3[6]
            {line1MarkerA.position, line1MarkerB.position, line2MarkerA.position, line2MarkerB.position, line3MarkerA.position, line3MarkerB.position};
        worldPointsArrayIndex = 0;

        // Always start with moving the camera circularly, as board is still building
        movementMethod = MoveCameraCircularly;
        isCameraCircling = true;
    
        // Set previous camera position to default value 
        previousCameraPositionOfCircularMovement = new Vector3(cameraDistanceToBoardCenter, cameraHeight, 0);
    }

    void Start()
    {
        PrepareCameraForCircularMovement();
    }

    private void PrepareCameraForCircularMovement(){
        // Position camera and rotate towards viewtarget
        cameraTransform.position = previousCameraPositionOfCircularMovement;
        cameraTransform.LookAt(viewTarget.transform);
        rotatingVector = cameraTransform.position;
    }

    private void MoveCameraCircularly(){
        rotatingVector = Quaternion.AngleAxis(cameraXRotationValue, Vector3.up) * (rotatingVector*cameraMovementSpeed);
        cameraTransform.position = rotatingVector;
        cameraTransform.LookAt(viewTarget.transform);
    }

    private void MoveCameraFromPointToPoint(){
        float distCovered = (Time.time - startTime) * cameraMovementSpeed;
        float fracOfJourney = distCovered / distanceBetweenPoints;
        cameraTransform.position = Vector3.Lerp(startPoint, endPoint, fracOfJourney);
        if(cameraTransform.position == endPoint){
            currentModeDuration = movementModeDuration;
        }
        cameraTransform.LookAt(endPoint);
    }

    private void UpdateCameraMovement(){
        // Switch mode as duration is succeeded
        if(currentModeDuration >= movementModeDuration){
            if(isCameraCircling){
                // First save the current camera position to continue from there later
                previousCameraPositionOfCircularMovement = cameraTransform.position;

                // Randomize the start- and end-point for more unlinear camera movement
                if(UnityEngine.Random.Range(0,2) == 0) {
                    startPoint = worldPointsOfMovingCameraStraight[worldPointsArrayIndex];
                    endPoint = worldPointsOfMovingCameraStraight[worldPointsArrayIndex+1];
                } else{
                    endPoint = worldPointsOfMovingCameraStraight[worldPointsArrayIndex];
                    startPoint = worldPointsOfMovingCameraStraight[worldPointsArrayIndex+1];
                };

                distanceBetweenPoints = Vector3.Distance(startPoint, endPoint);

                cameraTransform.position = startPoint;

                movementMethod = MoveCameraFromPointToPoint;
                isCameraCircling = false;

                worldPointsArrayIndex = (worldPointsArrayIndex +  2) % worldPointsOfMovingCameraStraight.Length;
                startTime = Time.time;
            } else {
                movementMethod = MoveCameraCircularly;

                PrepareCameraForCircularMovement();

                isCameraCircling = true;
            }

            // In both cases reset duration of active mode and start time
            currentModeDuration = 0;
        }

        // Call delegate inhabting the current movement mode method
        movementMethod();
        
        currentModeDuration += Time.deltaTime;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCameraMovement();
    }
}
