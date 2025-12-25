using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 2D visualization for pressure mat data
/// Uses color gradients and numeric display to make data changes more visible
/// </summary>
public class Mat2DVisualizer : MonoBehaviour
{
    [Header("Data Source")]
    public DualMatReader matReader;

    [Header("Visualization Settings")]
    public bool showLeftMat = true;
    public bool showRightMat = false;
    
    [Header("Color Settings")]
    public Color minColor = new Color(0.2f, 0.2f, 0.8f);  // Dark blue (low pressure)
    public Color midColor = Color.yellow;                   // Yellow (medium pressure)
    public Color maxColor = Color.red;                      // Red (high pressure)
    public Color zeroColor = new Color(0.3f, 0.3f, 0.3f);  // Dark gray (zero value)
    
    [Header("Display Settings")]
    public float cellSize = 100f;            // Size of each cell
    public float cellSpacing = 15f;          // Spacing between cells
    public bool showNumbers = true;          // Whether to show numeric values
    public int fontSize = 24;                // Font size
    
    [Header("Visual Enhancement")]
    public float minValueThreshold = 5f;     // Minimum value threshold (values below this are shown as zero)
    public float colorSensitivity = 0.5f;    // Color sensitivity (0-1, higher = more obvious changes)
    public bool usePulseEffect = true;      // Use pulse effect (flashing for high values)
    
    [Header("Row/Column Mapping (Debug)")]
    public bool transposeMatrix = false;    // Transpose matrix (swap rows and columns)
    public bool flipRows = false;           // Flip row order
    public bool flipCols = false;           // Flip column order
    public bool showDebugInfo = false;      // Show debug information
    
    [Header("Animation Settings")]
    public bool enableAnimation = false;     // Enable animation effects (disabled for faster response)
    public float animationSpeed = 10f;       // Animation speed (higher = faster)
    
    // UI Elements
    private GameObject leftMatContainer;
    private GameObject rightMatContainer;
    private Image[,] leftCells;
    private TextMeshProUGUI[,] leftTexts;
    private Image[,] rightCells;
    private TextMeshProUGUI[,] rightTexts;
    
    // Animation
    private float[,] currentDisplayValues;
    private float[,] targetDisplayValues;
    private float[,] pulseTimers;  // For pulse effect

    void Start()
    {
        currentDisplayValues = new float[4, 4];
        targetDisplayValues = new float[4, 4];
        pulseTimers = new float[4, 4];
        
        CreateVisualization();
    }

    void CreateVisualization()
    {
        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            // Mark as runtime object to prevent editor save issues
            canvasObj.hideFlags = HideFlags.DontSave;
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create left mat container - centered on screen
        if (showLeftMat)
        {
            leftMatContainer = new GameObject("LeftMatVisualization");
            // Mark as runtime object to prevent editor save issues
            leftMatContainer.hideFlags = HideFlags.DontSave;
            leftMatContainer.transform.SetParent(canvas.transform, false);
            RectTransform leftRect = leftMatContainer.AddComponent<RectTransform>();
            // Center on screen
            leftRect.anchorMin = new Vector2(0.5f, 0.5f);
            leftRect.anchorMax = new Vector2(0.5f, 0.5f);
            leftRect.pivot = new Vector2(0.5f, 0.5f);
            leftRect.anchoredPosition = Vector2.zero;
            
            CreateMatGrid(leftMatContainer, out leftCells, out leftTexts, "Left");
        }

        // Create right mat container
        if (showRightMat)
        {
            rightMatContainer = new GameObject("RightMatVisualization");
            // Mark as runtime object to prevent editor save issues
            rightMatContainer.hideFlags = HideFlags.DontSave;
            rightMatContainer.transform.SetParent(canvas.transform, false);
            RectTransform rightRect = rightMatContainer.AddComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(1, 0.5f);
            rightRect.anchorMax = new Vector2(1, 0.5f);
            rightRect.pivot = new Vector2(1, 0.5f);
            rightRect.anchoredPosition = new Vector2(-50, 0);
            
            CreateMatGrid(rightMatContainer, out rightCells, out rightTexts, "Right");
        }
    }

    void CreateMatGrid(GameObject parent, out Image[,] cells, out TextMeshProUGUI[,] texts, string prefix)
    {
        cells = new Image[4, 4];
        texts = new TextMeshProUGUI[4, 4];

        // Calculate grid size - ensure it fits on screen
        float totalWidth = (cellSize + cellSpacing) * 4 - cellSpacing;
        float totalHeight = (cellSize + cellSpacing) * 4 - cellSpacing;
        
        // Center the grid
        float startX = -totalWidth / 2 + cellSize / 2;
        float startY = totalHeight / 2 - cellSize / 2;

        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                // Create cell container
                GameObject cellObj = new GameObject($"{prefix}_Cell_{row}_{col}");
                // Mark as runtime object to prevent editor save issues
                cellObj.hideFlags = HideFlags.DontSave;
                cellObj.transform.SetParent(parent.transform, false);
                
                RectTransform cellRect = cellObj.AddComponent<RectTransform>();
                cellRect.sizeDelta = new Vector2(cellSize, cellSize);
                cellRect.anchoredPosition = new Vector2(
                    startX + col * (cellSize + cellSpacing),
                    startY - row * (cellSize + cellSpacing)
                );

                // Create background image with border
                Image cellImage = cellObj.AddComponent<Image>();
                cellImage.color = zeroColor;
                
                // Add border effect using outline
                Outline outline = cellObj.AddComponent<Outline>();
                outline.effectColor = new Color(0.2f, 0.2f, 0.2f, 1f);
                outline.effectDistance = new Vector2(2, 2);
                
                cells[row, col] = cellImage;

                // Create text
                if (showNumbers)
                {
                    GameObject textObj = new GameObject("Text");
                    // Mark as runtime object to prevent editor save issues
                    textObj.hideFlags = HideFlags.DontSave;
                    textObj.transform.SetParent(cellObj.transform, false);
                    
                    RectTransform textRect = textObj.AddComponent<RectTransform>();
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.sizeDelta = Vector2.zero;
                    textRect.anchoredPosition = Vector2.zero;

                    TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
                    text.text = "0";
                    text.fontSize = fontSize;
                    text.color = Color.white;
                    text.alignment = TextAlignmentOptions.Center;
                    text.fontStyle = FontStyles.Bold;
                    texts[row, col] = text;
                }
            }
        }
    }

    void Update()
    {
        if (matReader == null) return;

        // Update left mat
        if (showLeftMat && leftCells != null && matReader.matLeft != null)
        {
            UpdateMatVisualization(matReader.matLeft, leftCells, leftTexts);
        }

        // Update right mat
        if (showRightMat && rightCells != null && matReader.matRight != null)
        {
            UpdateMatVisualization(matReader.matRight, rightCells, rightTexts);
        }
    }

    void UpdateMatVisualization(int[,] matData, Image[,] cells, TextMeshProUGUI[,] texts)
    {
        // Create a working copy with optional transformations
        int[,] workingData = new int[4, 4];
        
        // Copy and apply transformations
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                int sourceRow = flipRows ? (3 - row) : row;
                int sourceCol = flipCols ? (3 - col) : col;
                
                if (transposeMatrix)
                {
                    workingData[row, col] = matData[sourceCol, sourceRow];
                }
                else
                {
                    workingData[row, col] = matData[sourceRow, sourceCol];
                }
            }
        }
        
        // Now update visualization using transformed data
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                int value = workingData[row, col];
                
                // Update target value for animation
                targetDisplayValues[row, col] = value;
                
                // Smooth animation
                if (enableAnimation)
                {
                    currentDisplayValues[row, col] = Mathf.Lerp(
                        currentDisplayValues[row, col],
                        targetDisplayValues[row, col],
                        Time.deltaTime * animationSpeed
                    );
                }
                else
                {
                    currentDisplayValues[row, col] = value;
                }

                float displayValue = currentDisplayValues[row, col];
                
                // Apply threshold - values below threshold are treated as zero
                float effectiveValue = displayValue < minValueThreshold ? 0 : displayValue;
                
                // Calculate color based on value (0-100 range) with enhanced sensitivity
                Color cellColor;
                if (effectiveValue <= 1)
                {
                    cellColor = zeroColor;
                }
                else
                {
                    // Enhanced color mapping: use three-color gradient for better visibility
                    float normalizedValue = Mathf.Clamp01((effectiveValue - 1) / 99f);
                    
                    // Apply sensitivity multiplier to make changes more obvious
                    normalizedValue = Mathf.Pow(normalizedValue, 1f - colorSensitivity);
                    
                    // Three-color gradient: blue -> yellow -> red
                    if (normalizedValue < 0.5f)
                    {
                        // Blue to Yellow
                        float t = normalizedValue * 2f;
                        cellColor = Color.Lerp(minColor, midColor, t);
                    }
                    else
                    {
                        // Yellow to Red
                        float t = (normalizedValue - 0.5f) * 2f;
                        cellColor = Color.Lerp(midColor, maxColor, t);
                    }
                }

                // Pulse effect for high values
                if (usePulseEffect && effectiveValue > 20)
                {
                    pulseTimers[row, col] += Time.deltaTime * 3f;
                    float pulse = (Mathf.Sin(pulseTimers[row, col]) + 1f) * 0.1f + 0.9f; // 0.9 to 1.1
                    cellColor *= pulse;
                }

                // Update cell color
                if (cells[row, col] != null)
                {
                    cells[row, col].color = cellColor;
                    
                    // Scale effect for high values
                    RectTransform cellRect = cells[row, col].GetComponent<RectTransform>();
                    if (cellRect != null && effectiveValue > 30)
                    {
                        float scale = 1f + (effectiveValue / 100f) * 0.1f; // Slight scale up
                        cellRect.localScale = Vector3.one * scale;
                    }
                    else if (cellRect != null)
                    {
                        cellRect.localScale = Vector3.one;
                    }
                }

                // Update text with larger font for high values
                if (showNumbers && texts[row, col] != null)
                {
                    int displayInt = Mathf.RoundToInt(effectiveValue);
                    texts[row, col].text = displayInt.ToString();
                    
                    // Dynamic font size based on value
                    int dynamicFontSize = fontSize;
                    if (effectiveValue > 50)
                    {
                        dynamicFontSize = fontSize + 4; // Larger font for high values
                    }
                    texts[row, col].fontSize = dynamicFontSize;
                    
                    // Change text color based on background brightness
                    float brightness = (cellColor.r + cellColor.g + cellColor.b) / 3f;
                    texts[row, col].color = brightness > 0.5f ? Color.black : Color.white;
                    texts[row, col].fontStyle = FontStyles.Bold;
                }
            }
        }
    }

    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        if (matReader == null || matReader.matLeft == null) return;
        
        GUIStyle style = new GUIStyle();
        style.fontSize = 14;
        style.normal.textColor = Color.yellow;
        style.wordWrap = true;
        
        string debugText = "=== Data Mapping Debug ===\n";
        debugText += "Raw Data (matLeft):\n";
        for (int r = 0; r < 4; r++)
        {
            debugText += $"Row {r}: ";
            for (int c = 0; c < 4; c++)
            {
                debugText += $"{matReader.matLeft[r, c],3} ";
            }
            debugText += "\n";
        }
        
        debugText += $"\nTransformations:\n";
        debugText += $"Transpose: {transposeMatrix}\n";
        debugText += $"Flip Rows: {flipRows}\n";
        debugText += $"Flip Cols: {flipCols}\n";
        
        GUI.Label(new Rect(Screen.width - 300, 10, 290, 200), debugText, style);
    }

    void OnDestroy()
    {
        if (leftMatContainer != null) Destroy(leftMatContainer);
        if (rightMatContainer != null) Destroy(rightMatContainer);
    }
}

