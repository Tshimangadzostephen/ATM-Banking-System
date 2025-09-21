# ATM State Machine (NDC)

## States

| State | Name | Description |
|-------|------|------------|
| 000   | Idle | Attract loop, waiting for card |
| 136   | Card Read | Card inserted, reading data |
| 137   | PIN Entry | Enter PIN |
| 141   | Transaction Selection | Choose transaction type |
| 395   | Amount Entry | Input withdrawal amount |
| 789   | Authorization | Host authorization request |
| 907   | Dispense | Dispense cash |
| 909   | End Transaction | Transaction complete |
| 766   | Display Information | Display balance or info |
| 924   | Device Error | Error handling screen |

## Workflows

### Cash Withdrawal
   - States: 000 → 136 → 137 → 141 → 395 → 789 → 907 → 909
   - Card insertion → PIN entry → Menu → Amount → Host authorisation → Dispense → Complete

### Balance Inquiry**
   - States: 000 → 136 → 137 → 141 → 789 → 766 → 909
   - Card insertion → PIN entry → Menu → Host request → Balance display → Complete

### Fast Cash
   - States: 000 → 136 → 137 → 395 → 789 → 907 → 909
   - Card insertion → PIN entry → Quick amount → Host authorisation → Dispense → Complete


- Transitions are triggered by user actions, device events, or host responses
