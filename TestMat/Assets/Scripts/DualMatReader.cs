using UnityEngine;
using System.IO;
using System.IO.Ports;
using System.Collections.Generic;

public class DualMatReader : MonoBehaviour
{
    [Header("Serial Port Configuration")]
    [Tooltip("Left pressure mat serial port name (on Mac usually /dev/cu.usbmodem... or /dev/tty.usbmodem...)")]
    public string portLeft = "/dev/cu.usbmodemC04E301365CC2";
    
    [Tooltip("Right pressure mat serial port name (leave empty if you only have one mat)")]
    public string portRight = "";

    [Header("Serial Port Settings")]
    public int baudRate = 115200;

    [Header("Debug Settings")]
    public bool enableDebugLog = true;

    private SerialPort spLeft;
    private SerialPort spRight;

    [Header("Data Output (Read Only)")]
    public int[,] matLeft = new int[4,4];
    public int[,] matRight = new int[4,4];

    // Debug info
    private int bytesReceivedLeft = 0;
    private int bytesReceivedRight = 0;
    private string lastRawDataLeft = "";
    private string lastRawDataRight = "";
    
    // Buffer to accumulate data until we have a complete packet
    private string leftDataBuffer = "";
    private string rightDataBuffer = "";

    void Start()
    {
        spLeft = TryOpen(portLeft);
        spRight = TryOpen(portRight);
        
        // Try to wake up Arduino by toggling DTR/RTS
        if (spLeft != null && spLeft.IsOpen)
        {
            try
            {
                // Reset Arduino by toggling DTR
                spLeft.DtrEnable = false;
                System.Threading.Thread.Sleep(100);
                spLeft.DtrEnable = true;
                System.Threading.Thread.Sleep(100);
                
                if (enableDebugLog) Debug.Log("Arduino reset signal sent via DTR");
            }
            catch (System.Exception e)
            {
                if (enableDebugLog) Debug.LogWarning($"Could not reset Arduino: {e.Message}");
            }
        }
    }

    SerialPort TryOpen(string port)
    {
        if (string.IsNullOrEmpty(port))
        {
            if (enableDebugLog) Debug.Log("Port name is empty, skipping");
            return null;
        }

        try {
            var sp = new SerialPort(port, baudRate);
            sp.ReadTimeout = 100; // Set read timeout
            sp.Open();
            if (enableDebugLog) Debug.Log($"✓ Serial port {port} opened successfully! Baud rate: {baudRate}");
            return sp;
        }
        catch (System.Exception e) {
            Debug.LogError($"✗ Failed to open serial port {port}: {e.Message}");
            Debug.LogError($"Make sure:");
            Debug.LogError($"1. Arduino is connected");
            Debug.LogError($"2. Port name is correct (use SerialPortFinder to find it)");
            Debug.LogError($"3. No other program is using the port (like Arduino IDE)");
            return null;
        }
    }

    private float lastStatusLogTime = 0f;
    private const float STATUS_LOG_INTERVAL = 2f; // Log status every 2 seconds

    void Update()
    {
        // Check left port
        if(spLeft != null && spLeft.IsOpen)
        {
            int bytesAvailable = spLeft.BytesToRead;
            if (bytesAvailable > 0)
            {
                bytesReceivedLeft += bytesAvailable;
                if (enableDebugLog)
                {
                    Debug.Log($"[Left] Receiving {bytesAvailable} bytes (Total: {bytesReceivedLeft} bytes)");
                }
                ReadOneMat(spLeft, matLeft, "Left");
            }
            else if (enableDebugLog && Time.time - lastStatusLogTime > STATUS_LOG_INTERVAL)
            {
                // Log status even when no data is available
                Debug.Log($"[Left] Port open, waiting for data... (Total received: {bytesReceivedLeft} bytes)");
                lastStatusLogTime = Time.time;
            }
        }
        else if (spLeft == null && !string.IsNullOrEmpty(portLeft))
        {
            // Try to reconnect
            if (Time.frameCount % 300 == 0) // Try every 5 seconds (60fps * 5)
            {
                if (enableDebugLog) Debug.Log("Attempting to reconnect to left port...");
                spLeft = TryOpen(portLeft);
            }
        }

        // Check right port
        if(spRight != null && spRight.IsOpen)
        {
            int bytesAvailable = spRight.BytesToRead;
            if (bytesAvailable > 0)
            {
                bytesReceivedRight += bytesAvailable;
                ReadOneMat(spRight, matRight, "Right");
            }
        }
    }

    void ReadOneMat(SerialPort sp, int[,] mat, string side)
    {
        if (sp == null || !sp.IsOpen) return;

        try
        {
            // Get buffer for this side
            string buffer = side == "Left" ? leftDataBuffer : rightDataBuffer;
            
            // Read new data
            int bytesAvailable = sp.BytesToRead;
            if (bytesAvailable > 0)
            {
                byte[] newBytes = new byte[bytesAvailable];
                int bytesRead = sp.Read(newBytes, 0, bytesAvailable);
                string newData = System.Text.Encoding.UTF8.GetString(newBytes, 0, bytesRead);
                buffer += newData;
            }
            else
            {
                return; // No new data
            }

            // Update buffer
            if (side == "Left")
                leftDataBuffer = buffer;
            else
                rightDataBuffer = buffer;

            // Arduino sends: 4 lines of data, then 2 empty lines
            // Parse all lines and find the most recent complete 4x4 matrix
            string[] allLines = buffer.Split(new char[] { '\n' }, System.StringSplitOptions.None);
            
            // Collect all valid data lines (lines with 4 values)
            List<string> allValidLines = new List<string>();
            for (int i = 0; i < allLines.Length; i++)
            {
                string line = allLines[i].Trim();
                if (!string.IsNullOrEmpty(line))
                {
                    string[] tokens = line.Split(new char[] { '\t', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Length >= 4)
                    {
                        allValidLines.Add(line);
                    }
                }
            }

            // Use the last 4 valid lines (most recent complete matrix)
            if (allValidLines.Count >= 4)
            {
                List<string> dataLines = allValidLines.GetRange(allValidLines.Count - 4, 4);
                
                // Parse the 4 lines
                for(int r=0; r<4; r++)
                {
                    string[] tokens = dataLines[r].Split(new char[] { '\t', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Length >= 4)
                    {
                        for(int c=0; c<4; c++)
                        {
                            int value;
                            if(int.TryParse(tokens[c], out value))
                            {
                                mat[r,c] = value;
                            }
                        }
                        
                        if (enableDebugLog)
                        {
                            Debug.Log($"[{side}] Row {r}: {mat[r,0]} {mat[r,1]} {mat[r,2]} {mat[r,3]}");
                        }
                    }
                }

                // Clear buffer after processing (keep only very recent unprocessed data)
                // Keep last 100 chars in case there's incomplete data
                if (buffer.Length > 100)
                {
                    buffer = buffer.Substring(buffer.Length - 100);
                }
                else
                {
                    buffer = "";
                }

                // Update buffer
                if (side == "Left")
                    leftDataBuffer = buffer;
                else
                    rightDataBuffer = buffer;

                // Store for debugging
                string dataToStore = buffer.Length > 200 ? buffer.Substring(buffer.Length - 200) : buffer;
                if (side == "Left")
                    lastRawDataLeft = dataToStore;
                else
                    lastRawDataRight = dataToStore;
                    
                if (enableDebugLog)
                {
                    Debug.Log($"[{side}] Matrix updated:");
                    Debug.Log($"[{side}]   [{mat[0,0],3},{mat[0,1],3},{mat[0,2],3},{mat[0,3],3}]");
                    Debug.Log($"[{side}]   [{mat[1,0],3},{mat[1,1],3},{mat[1,2],3},{mat[1,3],3}]");
                    Debug.Log($"[{side}]   [{mat[2,0],3},{mat[2,1],3},{mat[2,2],3},{mat[2,3],3}]");
                    Debug.Log($"[{side}]   [{mat[3,0],3},{mat[3,1],3},{mat[3,2],3},{mat[3,3],3}]");
                }
            }
            else
            {
                // Incomplete packet, keep in buffer
                // Limit buffer size to prevent memory issues
                if (buffer.Length > 2000)
                {
                    buffer = buffer.Substring(buffer.Length - 1000);
                    if (side == "Left")
                        leftDataBuffer = buffer;
                    else
                        rightDataBuffer = buffer;
                }
            }
        }
        catch (System.TimeoutException)
        {
            // Timeout is normal
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[{side}] Error reading serial data: {e.Message}");
        }
    }

    void OnGUI()
    {
        if (!enableDebugLog) return;

        // Display debug info on screen
        GUIStyle style = new GUIStyle();
        style.fontSize = 12;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.UpperLeft;

        string debugInfo = "=== Serial Port Debug Info ===\n";
        debugInfo += $"Left Port: {(spLeft != null && spLeft.IsOpen ? "✓ Connected" : "✗ Disconnected")}\n";
        debugInfo += $"Left Bytes Received: {bytesReceivedLeft}\n";
        debugInfo += $"Right Port: {(spRight != null && spRight.IsOpen ? "✓ Connected" : "✗ Disconnected")}\n";
        debugInfo += $"Right Bytes Received: {bytesReceivedRight}\n";
        
        if (!string.IsNullOrEmpty(lastRawDataLeft))
        {
            debugInfo += $"\nLast Left Data:\n{lastRawDataLeft.Substring(0, Mathf.Min(50, lastRawDataLeft.Length))}...\n";
        }

        GUI.Label(new Rect(10, 10, 400, 200), debugInfo, style);
    }

    void OnApplicationQuit()
    {
        if(spLeft != null && spLeft.IsOpen)
        {
            spLeft.Close();
            if (enableDebugLog) Debug.Log("Left serial port closed");
        }

        if(spRight != null && spRight.IsOpen)
        {
            spRight.Close();
            if (enableDebugLog) Debug.Log("Right serial port closed");
        }
    }
}
