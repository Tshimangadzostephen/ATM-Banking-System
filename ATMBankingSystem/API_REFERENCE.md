# ATM Banking System API Reference

## ActiveX Control (IATMActiveXControl)
| Method | Description | Parameters | Return |
|--------|-------------|------------|--------|
| `InsertCard(cardNumber)` | Simulate card insertion | `string cardNumber` | `bool` success |
| `EnterPIN(pin)` | Simulate PIN entry | `string pin` | `bool` success |
| `SelectTransaction(type)` | Choose transaction type | `string type` | `bool` success |
| `EnterAmount(amount)` | Enter cash amount | `decimal amount` | `bool` success |
| `StartTransaction()` | Begin transaction workflow | - | `bool` success |

## Mock XFS Manager
| Method | Description | Parameters | Return |
|--------|-------------|------------|--------|
| `WFSStartUp(version)` | Initialize XFS Manager | `int version` | HRESULT |
| `WFSOpen(logicalName, out hService)` | Open device | `string`, `out int` | HRESULT |
| `WFSExecute(hService, command, data, timeout)` | Execute command | `int`, `int`, `object`, `int` | HRESULT |
| `WFSGetInfo(hService, category, out result)` | Get device info | `int`, `int`, `out object` | HRESULT |
| `WFSClose(hService)` | Close device | `int` | HRESULT |
| `WFSCleanUp()` | Shutdown XFS Manager | - | HRESULT |
