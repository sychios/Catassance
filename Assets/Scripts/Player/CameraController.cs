using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private GameObject cameraGameObject;

    private Transform cameraTransform;
    private Camera playerCamera;

    private float mouseScrollData;
    
    // MacOS are using double-finger movement on the trackpad which needs to be adjusted by a scale
    private const float mouseScrollDataScale = 0.1f;

    private Vector3 cursorPosition;

    // Target to look at (and rotate around)
    [SerializeField]
    private readonly Vector3 cameraViewTarget = new Vector3(0f, 0f, 0f);

    // zoomStepScale := determines the amount of position-change, each zoom does
    // is used to define variable "ZoomStep" as: (CameraViewTarget - InitialCameraPosition) / zoomStepScale
    [SerializeField]
    private const float zoomStepScale = 10f;
    private const float zoomInLimit_y = 2.75f;
    private const float zoomOutLimit_y = 8f;

    // change in position each zoom-action implicates
    private Vector3 zoomStep;

    // TODO: somehow include the step-scale, which determines how many times player can zoom until zoomed to the max/min
    private int zoomFactor = 0;

    // bounds camera can not overstep
    // vector := (x, z)
    private Vector2 cameraBounds = new Vector2(2f,6f);

    private Vector3 horizontalMovementStep;
    private int horizontalMovementCount;
    private Vector3 verticalMovementStep;
    private int verticalMovementCount;

    private void Awake(){
        cameraTransform = cameraGameObject.GetComponent<Transform>();
        playerCamera = cameraGameObject.GetComponent<Camera>();
    }

    private void Start(){
        zoomStep = (cameraViewTarget - cameraTransform.position) / zoomStepScale;
    }

    private void Update(){
        // Input.mouseScrollDelta.y is positive when scrolling up, and negative when scrolling down
        mouseScrollData = Input.mouseScrollDelta.y;
        cursorPosition = Input.mousePosition;

        // Modern MacOS are using double finger movement that needs to be adjusted according to a scrollrate-scale
        if(Application.platform == RuntimePlatform.OSXPlayer){
            mouseScrollData *= mouseScrollDataScale;
        }

        if(mouseScrollData != 0f){
            HandleZoom(cameraTransform.position);
        }


        // If game is in windowed mode, a cursor position smaller than 0 or greater than screen-values indicates that cursor is outside of window
        if(cursorPosition.y < 0 || cursorPosition.y > Screen.height || cursorPosition.x < 0 || cursorPosition.x > Screen.width){
            return;
        } else {
            var cameraPosition = cameraTransform.position;

            // Vertical Movement
            if(cursorPosition.y >= Screen.height*0.99 && cameraPosition.z < cameraBounds.y){
                MoveVertically(1);
            } else if(cursorPosition.y <= Screen.height*0.01 && cameraPosition.z > (cameraBounds.y * -1f)){
                MoveVertically(-1);
            }

            // Horizontal Movement
            if(cursorPosition.x >= Screen.width*0.99 && cameraPosition.x < cameraBounds.x){
                MoveHorizontally(1);
            } else if(cursorPosition.x <= Screen.width*0.01 && cameraPosition.x > (cameraBounds.x * -1f)){
                MoveHorizontally(-1);
            }            
        }
    }

    
    // If zoomed in to the max, only allow zooming out
    // If zoomed out to the max, only allow zooming in
    // Else handle zooming
    void HandleZoom(Vector3 cameraPosition){
        /*if(zoomFactor >= 5 && MouseScrollData < 0){
            ZoomOut();
        } else if(zoomFactor <= -10 && MouseScrollData > 0){
            ZoomIn();
        } else {
            if(MouseScrollData < 0){
                ZoomOut();
            }  else if (MouseScrollData > 0) {
                ZoomIn();
            }
        } */

        // Zoom-In limit has been passed
        if(cameraTransform.position.y < zoomInLimit_y){
            if(mouseScrollData < 0)
                ZoomOut();
            return;
        } else if(cameraTransform.position.y > zoomOutLimit_y){
            if(mouseScrollData > 0)
                ZoomIn();
            return;
        } else {
            if(mouseScrollData < 0){
                ZoomOut();
            }  else if (mouseScrollData > 0) {
                ZoomIn();
            }
        }
    }

    float ScaleZoomYPosition(float yPos){
        return (Mathf.Max(yPos, zoomInLimit_y+0.5f) - zoomInLimit_y) / (zoomOutLimit_y - zoomInLimit_y);
    }

    void ZoomIn(){
        cameraTransform.position += zoomStep;
        ++zoomFactor;
    }

    void ZoomOut(){
        cameraTransform.position -= zoomStep;
        --zoomFactor;
    }

    // TODO: include zoom factor in movement speed
    // direction := negative if moving left, positive right
    void MoveHorizontally(int direction){
        cameraTransform.position += (ScaleZoomYPosition(cameraTransform.position.y) * (direction * new Vector3(0.1f, 0f, 0f)));
    }

    //TODO: include zoom factor in movement speed
    // move down if direction is negative, else up
    void MoveVertically(int direction){
        cameraTransform.position += (ScaleZoomYPosition(cameraTransform.position.y) * (direction * new Vector3(0f, 0f, 0.1f)));
    }

    // Necessary only for Mocking Players
    // TODO: When networking player, delete Camera Component on other players
    public void SetPlayerCameraActive(bool activated){
        playerCamera.enabled = activated;
    }
}
