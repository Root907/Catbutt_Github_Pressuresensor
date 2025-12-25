# Pressure Mat Scene Setup Guide

## Quick Setup (Recommended)

### Method 1: Use Automatic Setup Tool
1. In Unity Editor, click menu: **Tools → Setup Mat Scene**
2. Script will automatically create all necessary GameObjects and components
3. Check if settings in Inspector are correct

### Method 2: Manual Setup

## Manual Setup Steps

### Step 1: Create Data Reader
1. Right-click in Hierarchy → **Create Empty**
2. Name it `MatReader`
3. Select `MatReader`, click **Add Component** in Inspector
4. Search and add `DualMatReader` component
5. Configure in Inspector:
   - **Port Left**: 
     - **On Mac**: Usually `/dev/cu.usbmodem...` or `/dev/tty.usbmodem...` (e.g., `/dev/cu.usbmodemC04E301365CC2`)
     - **On Windows**: Usually `COM3`, `COM4`, etc. (e.g., `COM3`)
     - Use `SerialPortFinder` script or check Device Manager (Windows) / Terminal (Mac) to find the correct port
   - **Port Right**: 
     - **If you only have one pressure mat**: Leave empty (or leave as default empty string)
     - **If you have two pressure mats**: Set to the second Arduino's port (see "Adding Second Pressure Mat" section below)
   - **Baud Rate**: `115200`

### Step 2: Create UI Canvas
1. Right-click in Hierarchy → **UI → Canvas**
2. Canvas will be automatically created, including:
   - Canvas
   - EventSystem

### Step 3: Create 2D Visualizer
1. Right-click in Hierarchy → **Create Empty**
2. Name it `MatVisualizer`
3. Select `MatVisualizer`, add `Mat2DVisualizer` component
4. Configure in Inspector:
   - **Mat Reader**: Drag `MatReader` GameObject here
   - **Show Left Mat**: ✓
   - Adjust other visualization settings as needed

## Verify Setup

### Checklist
- [ ] `MatReader` GameObject exists with `DualMatReader` component
- [ ] Serial port name is correctly set
- [ ] `Canvas` exists
- [ ] `MatVisualizer` exists with `Mat2DVisualizer` component
- [ ] `Mat2DVisualizer`'s `Mat Reader` reference is connected

## Test Run

1. **Ensure Arduino is Connected**
   - Check if serial port name is correct
   - Can use `SerialPortFinder` script to find serial port

2. **Ensure Arduino Code is Running**
   - Arduino should send data in format: 4 lines, each with 4 tab-separated values
   - Format example:
     ```
     100	200	150	180
     120	250	190	200
     110	180	160	170
     130	220	200	190
     ```

3. **Run Unity Scene**
   - Click Play button
   - Check Console for any errors
   - Check Game view, should see real-time pressure data grid

## Troubleshooting

### Issue 1: Serial Port Cannot Open
- **Check Serial Port Name**: Use `SerialPortFinder` script to find correct serial port
- **Check Permissions**: On Mac, may need permissions, allow Unity to access serial port in System Settings
- **Check Arduino IDE**: Ensure Arduino IDE is not using the serial port

### Issue 2: No Data Display
- **Check Console**: Check for error messages
- **Check References**: Ensure `Mat2DVisualizer`'s references are correctly connected
- **Check Arduino Data Format**: Ensure data format is correct (4 lines, each with 4 values, tab-separated)

### Issue 3: Visualization Not Displaying
- **Check Canvas**: Ensure Canvas exists and Render Mode is Screen Space - Overlay
- **Check Mat2DVisualizer**: Ensure component is added and `Mat Reader` is connected
- **Check Position**: Grid should be centered on screen

## Adding Second Pressure Mat

When you have two pressure mats:

1. **Find the second Arduino's serial port:**
   - Connect both Arduinos to your computer
   - Use `SerialPortFinder` script to list all available ports
   - Or check:
     - **Windows**: Device Manager → Ports (COM & LPT) → Look for two Arduino devices
     - **Mac**: Terminal → `ls /dev/cu.* | grep usb` → Should show two ports

2. **Configure in `DualMatReader`:**
   - **Port Left**: First Arduino's port (e.g., `/dev/cu.usbmodemC04E301365CC2` on Mac, or `COM3` on Windows)
   - **Port Right**: Second Arduino's port (e.g., `/dev/cu.usbmodemC04E301365CC3` on Mac, or `COM4` on Windows)
   - Both should use the same **Baud Rate**: `115200`

3. **Configure in `Mat2DVisualizer`:**
   - Set `Show Right Mat` = ✓
   - The right mat will appear on the right side of the screen

## Serial Port Finder Tool

If you need to find serial port:
1. Create empty GameObject
2. Add `SerialPortFinder` component
3. Run scene, check available serial port list in Console

