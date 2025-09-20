using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace ATMBankingSystem.ActiveXControl
{
    /// <summary>
    /// Simple ATM ActiveX Control Implementation
    /// This is the actual implementation that will be called from the HTA
    /// </summary>
    [ComVisible(true)]
    [Guid("A3F7B9C2-8E5D-4A1B-9C3E-2D6F8B7A5E4C")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("ATMBankingSystem.ATMControl")]
    public class ATMActiveXControl : IATMActiveXControl
    {
        // Private fields to store ATM state
        private string currentState = "000";  // Start at idle state
        private string currentCardNumber = "";
        private string enteredPIN = "";
        private int pinAttempts = 0;
        private decimal accountBalance = 1500.00m;  // Demo balance
        private string selectedTransaction = "";
        private decimal transactionAmount = 0;
        private string lastError = "";

        // Simple data storage (like a mini database)
        private Dictionary<string, string> dataStore = new Dictionary<string, string>();

        // List to keep track of what happened
        private List<string> logMessages = new List<string>();

        /// <summary>
        /// Constructor - runs when control is created
        /// </summary>
        public ATMActiveXControl()
        {
            LogMessage("ATM Control created");
        }

        #region Basic Control Methods

        public string GetVersion()
        {
            return "1.0.0";
        }

        public bool Initialize()
        {
            try
            {
                LogMessage("Initializing ATM System...");

                // Reset everything to starting state
                currentState = "000";
                currentCardNumber = "";
                enteredPIN = "";
                pinAttempts = 0;
                selectedTransaction = "";
                transactionAmount = 0;
                dataStore.Clear();

                LogMessage("ATM System initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                LogMessage($"ERROR: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region State Management

        public bool LoadState(string stateNumber)
        {
            try
            {
                LogMessage($"Loading state: {stateNumber}");

                // Check if this is a valid state transition
                if (IsValidStateChange(currentState, stateNumber))
                {
                    currentState = stateNumber;
                    LogMessage($"State changed to: {stateNumber}");
                    return true;
                }
                else
                {
                    lastError = $"Invalid state change from {currentState} to {stateNumber}";
                    LogMessage($"ERROR: {lastError}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                LogMessage($"ERROR: {ex.Message}");
                return false;
            }
        }

        public string GetCurrentState()
        {
            return currentState;
        }

        /// <summary>
        /// Check if we can move from one state to another
        /// Based on the NDC state machine rules
        /// </summary>
        private bool IsValidStateChange(string fromState, string toState)
        {
            // Define which states can go to which states
            // This is like a map of allowed paths through the ATM screens

            switch (fromState)
            {
                case "000":  // Idle - can only go to card read
                    return toState == "136";

                case "136":  // Card Read - can go to PIN entry or back to idle
                    return toState == "137" || toState == "000";

                case "137":  // PIN Entry - can go to menu or back to idle
                    return toState == "141" || toState == "000";

                case "141":  // Transaction Menu - can go to various transactions or cancel
                    return toState == "395" || toState == "766" || toState == "000";

                case "395":  // Amount Entry - can go to authorization or back
                    return toState == "789" || toState == "141" || toState == "000";

                case "789":  // Authorization - can go to dispense, display, or error
                    return toState == "907" || toState == "766" || toState == "924" || toState == "000";

                case "907":  // Dispense - can go to complete
                    return toState == "909";

                case "766":  // Display Balance - can go to complete or back to menu
                    return toState == "909" || toState == "141";

                case "909":  // Transaction Complete - must go back to idle
                    return toState == "000";

                case "924":  // Error - must go back to idle
                    return toState == "000";

                default:
                    return false;
            }
        }

        #endregion

        #region Card Operations

        public bool InsertCard(string cardNumber)
        {
            try
            {
                LogMessage($"Card inserted: ****{cardNumber.Substring(cardNumber.Length - 4)}");

                // Simple validation - just check it's 16 digits
                if (cardNumber.Length != 16)
                {
                    lastError = "Invalid card number length";
                    return false;
                }

                currentCardNumber = cardNumber;
                SetData("CardNumber", cardNumber);
                return true;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                LogMessage($"ERROR: {ex.Message}");
                return false;
            }
        }

        public bool EjectCard()
        {
            try
            {
                LogMessage("Ejecting card");
                currentCardNumber = "";
                SetData("CardNumber", "");
                return true;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                LogMessage($"ERROR: {ex.Message}");
                return false;
            }
        }

        public string GetCardNumber()
        {
            // Return masked card number for security
            if (string.IsNullOrEmpty(currentCardNumber))
                return "";

            return "****" + currentCardNumber.Substring(currentCardNumber.Length - 4);
        }

        #endregion

        #region PIN Operations

        public bool EnterPIN(string pin)
        {
            try
            {
                LogMessage("PIN entered (hidden for security)");

                // Basic validation
                if (pin.Length < 4 || pin.Length > 6)
                {
                    lastError = "PIN must be 4-6 digits";
                    return false;
                }

                enteredPIN = pin;
                return true;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                LogMessage($"ERROR: {ex.Message}");
                return false;
            }
        }

        public bool ValidatePIN()
        {
            try
            {
                LogMessage("Validating PIN...");

                // For demo: PIN is always 1234
                bool isValid = (enteredPIN == "1234");

                if (!isValid)
                {
                    pinAttempts++;
                    LogMessage($"Invalid PIN. Attempts: {pinAttempts}/3");

                    if (pinAttempts >= 3)
                    {
                        LogMessage("Card retained due to 3 failed PIN attempts");
                        currentCardNumber = "";
                        return false;
                    }
                }
                else
                {
                    LogMessage("PIN validated successfully");
                    pinAttempts = 0;
                }

                return isValid;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                LogMessage($"ERROR: {ex.Message}");
                return false;
            }
        }

        public int GetPINAttempts()
        {
            return pinAttempts;
        }

        #endregion

        #region Transaction Operations

        public bool SelectTransaction(string transactionType)
        {
            try
            {
                LogMessage($"Transaction selected: {transactionType}");
                selectedTransaction = transactionType;
                SetData("TransactionType", transactionType);
                return true;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                LogMessage($"ERROR: {ex.Message}");
                return false;
            }
        }

        public bool SetAmount(decimal amount)
        {
            try
            {
                LogMessage($"Amount set: £{amount}");

                // Check limits
                if (amount > 500)
                {
                    lastError = "Amount exceeds daily limit of £500";
                    LogMessage($"ERROR: {lastError}");
                    return false;
                }

                if (amount > accountBalance)
                {
                    lastError = "Insufficient funds";
                    LogMessage($"ERROR: {lastError}");
                    return false;
                }

                transactionAmount = amount;
                SetData("Amount", amount.ToString());
                return true;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                LogMessage($"ERROR: {ex.Message}");
                return false;
            }
        }

        public decimal GetBalance()
        {
            LogMessage($"Balance inquiry: £{accountBalance}");
            return accountBalance;
        }

        public bool ProcessTransaction()
        {
            try
            {
                LogMessage($"Processing {selectedTransaction} for £{transactionAmount}");

                // Simulate processing
                if (selectedTransaction == "WITHDRAWAL")
                {
                    if (transactionAmount <= accountBalance)
                    {
                        accountBalance -= transactionAmount;
                        LogMessage($"Withdrawal successful. New balance: £{accountBalance}");
                        return true;
                    }
                    else
                    {
                        lastError = "Insufficient funds";
                        return false;
                    }
                }
                else if (selectedTransaction == "BALANCE")
                {
                    LogMessage("Balance inquiry completed");
                    return true;
                }

                return true;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                LogMessage($"ERROR: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Cash Dispenser

        public bool DispenseCash(decimal amount)
        {
            try
            {
                LogMessage($"Dispensing £{amount}");

                // Calculate notes to dispense
                int twenties = (int)(amount / 20);
                int tens = (int)((amount % 20) / 10);

                LogMessage($"Dispensing: {twenties} x £20, {tens} x £10");

                SetData("DispenseStatus", "COMPLETE");
                return true;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                LogMessage($"ERROR: {ex.Message}");
                return false;
            }
        }

        public string GetDispenseStatus()
        {
            return GetData("DispenseStatus");
        }

        #endregion

        #region Receipt Printer

        public bool PrintReceipt()
        {
            try
            {
                LogMessage("Printing receipt...");

                string receipt = $@"
                ===========================
                     ATM TRANSACTION
                ===========================
                Date: {DateTime.Now}
                Card: {GetCardNumber()}
                Type: {selectedTransaction}
                Amount: £{transactionAmount}
                Balance: £{accountBalance}
                ===========================
                Thank you for banking with us
                ===========================";

                SetData("LastReceipt", receipt);
                LogMessage("Receipt printed successfully");
                return true;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                LogMessage($"ERROR: {ex.Message}");
                return false;
            }
        }

        public string GetPrinterStatus()
        {
            // Simple status: OK, PAPER_LOW, NO_PAPER, ERROR
            return "OK";
        }

        #endregion

        #region Data Storage

        public void SetData(string key, string value)
        {
            dataStore[key] = value;
            LogMessage($"Data stored: {key}");
        }

        public string GetData(string key)
        {
            if (dataStore.ContainsKey(key))
                return dataStore[key];
            return "";
        }

        public void ClearData()
        {
            dataStore.Clear();
            LogMessage("All data cleared");
        }

        #endregion

        #region Logging

        public void LogMessage(string message)
        {
            string logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}";
            logMessages.Add(logEntry);

            // Also write to console for debugging
            Console.WriteLine(logEntry);
        }

        public string GetLastError()
        {
            return lastError;
        }

        #endregion

        #region COM Registration

        /// <summary>
        /// This method runs when we register the DLL with Windows
        /// Run: regasm ATMActiveXControl.dll /codebase
        /// </summary>
        [ComRegisterFunction]
        public static void RegisterClass(string key)
        {
            // Create registry entries so Internet Explorer can use our control
            RegistryKey k = Registry.ClassesRoot.OpenSubKey(key.Replace(@"HKEY_CLASSES_ROOT\", ""), true);
            RegistryKey ctrl = k.CreateSubKey("Control");
            ctrl.Close();
            k.Close();
        }

        /// <summary>
        /// This method runs when we unregister the DLL
        /// Run: regasm /u ATMActiveXControl.dll
        /// </summary>
        [ComUnregisterFunction]
        public static void UnregisterClass(string key)
        {
            Registry.ClassesRoot.OpenSubKey(key.Replace(@"HKEY_CLASSES_ROOT\", ""), true)
                ?.DeleteSubKey("Control", false);
        }

        #endregion
    }
}