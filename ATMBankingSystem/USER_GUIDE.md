# ATM Banking System - User Guide

## Starting the Demo
1. Run `scripts\build.bat` to build all components
2. Run `scripts\install.bat` to register ActiveX controls
3. Launch the HTA interface: `src\UI\index.hta`
4. The demo starts at idle state (`000`)

## Performing Transactions

### Cash Withdrawal
1. Insert card (choose a test card from `test-data.json`)
2. Enter PIN
3. Select "Withdrawal"
4. Enter amount
5. Host authorizes → Cash dispensed
6. End transaction

### Balance Inquiry
1. Insert card
2. Enter PIN
3. Select "Balance Inquiry"
4. Balance is displayed
5. End transaction

### Fast Cash
1. Insert card
2. Enter PIN
3. Select "Fast Cash" → choose amount
4. Host authorizes → Cash dispensed
5. End transaction

## Notes
- Devices may be simulated with Mock XFS
- If an error occurs, the system will display state `924` (Device Error)
- For testing, refer to `config/devices.json` and `config/test-data.json`
