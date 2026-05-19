using UnityEngine;
using UnityEngine.UI;
using AetherFlow.Core;
using AetherFlow.Engine;
using AetherFlow.Unity;

namespace AetherFlow.Unity
{
    // Add this script to ANY GameObject in an empty scene.
    // It creates the camera, UI canvas, map, and hands — no prefabs, no manual wiring.
    // Press Play in Unity to run an interactive 5v5 match on Vienna.
    public class SceneBootstrapper : MonoBehaviour
    {
        private void Start()
        {
            SetupCamera();
            var canvas = SetupCanvas();

            // Build game objects.
            var mapView  = new GameObject("MapView").AddComponent<MapView>();
            var handView = new GameObject("HandView").AddComponent<HandView>();
            var hudView  = new GameObject("HudView").AddComponent<HudView>();

            // Build game state.
            var map       = Maps.LoadMap("Vienna");
            var attackers = AgentFactory.CreateAttackers(map);
            var defenders = AgentFactory.CreateDefenders(map);
            var state     = new MatchState(attackers, defenders, map);

            // Create the game controller and inject state.
            var gc = new GameObject("GameController").AddComponent<GameController>();

            // Wire views to canvas / map.
            mapView.Build(map, attackers, defenders);
            handView.Build(canvas);
            hudView.Build(canvas);

            // Start match AFTER views are subscribed to events.
            gc.StartMatch(state);
        }

        private static void SetupCamera()
        {
            // If there is already a main camera use it; otherwise create one.
            var cam = Camera.main;
            if (cam == null)
            {
                cam = new GameObject("Main Camera").AddComponent<Camera>();
                cam.gameObject.tag = "MainCamera";
                cam.gameObject.AddComponent<AudioListener>();
            }

            cam.orthographic     = true;
            cam.orthographicSize = 5.5f;
            cam.transform.position = new Vector3(0, 0, -10);
            cam.backgroundColor  = new Color(0.07f, 0.07f, 0.12f);
            cam.clearFlags       = CameraClearFlags.SolidColor;
        }

        private static Canvas SetupCanvas()
        {
            var go = new GameObject("Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight  = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }
    }
}
