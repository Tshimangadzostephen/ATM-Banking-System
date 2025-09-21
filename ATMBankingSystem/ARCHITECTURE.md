# ATM Banking System Architecture

## Components

1. **ActiveX Control**
   - Provides methods for the HTA UI to interact with simulated ATM devices
   - COM-visible

2. **Mock XFS Manager**
   - Implements XFS standard device interface
   - Manages logical devices: CardReader, CashDispenser, PINPad, Printer
   - Raises events via `EventBus`

3. **Host Simulator**
   - ISO-8583 TCP server
   - Validates card numbers and PINs using `test-data.json`
   - Processes withdrawals, balance inquiries, fast cash

4. **HTA UI**
   - Displays ATM screens based on NDC state machine (`states.xml`)
   - Handles user input and triggers ActiveX methods

5. **GRG Controls**
   - Mock GRG applications and data pool used for demo/testing

## Data Flow
User -> HTA UI -> ActiveX Control -> Mock XFS -> Devices -> Host Simulator -> ISO-8583 responses


## Build & Deployment
- All components built with MSBuild
- ActiveX controls require registration via `install.bat`
- Demo runs entirely on Windows using HTA host
