using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Unity Editor Script: Automatically setup pressure mat scene
/// Usage: Menu -> Tools -> Setup Mat Scene
/// </summary>
public class MatSceneSetup : EditorWindow
{
    [MenuItem("Tools/Setup Mat Scene")]
    public static void SetupScene()
    {
        // 1. Create data reader GameObject
        GameObject matReaderObj = GameObject.Find("MatReader");
        if (matReaderObj == null)
        {
            matReaderObj = new GameObject("MatReader");
            matReaderObj.AddComponent<DualMatReader>();
            Debug.Log("✓ Created MatReader GameObject");
        }
        else
        {
            if (matReaderObj.GetComponent<DualMatReader>() == null)
            {
                matReaderObj.AddComponent<DualMatReader>();
                Debug.Log("✓ Added DualMatReader component");
            }
        }

        // 2. Create Canvas (if not exists)
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("✓ Created Canvas");
        }

        // 3. Create EventSystem (if not exists)
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("✓ Created EventSystem");
        }

        // 4. Create UI display text
        GameObject leftMatTextObj = GameObject.Find("LeftMatText");
        if (leftMatTextObj == null)
        {
            leftMatTextObj = new GameObject("LeftMatText");
            leftMatTextObj.transform.SetParent(canvas.transform, false);
            
            // Try to use TextMeshPro, fallback to regular Text if not available
            TextMeshProUGUI tmpText = leftMatTextObj.GetComponent<TextMeshProUGUI>();
            if (tmpText == null)
            {
                tmpText = leftMatTextObj.AddComponent<TextMeshProUGUI>();
            }
            
            if (tmpText != null)
            {
                tmpText.text = "Waiting for data...";
                tmpText.fontSize = 14;
                tmpText.color = Color.white;
                tmpText.alignment = TextAlignmentOptions.TopLeft;
            }
            else
            {
                // If TextMeshPro is not available, use regular Text
                Text text = leftMatTextObj.AddComponent<Text>();
                text.text = "Waiting for data...\n(Import TextMeshPro for better results)";
                text.fontSize = 14;
                text.color = Color.white;
                text.alignment = TextAnchor.UpperLeft;
                Debug.LogWarning("TextMeshPro not imported, using regular Text component. Suggestion: Window -> TextMeshPro -> Import TMP Essentials");
            }
            
            RectTransform rect = leftMatTextObj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0, 0.5f);
                rect.anchorMax = new Vector2(0, 0.5f);
                rect.pivot = new Vector2(0, 0.5f);
                rect.anchoredPosition = new Vector2(20, 0);
                rect.sizeDelta = new Vector2(400, 300);
            }
            
            Debug.Log("✓ Created LeftMatText UI");
        }

        // 5. Create 2D visualizer
        GameObject visualizerObj = GameObject.Find("MatVisualizer");
        if (visualizerObj == null)
        {
            visualizerObj = new GameObject("MatVisualizer");
            Mat2DVisualizer visualizer = visualizerObj.AddComponent<Mat2DVisualizer>();
            
            // Connect references
            DualMatReader reader = matReaderObj.GetComponent<DualMatReader>();
            visualizer.matReader = reader;
            
            Debug.Log("✓ Created MatVisualizer and connected references");
        }
        else
        {
            Mat2DVisualizer visualizer = visualizerObj.GetComponent<Mat2DVisualizer>();
            if (visualizer == null)
            {
                visualizer = visualizerObj.AddComponent<Mat2DVisualizer>();
            }
            
            DualMatReader reader = matReaderObj.GetComponent<DualMatReader>();
            visualizer.matReader = reader;
            
            Debug.Log("✓ Updated MatVisualizer references");
        }

        // 6. Configure DualMatReader serial port settings
        DualMatReader matReader = matReaderObj.GetComponent<DualMatReader>();
        if (string.IsNullOrEmpty(matReader.portLeft))
        {
            matReader.portLeft = "/dev/cu.usbmodemC04E301365CC2";
            Debug.Log("✓ Set default serial port name");
        }

        EditorUtility.SetDirty(matReaderObj);
        if (canvas != null) EditorUtility.SetDirty(canvas.gameObject);
        if (leftMatTextObj != null) EditorUtility.SetDirty(leftMatTextObj);
        if (visualizerObj != null) EditorUtility.SetDirty(visualizerObj);
        
        Debug.Log("========================================");
        Debug.Log("Scene setup completed!");
        Debug.Log("========================================");
        Debug.Log("Next steps:");
        Debug.Log("1. Check if MatReader's serial port settings are correct");
        Debug.Log("2. Make sure Arduino is connected and running");
        Debug.Log("3. Run the scene to test");
        Debug.Log("========================================");
    }
}
