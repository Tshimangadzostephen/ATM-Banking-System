using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ATMBankingSystem.GrgControls
{
    /// <summary>
    /// COM Interface for GrgDataPool
    /// </summary>
    [ComVisible(true)]
    [Guid("D3456789-ABCD-EF01-2345-6789ABCDEF01")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IGrgDataPool
    {
        [DispId(1)]
        int SetData(string key, string value);

        [DispId(2)]
        object GetData(string key);

        [DispId(3)]
        int ClearData();

        [DispId(4)]
        string GetTransactionData();

        [DispId(5)]
        string HelloWorld();

        [DispId(6)]
        bool HasKey(string key);

        [DispId(7)]
        int GetCount();

        [DispId(8)]
        int RemoveData(string key);
    }
}

namespace ATMBankingSystem.GrgControls
{
    /// <summary>
    /// Mock GrgDataPool Control with explicit interface
    /// </summary>
    [ComVisible(true)]
    [Guid("C2345678-9ABC-DEF0-1234-56789ABCDEF0")]
    [ClassInterface(ClassInterfaceType.None)]  // Use only the explicit interface
    [ProgId("ATMBankingSystem.GrgDataPool")]
    public class MockGrgDataPool : IGrgDataPool
    {
        private Dictionary<string, string> dataPool = new Dictionary<string, string>();

        public string HelloWorld()
        {
            return "Hello from C#!";
        }

        public int SetData(string key, string value)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[GrgDataPool] Setting: " + key + " = " + value);
                dataPool[key] = value;
                return 0; // S_OK (success)
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[GrgDataPool] ERROR: " + ex.Message);
                return unchecked((int)0x80004005); // E_FAIL
            }
        }

        public object GetData(string key)
        {
            try
            {
                if (dataPool.ContainsKey(key))
                {
                    System.Diagnostics.Debug.WriteLine("[GrgDataPool] Getting: " + key + " = " + dataPool[key]);
                    return dataPool[key];
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[GrgDataPool] Key not found: " + key);
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[GrgDataPool] ERROR: " + ex.Message);
                return null;
            }
        }

        public int ClearData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[GrgDataPool] Clearing all data");
                dataPool.Clear();
                return 0; // Success
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[GrgDataPool] ERROR: " + ex.Message);
                return unchecked((int)0x80004005); // E_FAIL
            }
        }

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

                System.Diagnostics.Debug.WriteLine("[GrgDataPool] Transaction data: " + json);
                return json;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[GrgDataPool] ERROR: " + ex.Message);
                return "{}";
            }
        }

        public bool HasKey(string key)
        {
            return dataPool.ContainsKey(key);
        }

        public int GetCount()
        {
            return dataPool.Count;
        }

        public int RemoveData(string key)
        {
            try
            {
                if (dataPool.ContainsKey(key))
                {
                    dataPool.Remove(key);
                    System.Diagnostics.Debug.WriteLine("[GrgDataPool] Removed key: " + key);
                    return 0; // Success
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[GrgDataPool] Key not found: " + key);
                    return -2; // Not found
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[GrgDataPool] ERROR: " + ex.Message);
                return -1; // Error
            }
        }

        #region COM Registration
        [ComRegisterFunction]
        public static void RegisterClass(string key)
        {
            try
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
                    subKey.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("COM Registration error: " + ex.Message);
            }
        }

        [ComUnregisterFunction]
        public static void UnregisterClass(string key)
        {
            try
            {
                var subKey = Microsoft.Win32.Registry.ClassesRoot
                    .OpenSubKey(key.Replace(@"HKEY_CLASSES_ROOT\", ""), true);

                if (subKey != null)
                {
                    subKey.DeleteSubKey("Control", false);
                    subKey.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("COM Unregistration error: " + ex.Message);
            }
        }
        #endregion
    }
}