using System;
using System.Runtime.InteropServices;

namespace ATMBankingSystem.GrgControls
{
    /// <summary>
    /// Mock GrgApp Control - Manages the overall ATM application
    /// This simulates the real GRG banking hardware control
    /// </summary>
    [ComVisible(true)]
    [Guid("B1234567-89AB-CDEF-0123-456789ABCDEF")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("ATMBankingSystem.GrgApp")]
    public class MockGrgApp
    {
        private string currentState = "000";
        private string applicationVersion = "1.0.0";
        private bool isInitialized = false;

        /// <summary>
        /// Initialize the ATM application
        /// </summary>
        public int Init()
        {
            try
            {
                Console.WriteLine("[GrgApp] Initializing ATM application...");
                currentState = "000";
                isInitialized = true;
                Console.WriteLine("[GrgApp] Initialization complete");
                return 0; // Success
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"[GrgApp] ERROR: {ex.Message}");
                Console.WriteLine("[GrgApp] ERROR: " + ex.Message);
                return -1; // Error
            }
        }

        /// <summary>
        /// Load a new NDC state (screen)
        /// </summary>
        public int LoadState(string stateNumber)
        {
            try
            {
                Console.WriteLine("[GrgApp] Loading state: " + stateNumber);

                // Validate state exists
                if (!IsValidState(stateNumber))
                {
                    Console.WriteLine("[GrgApp] ERROR: Invalid state " + stateNumber);
                    return -2; // Invalid state
                }

                currentState = stateNumber;
                Console.WriteLine("[GrgApp] State loaded: " + stateNumber);
                return 0; // Success
            }
            catch (Exception ex)
            {
                Console.WriteLine("[GrgApp] ERROR: " + ex.Message);
                return -1; // Error
            }
        }

        /// <summary>
        /// Get the current state number
        /// </summary>
        public string GetCurrentState()
        {
            return currentState;
        }

        /// <summary>
        /// Process a message from the UI
        /// </summary>
        public int ProcessMessage(string message)
        {
            try
            {
                Console.WriteLine("[GrgApp] Processing message: " + message);

                // Parse and handle different message types
                if (message.StartsWith("KEY:"))
                {
                    // Handle keypress
                    string key = message.Substring(4);
                    Console.WriteLine("[GrgApp] Key pressed: " + key);
                }
                else if (message.StartsWith("TIMEOUT"))
                {
                    // Handle timeout
                    Console.WriteLine("[GrgApp] Session timeout");
                    LoadState("000");
                }

                return 0; // Success
            }
            catch (Exception ex)
            {
                Console.WriteLine("[GrgApp] ERROR: " + ex.Message);
                return -1; // Error
            }
        }

        /// <summary>
        /// Shutdown the ATM application
        /// </summary>
        public int Shutdown()
        {
            try
            {
                Console.WriteLine("[GrgApp] Shutting down ATM application");
                isInitialized = false;
                currentState = "000";
                return 0; // Success
            }
            catch (Exception ex)
            {
                Console.WriteLine("[GrgApp] ERROR: " + ex.Message);
                return -1; // Error
            }
        }

        /// <summary>
        /// Log an event
        /// </summary>
        public void LogEvent(string eventMessage)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine("[{timestamp}] [GrgApp] {eventMessage}");
        }

        /// <summary>
        /// Get application version
        /// </summary>
        public string GetVersion()
        {
            return applicationVersion;
        }

        /// <summary>
        /// Check if the application is initialized
        /// </summary>
        public bool IsReady()
        {
            return isInitialized;
        }

        /// <summary>
        /// Validate if a state number is valid
        /// </summary>
        private bool IsValidState(string stateNumber)
        {
            // List of valid NDC states
            string[] validStates = { "000", "136", "137", "141", "395", "789", "907", "909", "766", "924" };

            foreach (string state in validStates)
            {
                if (state == stateNumber)
                    return true;
            }

            return false;
        }

        #region COM Registration

        [ComRegisterFunction]
        public static void RegisterClass(string key)
        {
            var subKey = Microsoft.Win32.Registry.ClassesRoot
                .OpenSubKey(key.Replace(@"HKEY_CLASSES_ROOT\", ""), true);

            if (subKey != null)
            {
                var controlKey = subKey.CreateSubKey("Control");
                if (controlKey != null)
                {
                    controlKey.Close();
                }
            }
        }

        [ComUnregisterFunction]
        public static void UnregisterClass(string key)
        {
            var subKey = Microsoft.Win32.Registry.ClassesRoot
                .OpenSubKey(key.Replace(@"HKEY_CLASSES_ROOT\", ""), true);

            if (subKey != null)
            {
                subKey.DeleteSubKey("Control", false);
            }
        }
        #endregion
    }
}