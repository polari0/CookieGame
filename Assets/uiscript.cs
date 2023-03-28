/**********
    Name: uiscript.cs
    Description: UI functionality in the AR cookie game

    Date created: March 2023
    Last edit:

    Author: Kalle
**********/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation; //Needed for ARCameraManager
using TMPro; // TextMeshPro for text

public class uiscript : MonoBehaviour
{
    // Reference to the ARCamera Manager
    [SerializeField] private ARCameraManager arcamera;
    
    // Reference to the ARTrackedImagemanger
    [SerializeField] private ARTrackedImageManager artim;

    // Reference to the ARFaceManager
    [SerializeField] private ARFaceManager arfm;
    private List<ARFace> faces = new List<ARFace>(); // List of ARFaces

    private bool cookiefound = false; // Have we a cookie?
    [SerializeField] private TMP_Text cookietext;   // Text which indicates whether we have a cookie?
    [SerializeField] private Button eatButton;  // Reference to the eat buggon

    private void Awake()
    {
        arcamera.requestedFacingDirection = CameraFacingDirection.User;
    }

    // Subscribe to the trackedImagesChanged and facesChanged-events
    void OnEnable()
    {
        artim.trackedImagesChanged += OnChanged;
        arfm.facesChanged += OnFaceChanged;
    }
    void OnDisable()
    {
        artim.trackedImagesChanged -= OnChanged;
        arfm.facesChanged -= OnFaceChanged;
    }

    void OnFaceChanged(ARFacesChangedEventArgs eventArgs)
    {
        // Add tracked Face to our list of ARFaces
        foreach (var newFace in eventArgs.added)
        {
            faces.Add(newFace);
        }

        // Remove tracked Face from our list of ARFaces
        foreach (var lostFace in eventArgs.removed)
        {
            faces.Remove(lostFace);
        }
    }

    void OnChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // In this event, when cookie has been found by camera, we have gained a cookie!
        foreach (var newImage in eventArgs.added)
        {
            cookiefound = true;
        }
    }

    private void Update()
    {
        // Change UI cookietext depending on whether we have a cookie or not
        if (cookiefound)
        {
            cookietext.text = "Cookie!";
            cookietext.color = Color.green;
        }
        else
        {
            cookietext.text = "No cookie :<";
            cookietext.color = Color.red;
        }

        // Iterate faces on our faces list... 
        // As our app only detects 1 face at this time, we could simply use faces[0]
        foreach (var face in faces)
        {
            // The face.vertice is a position relative to the position of the face.transform
            // face.vertice[15] is near the mouth: https://user-images.githubusercontent.com/7452527/53465316-4a282000-3a02-11e9-8e85-0006e3100da0.png
            Vector3 pos = face.transform.TransformPoint(face.vertices[14]);

            // After using TransformPoint to find the vertice position in World Space,
            // we need to convert it relative to the screen position
            Vector3 screenpos = arcamera.GetComponent<Camera>().WorldToScreenPoint(pos);

            // Move eat button to position
            eatButton.transform.position = screenpos;
        }
    }

    // This button will switch between the front and the back cameras
    public void CameraSwitchButton(TMP_Text buttontext)
    {
        // If camera is facing the world, request to be facing user
        if(arcamera.currentFacingDirection == CameraFacingDirection.World)
        {
            artim.enabled = false;  // Disable Image Tracking when using rear camera
            arfm.enabled = true;    // Enable Face Tracking when using rear camera
            arcamera.requestedFacingDirection = CameraFacingDirection.User;
            buttontext.text = "R"; // Change button text

        }

        // If camera not facing the world, make it face the world
        else
        {
            artim.enabled = true; // Enable Image Tracking when using rear camera
            arfm.enabled = false; // Disable Face Tracking when using rear camera
            arcamera.requestedFacingDirection = CameraFacingDirection.World;
            buttontext.text = "F"; // Change button text

        }

    }

    public void EatCookieButton()
    {
        if(cookiefound)
        {
            cookiefound = false;

            cookietext.text = "You ate cookie";
        }
    }
}


/* notes so what breaks this currently is Image tracking and face tracking interaction 
 * Where you first use image tracking instead of face tracking I have no idea why. 
 * so I came up with stupid solution of just starting the app facing the user instead of facing the world and that fixed it '
 * 
 * Code editor Ilmari Vainio 
 */