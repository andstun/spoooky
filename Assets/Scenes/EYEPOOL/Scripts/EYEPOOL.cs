using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.HighDefinition;
using System.Collections;
using UnityEditor;

public class EyepoolCubeGenerator : MonoBehaviour
{
    public float wallLength = 33.6f;
    public float wallWidth = 33.6f;
    public float wallHeight = 10.8f;
    public float floorWidth = 2.97f;
    public float floorLength = 3.69f;

    public int displayPortFrontBack = 0;
    public int displayPortLeftRight = 1;
    public int displayPortFloor1 = 2;
    public int displayPortFloor2 = 3;

    private Dictionary<int, Dictionary<string, List<Camera>>> camerasByDisplay = new Dictionary<int, Dictionary<string, List<Camera>>>();

    public Material Mat1;
    public Material Mat2;
    public Material Mat3;
    public Material Mat4;
    public Material FloorMat;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Activate all available displays
        for (int i = 0; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }

        // Create walls and floor (without cameras)
        CreateWall(new Vector3(0, 450, 0), Quaternion.Euler(180, 0, 180), displayPort: 2, "Floor");
        CreateWall(new Vector3(0, 450 + wallHeight / 2, wallWidth / 2), Quaternion.Euler(90, 0, 180), displayPort: 0, "Front");
        CreateWall(new Vector3(0, 450 + wallHeight / 2, -wallWidth / 2), Quaternion.Euler(90, 0, 0), displayPort: 0, "Back");
        CreateWall(new Vector3(wallLength / 2, 450 + wallHeight / 2, 0), Quaternion.Euler(90, 0, 90), displayPort: 1, "Right");
        CreateWall(new Vector3(-wallLength / 2, 450 + wallHeight / 2, 0), Quaternion.Euler(90, 0, -90), displayPort: 1, "Left");

        // Set up the cameras
        // SetupCameras();

        // Refresh cameras to ensure they are set up correctly
        // StartCoroutine(RefreshCameras());
    }

    void CreateWall(Vector3 position, Quaternion rotation, int displayPort, string label)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Plane);
        wall.transform.position = position;
        wall.transform.rotation = rotation;
        wall.name = label;  // Naming for debugging

        if (label == "Front")
        {
            wall.transform.localScale = new Vector3(wallLength / 10, 450, wallHeight / 10);
            wall.GetComponent<Renderer>().material = Mat1;
        }
        else if (label == "Back")
        {
            wall.transform.localScale = new Vector3(wallLength / 10, 450, wallHeight / 10);
            wall.GetComponent<Renderer>().material = Mat3;
        }
        else if (label == "Right")
        {
            wall.transform.localScale = new Vector3(wallWidth / 10, 450, wallHeight / 10);
            wall.GetComponent<Renderer>().material = Mat2;
        }
        else if (label == "Left")
        {
            wall.transform.localScale = new Vector3(wallWidth / 10, 450, wallHeight / 10);
            wall.GetComponent<Renderer>().material = Mat4;
        }
        else
        { // Floor
            wall.transform.localScale = new Vector3(floorLength * 2 / 3, 450, floorWidth * 2 / 3);
            wall.GetComponent<Renderer>().material = FloorMat;
        }
    }

    void SetupCameras()
    {
        // Walls
        // Vector3 rightWallLeftQuadrant = new Vector3(-7.47f, 455.4f, -7.2f);
        // Vector3 rightWallRightQuadrant = new Vector3(-7.47f, 455.4f, 7.2f);

        // Vector3 leftWallLeftQuadrant = new Vector3(7.47f, 455.4f, 7.2f);
        // Vector3 leftWallRightQuadrant = new Vector3(7.47f, 455.4f, -7.2f);

        // Vector3 frontWallLeftQuadrant = new Vector3(7.2f, 455.4f, -7.47f);
        // Vector3 frontWallRightQuadrant = new Vector3(-7.2f, 455.4f, -7.47f);

        // Vector3 backWallLeftQuadrant = new Vector3(-7.2f, 455.4f, 7.47f);
        // Vector3 backWallRightQuadrant = new Vector3(7.2f, 455.4f, 7.47f);

        // Floor
        Vector3 floorTopRightQuadrant1 = new Vector3(-5.85f, 456.2f, 6.25f);
        Vector3 floorTopLeftQuadrant1 = new Vector3(5.85f, 456.2f, 6.25f);

        Vector3 floorBottomRightQuadrant = new Vector3(-5.85f, 456.2f, 0);
        Vector3 floorBottomLeftQuadrant = new Vector3(5.85f, 456.2f, 0);

        Vector3 floorTopRightQuadrant2 = new Vector3(-5.85f, 456.2f, -6.25f);
        Vector3 floorTopLeftQuadrant2 = new Vector3(5.85f, 456.2f, -6.25f);

        string left = "_left";
        string right = "_right";

        // CreateCamera(frontWallLeftQuadrant, right, Quaternion.Euler(0, 180, 0), "FrontCamera", displayPortFrontBack);
        // CreateCamera(backWallLeftQuadrant, right, Quaternion.Euler(0, 0, 0), "BackCamera", displayPortLeftRight);
        // CreateCamera(rightWallLeftQuadrant, right, Quaternion.Euler(0, -90, 0), "RightCamera", displayPortFrontBack);
        // CreateCamera(leftWallLeftQuadrant, right, Quaternion.Euler(0, 90, 0), "LeftCamera", displayPortLeftRight);

        // CreateCamera(frontWallRightQuadrant, left, Quaternion.Euler(0, 180, 0), "FrontCamera", displayPortFrontBack);
        // CreateCamera(backWallRightQuadrant, left, Quaternion.Euler(0, 0, 0), "BackCamera", displayPortLeftRight);
        // CreateCamera(rightWallRightQuadrant, left, Quaternion.Euler(0, -90, 0), "RightCamera", displayPortFrontBack);
        // CreateCamera(leftWallRightQuadrant, left, Quaternion.Euler(0, 90, 0), "LeftCamera", displayPortLeftRight);

        CreateCamera(floorTopRightQuadrant1, right, Quaternion.Euler(90, 0, 0), "FloorCameraUp", displayPortFloor1);
        CreateCamera(floorBottomRightQuadrant, right, Quaternion.Euler(90, 0, 0), "FloorCameraMid", displayPortFloor1);
        CreateCamera(floorTopLeftQuadrant1, left, Quaternion.Euler(90, 0, 0), "FloorCameraUp", displayPortFloor1);
        CreateCamera(floorBottomLeftQuadrant, left, Quaternion.Euler(90, 0, 0), "FloorCameraMid", displayPortFloor1);

        CreateCamera(floorTopRightQuadrant2, right, Quaternion.Euler(90, 0, 0), "FloorCameraDown", displayPortFloor2);
        CreateCamera(floorTopLeftQuadrant2, left, Quaternion.Euler(90, 0, 0), "FloorCameraDown", displayPortFloor2);
    }

    void CreateCamera(Vector3 position, string direction, Quaternion rotation, string label, int displayPort)
    {
        GameObject camObj = new GameObject(label + direction);
        Camera cam = camObj.AddComponent<Camera>();
        camObj.transform.position = position;
        camObj.transform.rotation = rotation;
        cam.targetDisplay = displayPort;
        cam.fieldOfView = CalculateFOVForPosition(position);
        cam.aspect = 16f / 9f;

        if (!camerasByDisplay.ContainsKey(displayPort))
            camerasByDisplay[displayPort] = new Dictionary<string, List<Camera>>();
        if (!camerasByDisplay[displayPort].ContainsKey(label))
            camerasByDisplay[displayPort][label] = new List<Camera>();

        camerasByDisplay[displayPort][label].Add(cam);
        SetViewport(cam, displayPort, label);
    }
    
    // uses the dictionary to decide which quadrant of the viewport the camera should get. 
    void SetViewport(Camera cam, int displayPort, string label)
    {
        var camerasInDisplay = camerasByDisplay[displayPort];
        int verticalIndex = 0, horizontalIndex = 0;
        int totalVertical = camerasInDisplay.Count;
        int totalHorizontal = camerasInDisplay[label].Count;

        foreach (var kvp in camerasInDisplay)
        {
            if (kvp.Key == label) break;
            verticalIndex++;
        }
        horizontalIndex = camerasInDisplay[label].IndexOf(cam);

        float width = 0.5f;
        float height = 0.5f;
        float x = horizontalIndex * width;
        float y = 1f - (verticalIndex + 1) * height;

        cam.rect = new Rect(x, y, width, height);
    }

    float CalculateFOVForPosition(Vector3 position)
    {
        return 60f;
    }

    public Vector3 GetEyepoolCubeSize() {
        return new Vector3(wallLength, wallHeight, wallWidth);
    }

    IEnumerator RefreshCameras()
    {
        foreach (var displayDict in camerasByDisplay)
        {
            // skip refresh if key is equal to displayPortFloor1 or displayPortFloor2
            if (displayDict.Key == displayPortFloor1 || displayDict.Key == displayPortFloor2)
            {
                continue;
            }

            foreach (var cameraList in displayDict.Value)
            {
                foreach (Camera cam in cameraList.Value)
                {
                    // Select the camera in the editor (simulates clicking)
                    GameObject myObject = GameObject.Find(cam.gameObject.name);
                    Camera foundCamera = myObject.GetComponent<Camera>();

                    foundCamera.enabled = false;
                    foundCamera.enabled = true;

                    yield return null;
                }
            }
        }
    }
}
