using System;
using System.Collections.Generic;
using System.Threading;

namespace ATMBankingSystem.MockXFS
{
    /// <summary>
    /// Card Reader Service Provider (IDC - Identification Card)
    /// </summary>
    public class CardReaderServiceProvider : ServiceProvider
    {
        // IDC Commands
        public const int WFS_CMD_IDC_READ_RAW_DATA = 301;
        public const int WFS_CMD_IDC_EJECT_CARD = 302;
        public const int WFS_CMD_IDC_RETAIN_CARD = 303;
        public const int WFS_CMD_IDC_RESET = 304;

        // IDC Info Categories
        public const int WFS_INF_IDC_STATUS = 401;
        public const int WFS_INF_IDC_CAPABILITIES = 402;

        private string currentCardData = "";
        private bool cardPresent = false;
        private int retainedCards = 0;

        public override int Execute(int command, object cmdData, int timeout)
        {
            Console.WriteLine("[Card Reader] Execute command: " + command);

            switch (command)
            {
                case WFS_CMD_IDC_READ_RAW_DATA:
                    return ReadCard();

                case WFS_CMD_IDC_EJECT_CARD:
                    return EjectCard();

                case WFS_CMD_IDC_RETAIN_CARD:
                    return RetainCard();

                case WFS_CMD_IDC_RESET:
                    return ResetDevice();

                default:
                    Console.WriteLine("[Card Reader] Unsupported command: " + command);
                    return XFSManager.WFS_ERR_UNSUPP_COMMAND;
            }
        }

        public override int GetInfo(int category, out object result)
        {
            result = null;
            Console.WriteLine("[Card Reader] GetInfo category: " + category);

            switch (category)
            {
                case WFS_INF_IDC_STATUS:
                    result = new CardReaderStatus
                    {
                        DeviceState = "ONLINE",
                        MediaPresent = cardPresent,
                        RetainBinCount = retainedCards
                    };
                    return XFSManager.WFS_SUCCESS;

                case WFS_INF_IDC_CAPABILITIES:
                    result = new CardReaderCapabilities
                    {
                        CanEject = true,
                        CanRetain = true,
                        CanReadTrack1 = true,
                        CanReadTrack2 = true,
                        CanReadTrack3 = true
                    };
                    return XFSManager.WFS_SUCCESS;

                default:
                    return XFSManager.WFS_ERR_UNSUPP_COMMAND;
            }
        }

        private int ReadCard()
        {
            Thread.Sleep(1000);
            currentCardData = "4532123456789012=2512101123456789";
            cardPresent = true;

            Console.WriteLine("[Card Reader] Card read successfully");

            if (eventBus != null)
            {
                eventBus.Publish(new XFSEvent
                {
                    EventType = "CARD_INSERTED",
                    EventId = 1001,
                    EventData = currentCardData
                });
            }

            return XFSManager.WFS_SUCCESS;
        }

        private int EjectCard()
        {
            if (!cardPresent)
            {
                Console.WriteLine("[Card Reader] No card to eject");
                return XFSManager.WFS_ERR_HARDWARE_ERROR;
            }

            Thread.Sleep(500);
            cardPresent = false;
            currentCardData = "";

            Console.WriteLine("[Card Reader] Card ejected");
            return XFSManager.WFS_SUCCESS;
        }

        private int RetainCard()
        {
            if (!cardPresent)
            {
                Console.WriteLine("[Card Reader] No card to retain");
                return XFSManager.WFS_ERR_HARDWARE_ERROR;
            }

            Thread.Sleep(500);
            cardPresent = false;
            currentCardData = "";
            retainedCards++;

            Console.WriteLine("[Card Reader] Card retained. Total retained: " + retainedCards);
            return XFSManager.WFS_SUCCESS;
        }

        private int ResetDevice()
        {
            Console.WriteLine("[Card Reader] Resetting device");
            if (cardPresent) EjectCard();
            return XFSManager.WFS_SUCCESS;
        }
    }

    /// <summary>
    /// Cash Dispenser Service Provider (CDM)
    /// </summary>
    public class CashDispenserServiceProvider : ServiceProvider
    {
        // CDM Commands
        public const int WFS_CMD_CDM_DISPENSE = 321;
        public const int WFS_CMD_CDM_PRESENT = 322;
        public const int WFS_CMD_CDM_RETRACT = 323;
        public const int WFS_CMD_CDM_RESET = 324;

        // CDM Info Categories
        public const int WFS_INF_CDM_STATUS = 421;
        public const int WFS_INF_CDM_CASH_UNIT_INFO = 422;

        private Dictionary<int, CashUnit> cashUnits;
        private bool cashPresented = false;
        private int lastDispensedAmount = 0;

        public CashDispenserServiceProvider()
        {
            cashUnits = new Dictionary<int, CashUnit>
            {
                { 1, new CashUnit { Denomination = 20, Count = 100, Currency = "GBP" } },
                { 2, new CashUnit { Denomination = 10, Count = 200, Currency = "GBP" } },
                { 3, new CashUnit { Denomination = 5, Count = 150, Currency = "GBP" } }
            };
        }

        public override int Execute(int command, object cmdData, int timeout)
        {
            Console.WriteLine("[Cash Dispenser] Execute command: " + command);

            switch (command)
            {
                case WFS_CMD_CDM_DISPENSE:
                    return DispenseCash((int)cmdData);

                case WFS_CMD_CDM_PRESENT:
                    return PresentCash();

                case WFS_CMD_CDM_RETRACT:
                    return RetractCash();

                case WFS_CMD_CDM_RESET:
                    return ResetDevice();

                default:
                    Console.WriteLine("[Cash Dispenser] Unsupported command: " + command);
                    return XFSManager.WFS_ERR_UNSUPP_COMMAND;
            }
        }

        public override int GetInfo(int category, out object result)
        {
            result = null;
            Console.WriteLine("[Cash Dispenser] GetInfo category: " + category);

            switch (category)
            {
                case WFS_INF_CDM_STATUS:
                    result = new DispenserStatus
                    {
                        DeviceState = "ONLINE",
                        DispenserState = cashPresented ? "PRESENTED" : "OK",
                        IntermediateStacker = "EMPTY"
                    };
                    return XFSManager.WFS_SUCCESS;

                case WFS_INF_CDM_CASH_UNIT_INFO:
                    result = cashUnits;
                    return XFSManager.WFS_SUCCESS;

                default:
                    return XFSManager.WFS_ERR_UNSUPP_COMMAND;
            }
        }

        private int DispenseCash(int amount)
        {
            Console.WriteLine("[Cash Dispenser] Dispensing £" + amount);

            var noteMix = CalculateNoteMix(amount);
            if (noteMix == null)
            {
                Console.WriteLine("[Cash Dispenser] Unable to dispense amount");
                return XFSManager.WFS_ERR_HARDWARE_ERROR;
            }

            foreach (var kvp in noteMix)
            {
                CashUnit unit = cashUnits[kvp.Key];
                unit.Count -= kvp.Value;
                Console.WriteLine("[Cash Dispenser] Dispensing " + kvp.Value + " x £" + unit.Denomination);
            }

            Thread.Sleep(2000);
            lastDispensedAmount = amount;
            cashPresented = true;

            Console.WriteLine("[Cash Dispenser] Cash dispensed successfully");
            return XFSManager.WFS_SUCCESS;
        }

        private int PresentCash()
        {
            if (!cashPresented)
            {
                Console.WriteLine("[Cash Dispenser] No cash to present");
                return XFSManager.WFS_ERR_HARDWARE_ERROR;
            }

            Console.WriteLine("[Cash Dispenser] Presenting cash to customer");
            if (eventBus != null)
            {
                eventBus.Publish(new XFSEvent
                {
                    EventType = "CASH_PRESENTED",
                    EventId = 2001,
                    EventData = lastDispensedAmount
                });
            }

            return XFSManager.WFS_SUCCESS;
        }

        private int RetractCash()
        {
            if (!cashPresented)
            {
                Console.WriteLine("[Cash Dispenser] No cash to retract");
                return XFSManager.WFS_ERR_HARDWARE_ERROR;
            }

            Console.WriteLine("[Cash Dispenser] Retracting cash");
            Thread.Sleep(1000);
            cashPresented = false;
            lastDispensedAmount = 0;

            Console.WriteLine("[Cash Dispenser] Cash retracted");
            return XFSManager.WFS_SUCCESS;
        }

        private int ResetDevice()
        {
            Console.WriteLine("[Cash Dispenser] Resetting device");
            if (cashPresented) RetractCash();
            return XFSManager.WFS_SUCCESS;
        }

        private Dictionary<int, int> CalculateNoteMix(int amount)
        {
            Dictionary<int, int> noteMix = new Dictionary<int, int>();
            int remaining = amount;

            foreach (KeyValuePair<int, CashUnit> kvp in cashUnits)
            {
                CashUnit unit = kvp.Value;
                if (remaining >= unit.Denomination && unit.Count > 0)
                {
                    int notesNeeded = remaining / unit.Denomination;
                    int notesToUse = Math.Min(notesNeeded, unit.Count);
                    if (notesToUse > 0)
                    {
                        noteMix[kvp.Key] = notesToUse;
                        remaining -= notesToUse * unit.Denomination;
                    }
                }
            }

            return remaining == 0 ? noteMix : null;
        }
    }

    /// <summary>
    /// PIN Pad Service Provider
    /// </summary>
    public class PINPadServiceProvider : ServiceProvider
    {
        public const int WFS_CMD_PIN_GET_PIN = 341;
        public const int WFS_CMD_PIN_GET_PINBLOCK = 342;
        public const int WFS_CMD_PIN_RESET = 343;

        public const int WFS_INF_PIN_STATUS = 441;
        public const int WFS_INF_PIN_CAPABILITIES = 442;

        private string currentPIN = "";
        private bool pinEntered = false;

        public override int Execute(int command, object cmdData, int timeout)
        {
            Console.WriteLine("[PIN Pad] Execute command: " + command);

            switch (command)
            {
                case WFS_CMD_PIN_GET_PIN:
                    return GetPIN(timeout);

                case WFS_CMD_PIN_GET_PINBLOCK:
                    return GetPINBlock();

                case WFS_CMD_PIN_RESET:
                    return ResetDevice();

                default:
                    Console.WriteLine("[PIN Pad] Unsupported command: " + command);
                    return XFSManager.WFS_ERR_UNSUPP_COMMAND;
            }
        }

        public override int GetInfo(int category, out object result)
        {
            result = null;
            Console.WriteLine("[PIN Pad] GetInfo category: " + category);

            switch (category)
            {
                case WFS_INF_PIN_STATUS:
                    result = new PINPadStatus
                    {
                        DeviceState = "ONLINE",
                        EncryptionState = "READY",
                        KeysLoaded = true
                    };
                    return XFSManager.WFS_SUCCESS;

                case WFS_INF_PIN_CAPABILITIES:
                    result = new PINPadCapabilities
                    {
                        CanEBC = true,
                        CanCBC = true,
                        CanMAC = true,
                        CanTripleDES = true
                    };
                    return XFSManager.WFS_SUCCESS;

                default:
                    return XFSManager.WFS_ERR_UNSUPP_COMMAND;
            }
        }

        private int GetPIN(int timeout)
        {
            currentPIN = "1234";
            pinEntered = true;
            Console.WriteLine("[PIN Pad] PIN entered (hidden)");
            return XFSManager.WFS_SUCCESS;
        }

        private int GetPINBlock()
        {
            if (!pinEntered)
            {
                Console.WriteLine("[PIN Pad] No PIN entered");
                return XFSManager.WFS_ERR_USER_ERROR;
            }

            string pinBlock = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(currentPIN));
            Console.WriteLine("[PIN Pad] PIN block created");
            return XFSManager.WFS_SUCCESS;
        }

        private int ResetDevice()
        {
            Console.WriteLine("[PIN Pad] Resetting device");
            currentPIN = "";
            pinEntered = false;
            return XFSManager.WFS_SUCCESS;
        }
    }

    /// <summary>
    /// Printer Service Provider
    /// </summary>
    public class PrinterServiceProvider : ServiceProvider
    {
        public const int WFS_CMD_PTR_PRINT_FORM = 361;
        public const int WFS_CMD_PTR_RESET = 362;
        public const int WFS_INF_PTR_STATUS = 461;

        private int paperLevel = 100;

        public override int Execute(int command, object cmdData, int timeout)
        {
            Console.WriteLine("[Printer] Execute command: " + command);

            switch (command)
            {
                case WFS_CMD_PTR_PRINT_FORM:
                    return PrintReceipt((string)cmdData);

                case WFS_CMD_PTR_RESET:
                    return ResetDevice();

                default:
                    Console.WriteLine("[Printer] Unsupported command: " + command);
                    return XFSManager.WFS_ERR_UNSUPP_COMMAND;
            }
        }

        public override int GetInfo(int category, out object result)
        {
            result = null;
            Console.WriteLine("[Printer] GetInfo category: " + category);

            switch (category)
            {
                case WFS_INF_PTR_STATUS:
                    result = new PrinterStatus
                    {
                        DeviceState = "ONLINE",
                        PaperLevel = paperLevel,
                        TonerLevel = 80
                    };
                    return XFSManager.WFS_SUCCESS;

                default:
                    return XFSManager.WFS_ERR_UNSUPP_COMMAND;
            }
        }

        private int PrintReceipt(string receiptData)
        {
            if (paperLevel <= 0)
            {
                Console.WriteLine("[Printer] Out of paper");
                return XFSManager.WFS_ERR_HARDWARE_ERROR;
            }

            Console.WriteLine("[Printer] Printing receipt...");
            Console.WriteLine(receiptData);
            Thread.Sleep(1500);

            paperLevel -= 1;
            Console.WriteLine("[Printer] Receipt printed successfully");
            return XFSManager.WFS_SUCCESS;
        }

        private int ResetDevice()
        {
            Console.WriteLine("[Printer] Resetting device");
            return XFSManager.WFS_SUCCESS;
        }
    }

    #region Status and Capability Classes

    public class CardReaderStatus
    {
        public string DeviceState { get; set; }
        public bool MediaPresent { get; set; }
        public int RetainBinCount { get; set; }
    }

    public class CardReaderCapabilities
    {
        public bool CanEject { get; set; }
        public bool CanRetain { get; set; }
        public bool CanReadTrack1 { get; set; }
        public bool CanReadTrack2 { get; set; }
        public bool CanReadTrack3 { get; set; }
    }

    public class CashUnit
    {
        public int Denomination { get; set; }
        public int Count { get; set; }
        public string Currency { get; set; }
    }

    public class DispenserStatus
    {
        public string DeviceState { get; set; }
        public string DispenserState { get; set; }
        public string IntermediateStacker { get; set; }
    }

    public class PINPadStatus
    {
        public string DeviceState { get; set; }
        public string EncryptionState { get; set; }
        public bool KeysLoaded { get; set; }
    }

    public class PINPadCapabilities
    {
        public bool CanEBC { get; set; }
        public bool CanCBC { get; set; }
        public bool CanMAC { get; set; }
        public bool CanTripleDES { get; set; }
    }

    public class PrinterStatus
    {
        public string DeviceState { get; set; }
        public int PaperLevel { get; set; }
        public int TonerLevel { get; set; }
    }

    #endregion
}
