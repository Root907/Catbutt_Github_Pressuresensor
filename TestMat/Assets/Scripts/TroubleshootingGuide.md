# Troubleshooting Guide - No Data Received

## Quick Checklist

### 1. Check Serial Port Connection
- [ ] Arduino is connected to computer via USB
- [ ] In Unity Inspector, `DualMatReader`'s `Port Left` is set correctly
- [ ] When running the scene, Console shows "✓ Serial port ... opened successfully!"

**How to find the correct serial port name:**
- Use `SerialPortFinder` script (create empty GameObject, add component, run scene)
- Or run in terminal: `ls /dev/cu.* | grep usb`

### 2. Check Arduino Code
Arduino needs to send data in a specific format. Two formats are supported:

**Format 1: 4 lines, each with 4 values (tab or space separated)**
```
100	200	150	180
120	250	190	200
110	180	160	170
130	220	200	190
```

**Format 2: Single line, 16 values (tab, space, or comma separated)**
```
100 200 150 180 120 250 190 200 110 180 160 170 130 220 200 190
```

### 3. Check Unity Console
After running the scene, check the Console window:

**Normal case should show:**
- `✓ Serial port /dev/cu.usbmodem... opened successfully!`
- `[Left] Matrix updated:` with 4x4 matrix data

**If there are errors:**
- `✗ Failed to open serial port` → Check serial port name and connection
- `Received X lines, expected 4` → Arduino data format is incorrect
- `Failed to parse value` → Data is not in numeric format

### 4. Enable Debug Mode

In Unity Inspector, find the `DualMatReader` component on `MatReader` GameObject:

1. **Enable Debug Log** = ✓ (shows detailed logs)

After running the scene:
- Console will show all received raw data
- Game view top-left will show connection status and bytes received

### 5. Common Issues

#### Issue 1: Serial Port Cannot Open
**Possible causes:**
- Arduino IDE is using the port (close Arduino IDE's Serial Monitor)
- Incorrect serial port name
- Permission issues (on Mac, may need to allow Unity access in System Settings)

**Solutions:**
- Close Arduino IDE
- Use `SerialPortFinder` to confirm the correct serial port name
- Check System Settings → Security & Privacy → Allow Unity access

#### Issue 2: Data Received But Parsing Failed
**Possible causes:**
- Arduino is sending data in incorrect format
- Separator is not tab (might be space or comma)

**Solutions:**
- Enable debug logging, check the actual received data format
- Modify Arduino code to ensure correct format
- Script supports space and comma separators, but tab is recommended

#### Issue 3: No Data At All
**Possible causes:**
- Arduino code is not running
- Baud rate mismatch
- Serial port connection disconnected

**Solutions:**
- Check if Arduino is running normally after uploading code
- Confirm `Baud Rate` setting matches `Serial.begin()` in Arduino code (usually 115200)
- Check debug info in Game view top-left for connection status

### 6. Testing Steps

1. **Confirm Serial Port Connection**
   ```
   Run scene → Check Console → Should see "opened successfully"
   ```

2. **Check Data Reception**
   ```
   Enable debug logging → Press pressure mat → Check raw data in Console
   ```

3. **Verify Data Parsing**
   ```
   Check Console → Should see "Matrix updated" messages
   ```

4. **Check UI Display**
   ```
   Check Game view → Should see pressure data grid
   ```

### 7. Arduino Code Example

If your Arduino code format is incorrect, here's a reference format:

```cpp
void loop() {
  // Read sensor data (assuming you have 4x4 matrix)
  for(int row = 0; row < 4; row++) {
    for(int col = 0; col < 4; col++) {
      int value = analogRead(row * 4 + col); // Adjust based on your actual wiring
      Serial.print(value);
      if(col < 3) Serial.print("\t"); // Tab separator
    }
    Serial.println(); // New line
  }
  delay(50); // Adjust delay to fit your needs
}
```

### 8. Getting Help

If none of the above methods work, please provide the following information:
1. Complete error messages from Console
2. Raw data seen after enabling debug logging
3. Relevant parts of Arduino code (especially `Serial.print` parts)
4. Screenshot of debug info displayed in Game view top-left

