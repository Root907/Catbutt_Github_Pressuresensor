using UnityEngine;
using System.IO.Ports;
using System.Linq;

/// <summary>
/// Utility to find and list available serial port devices
/// When run in Unity editor, outputs all available serial ports to Console
/// </summary>
public class SerialPortFinder : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== Available Serial Ports ===");
        string[] ports = SerialPort.GetPortNames();
        
        if (ports.Length == 0)
        {
            Debug.LogWarning("No serial port devices found! Make sure Arduino is connected.");
        }
        else
        {
            Debug.Log($"Found {ports.Length} serial port device(s):");
            for (int i = 0; i < ports.Length; i++)
            {
                Debug.Log($"[{i}] {ports[i]}");
            }
            
            // On Mac, usually use /dev/cu.* instead of /dev/tty.*
            var macPorts = ports.Where(p => p.Contains("usb") || p.Contains("USB")).ToArray();
            if (macPorts.Length > 0)
            {
                Debug.Log("\n=== Possible Arduino ports (containing 'usb') ===");
                foreach (var port in macPorts)
                {
                    Debug.Log($"  → {port}");
                }
            }
        }
    }
}

