# Unity Pressure Mat Visualization

A Unity project for real-time visualization of pressure sensor data from Arduino pressure mats.

## Features

- Real-time pressure data reading from Arduino via serial port
- Support for single or dual pressure mats
- 2D grid visualization with color gradients and numeric display
- Automatic scene setup tool
- Cross-platform support (Windows/Mac)

## Requirements

- Unity 2022.3 or later
- Arduino with pressure sensors
- Serial port connection

## Quick Start

### Method 1: Automatic Setup (Recommended)

1. Open the project in Unity
2. Click menu: **Tools → Setup Mat Scene**
3. Configure serial port in `MatReader` GameObject's Inspector
4. Run the scene

### Method 2: Manual Setup

See [SetupGuide.md](Assets/Scripts/SetupGuide.md) for detailed instructions.

## Configuration

### Serial Port Settings

- **Port Left**: Your Arduino's serial port
  - Mac: `/dev/cu.usbmodem...` or `/dev/tty.usbmodem...`
  - Windows: `COM3`, `COM4`, etc.
- **Port Right**: Leave empty for single mat, or set second Arduino's port for dual mats
- **Baud Rate**: `115200`

### Finding Your Serial Port

Use the `SerialPortFinder` script:
1. Create empty GameObject
2. Add `SerialPortFinder` component
3. Run scene, check Console for available ports

## Project Structure

```
Assets/
├── Scripts/
│   ├── DualMatReader.cs          # Main data reader from serial port
│   ├── Mat2DVisualizer.cs       # 2D grid visualization
│   ├── SerialPortFinder.cs      # Utility to find serial ports
│   ├── Editor/
│   │   └── MatSceneSetup.cs     # Automatic scene setup tool
│   ├── SetupGuide.md            # Setup instructions
│   └── TroubleshootingGuide.md  # Troubleshooting guide
└── Scenes/
    └── SampleScene.unity        # Main scene
```

## Troubleshooting

See [TroubleshootingGuide.md](Assets/Scripts/TroubleshootingGuide.md) for common issues and solutions.

## License

This project is provided as-is for educational and development purposes.

