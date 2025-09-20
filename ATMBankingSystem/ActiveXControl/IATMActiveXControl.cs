using System;
using System.Runtime.InteropServices;

namespace ATMBankingSystem.ActiveXControl
{
    /// <summary>
    /// COM Interface for ATM ActiveX Control
    /// This interface defines all the methods that can be called from the HTA/JavaScript
    /// </summary>
    [ComVisible(true)]
    [Guid("F5B4C8D1-9A2E-4F3B-8C6D-1E7A9B5C3D2F")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IATMActiveXControl
    {
        // Basic Control Methods
        [DispId(1)]
        string GetVersion();

        [DispId(2)]
        bool Initialize();

        // Simple State Management
        [DispId(10)]
        bool LoadState(string stateNumber);

        [DispId(11)]
        string GetCurrentState();

        // Card Operations
        [DispId(20)]
        bool InsertCard(string cardNumber);

        [DispId(21)]
        bool EjectCard();

        [DispId(22)]
        string GetCardNumber();

        // PIN Operations
        [DispId(30)]
        bool EnterPIN(string pin);

        [DispId(31)]
        bool ValidatePIN();

        [DispId(32)]
        int GetPINAttempts();

        // Transaction Operations
        [DispId(40)]
        bool SelectTransaction(string transactionType);

        [DispId(41)]
        bool SetAmount(decimal amount);

        [DispId(42)]
        decimal GetBalance();

        [DispId(43)]
        bool ProcessTransaction();

        // Cash Dispenser
        [DispId(50)]
        bool DispenseCash(decimal amount);

        [DispId(51)]
        string GetDispenseStatus();

        // Receipt Printer
        [DispId(60)]
        bool PrintReceipt();

        [DispId(61)]
        string GetPrinterStatus();

        // Data Storage (Simple Key-Value)
        [DispId(70)]
        void SetData(string key, string value);

        [DispId(71)]
        string GetData(string key);

        [DispId(72)]
        void ClearData();

        // Logging
        [DispId(80)]
        void LogMessage(string message);

        [DispId(81)]
        string GetLastError();
    }
}