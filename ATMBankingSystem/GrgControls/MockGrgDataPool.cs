using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ATMBankingSystem.GrgControls
{
    /// <summary>
    /// Mock GrgDataPool Control - Manages session data
    /// This acts like a mini database for the ATM session
    /// </summary>
    [ComVisible(true)]
    [Guid("C2345678-9ABC-DEF0-1234-56789ABCDEF0")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("ATMBankingSystem.GrgDataPool")]
    public class MockGrgDataPool
    {
        // Store all session data in a dictionary (like a simple database)
        private Dictionary<string, string> dataPool = new Dictionary<string, string>();

        /// <summary>
        /// Store a value with a key
        /// </summary>
        public int SetData(string key, string value)
        {
            try
            {
                //Console.WriteLine("[GrgDataPool] Setting: " + key + " = " + value);
                System.Diagnostics.Debug.WriteLine("[GrgDataPool] Setting: " + key + " = " + value);

                dataPool[key] = value;
                return 0; // Success
            }
            catch (Exception ex)
            {
                Console.WriteLine("[GrgDataPool] ERROR: " + ex.Message);
                return -1; // Error
            }
        }

        /// <summary>
        /// Get a value by its key
        /// </summary>
        public object GetData(string key)
        {
            try
            {
                if (dataPool.ContainsKey(key))
                {
                    Console.WriteLine("[GrgDataPool] Getting: " + key + " = " + dataPool[key]);
                    return dataPool[key];
                }
                else
                {
                    Console.WriteLine("[GrgDataPool] Key not found: " + key);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[GrgDataPool] ERROR: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Clear all stored data
        /// </summary>
        public int ClearData()
        {
            try
            {
                Console.WriteLine("[GrgDataPool] Clearing all data");
                dataPool.Clear();
                return 0; // Success
            }
            catch (Exception ex)
            {
                Console.WriteLine("[GrgDataPool] ERROR: " + ex.Message);
                return -1; // Error
            }
        }

        /// <summary>
        /// Get all transaction data as JSON string
        /// </summary>
        public string GetTransactionData()
        {
            try
            {
                // Build a simple JSON-like string with transaction data
                string json = "{\n";
                json += "  \"CardNumber\": \"" + GetData("CardNumber") + "\",\n";
                json += "  \"TransactionType\": \"" + GetData("TransactionType") + "\",\n";
                json += "  \"Amount\": \"" + GetData("Amount") + "\",\n";
                json += "  \"Balance\": \"" + GetData("Balance") + "\",\n";
                json += "  \"Timestamp\": \"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\",\n";
                json += "  \"ATMId\": \"ATM001\"\n";
                json += "}";

                Console.WriteLine("[GrgDataPool] Transaction data: " + json);
                return json;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[GrgDataPool] ERROR: " + ex.Message);
                return "{}";
            }
        }

        /// <summary>
        /// Check if a key exists
        /// </summary>
        public bool HasKey(string key)
        {
            return dataPool.ContainsKey(key);
        }

        /// <summary>
        /// Get the number of stored items
        /// </summary>
        public int GetCount()
        {
            return dataPool.Count;
        }

        /// <summary>
        /// Remove a specific key
        /// </summary>
        public int RemoveData(string key)
        {
            try
            {
                if (dataPool.ContainsKey(key))
                {
                    dataPool.Remove(key);
                    Console.WriteLine("[GrgDataPool] Removed key: " + key);
                    return 0; // Success
                }
                else
                {
                    Console.WriteLine("[GrgDataPool] Key not found: " + key);
                    return -2; // Not found
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[GrgDataPool] ERROR: " + ex.Message);
                return -1; // Error
            }
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