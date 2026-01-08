using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Smallgantics.Camera;

namespace Smallgantics.Editor
{
    public class SmallganticsSceneSetup
    {
        [MenuItem("Smallgantics/Setup Scene")]
        public static void SetupScene()
        {
            // 1. Setup Environment
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(5, 1, 5);
            
            Material groundMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            groundMat.color = new Color(0.2f, 0.5f, 0.2f); // Dark Green
            ground.GetComponent<Renderer>().material = groundMat;

            // Create some random cubes for "miniature" scale reference
            GameObject environmentRoot = new GameObject("Environment");
            for (int i = 0; i < 20; i++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.parent = environmentRoot.transform;
                cube.transform.position = new Vector3(Random.Range(-20, 20), 0.5f, Random.Range(-20, 20));
                cube.transform.localScale = Vector3.one * Random.Range(0.5f, 2f);
                
                Material cubeMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                cubeMat.color = Random.ColorHSV();
                cube.GetComponent<Renderer>().material = cubeMat;
            }

            // 2. Setup Player
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Cube);
            player.name = "Player";
            player.transform.position = new Vector3(0, 0.5f, 0);
            player.GetComponent<Renderer>().material.color = Color.red;

            // 3. Setup Camera
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }

            // Remove existing follow script if any
            var existingFollow = mainCam.GetComponent<CameraFollow>();
            if (existingFollow != null) Object.DestroyImmediate(existingFollow);

            // Add new follow script
            var followScript = mainCam.gameObject.AddComponent<CameraFollow>();
            followScript.Target = player.transform;
            
            // Set "Smallgantics" Angle
            // High angle, distance back
            mainCam.transform.position = player.transform.position + new Vector3(0, 15, -10);
            mainCam.transform.LookAt(player.transform);
            followScript.Offset = mainCam.transform.position - player.transform.position;
            
            // Adjust FOV for "Miniature" feel (Telephoto-ish)
            mainCam.fieldOfView = 20;

            // 4. Setup Post-Processing (Volume)
            GameObject volumeObj = new GameObject("Global Volume");
            Volume volume = volumeObj.AddComponent<Volume>();
            volume.isGlobal = true;

            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            volume.profile = profile;
            
            // Add Depth of Field
            // Note: Reflection might be safer if types are strictly internal, but URP types should be accessible.
            var dof = profile.Add<DepthOfField>(true);
            dof.mode.Override(DepthOfFieldMode.Bokeh);
            dof.focusDistance.Override(18f); // Approx distance from cam (0,15,-10) to (0,0,0) is sqrt(225+100) = 18.02
            dof.focalLength.Override(100f);
            dof.aperture.Override(4.0f);

            var colorAdjust = profile.Add<ColorAdjustments>(true);
            colorAdjust.saturation.Override(30f); // Boost saturation for "Toy" look
            colorAdjust.contrast.Override(20f);

            // Create Asset for profile (Optional, to save it)
            string path = "Assets/Data";
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder("Assets", "Data");
            
            string profilePath = AssetDatabase.GenerateUniqueAssetPath(path + "/SmallganticsProfile.asset");
            AssetDatabase.CreateAsset(profile, profilePath);
            AssetDatabase.SaveAssets();

            Selection.activeGameObject = player;
            SceneView.lastActiveSceneView.FrameSelected();
            
            Debug.Log("Smallgantics Scene Setup Complete!");
        }
    }
}
