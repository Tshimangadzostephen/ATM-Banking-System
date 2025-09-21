using System;
using System.Collections.Generic;

namespace ATMBankingSystem.MockXFS
{
    /// <summary>
    /// Mock XFS Manager - Simulates the XFS (Extensions for Financial Services) layer
    /// XFS is the standard that allows ATM software to talk to hardware devices
    /// </summary>
    public class XFSManager
    {
        // XFS Standard Result Codes (HRESULT)
        public const int WFS_SUCCESS = 0;
        public const int WFS_ERR_INTERNAL_ERROR = -1;
        public const int WFS_ERR_ALREADY_STARTED = -2;
        public const int WFS_ERR_NOT_STARTED = -3;
        public const int WFS_ERR_INVALID_HWND = -4;
        public const int WFS_ERR_INVALID_HSERVICE = -5;
        public const int WFS_ERR_TIMEOUT = -6;
        public const int WFS_ERR_HARDWARE_ERROR = -7;
        public const int WFS_ERR_CONNECTION_LOST = -8;
        public const int WFS_ERR_USER_ERROR = -9;
        public const int WFS_ERR_UNSUPP_COMMAND = -10;

        // XFS Service Classes (Device Types)
        public const int WFS_SERVICE_CLASS_PTR = 1;    // Printer
        public const int WFS_SERVICE_CLASS_IDC = 2;    // Card Reader
        public const int WFS_SERVICE_CLASS_CDM = 3;    // Cash Dispenser
        public const int WFS_SERVICE_CLASS_PIN = 4;    // PIN Pad
        public const int WFS_SERVICE_CLASS_CHK = 5;    // Check Reader
        public const int WFS_SERVICE_CLASS_TTU = 7;    // Text Terminal

        // Manager state
        private bool isStarted = false;
        private Dictionary<string, ServiceProvider> serviceProviders;
        private Dictionary<int, WFSService> openServices;
        private int nextServiceHandle = 1000;
        private EventBus eventBus;

        public XFSManager()
        {
            serviceProviders = new Dictionary<string, ServiceProvider>();
            openServices = new Dictionary<int, WFSService>();
            eventBus = new EventBus();

            Console.WriteLine("[XFS Manager] Created");
        }

        /// <summary>
        /// WFSStartUp - Initialize the XFS Manager
        /// </summary>
        public int WFSStartUp(int versionRequested)
        {
            Console.WriteLine(String.Format("[XFS Manager] WFSStartUp called with version: {0}", versionRequested));

            if (isStarted)
            {
                return WFS_ERR_ALREADY_STARTED;
            }

            try
            {
                // Register default service providers (mock devices)
                RegisterServiceProvider("CardReader", new CardReaderServiceProvider());
                RegisterServiceProvider("CashDispenser", new CashDispenserServiceProvider());
                RegisterServiceProvider("PINPad", new PINPadServiceProvider());
                RegisterServiceProvider("ReceiptPrinter", new PrinterServiceProvider());

                isStarted = true;
                Console.WriteLine("[XFS Manager] Started successfully");
                return WFS_SUCCESS;
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("[XFS Manager] Startup error: {0}", ex.Message));
                return WFS_ERR_INTERNAL_ERROR;
            }
        }

        /// <summary>
        /// WFSCleanUp - Shutdown the XFS Manager
        /// </summary>
        public int WFSCleanUp()
        {
            Console.WriteLine("[XFS Manager] WFSCleanUp called");

            if (!isStarted)
            {
                return WFS_ERR_NOT_STARTED;
            }

            try
            {
                // Close all open services
                foreach (var service in openServices.Values)
                {
                    service.Close();
                }

                openServices.Clear();
                serviceProviders.Clear();
                isStarted = false;

                Console.WriteLine("[XFS Manager] Cleaned up successfully");
                return WFS_SUCCESS;
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("[XFS Manager] Cleanup error: {0}", ex.Message));
                return WFS_ERR_INTERNAL_ERROR;
            }
        }

        /// <summary>
        /// WFSOpen - Open a service (device)
        /// </summary>
        public int WFSOpen(string logicalName, out int hService)
        {
            hService = 0;

            Console.WriteLine(String.Format("[XFS Manager] WFSOpen called for: {0}", logicalName));

            if (!isStarted)
            {
                return WFS_ERR_NOT_STARTED;
            }

            try
            {
                if (!serviceProviders.ContainsKey(logicalName))
                {
                    Console.WriteLine(String.Format("[XFS Manager] Service not found: {0}", logicalName));
                    return WFS_ERR_INVALID_HSERVICE;
                }

                // Create new service handle
                hService = nextServiceHandle++;

                // Create service instance
                var service = new WFSService
                {
                    Handle = hService,
                    LogicalName = logicalName,
                    Provider = serviceProviders[logicalName],
                    IsOpen = true
                };

                openServices[hService] = service;

                Console.WriteLine(String.Format("[XFS Manager] Service opened: {0} (Handle: {1})", logicalName, hService));
                return WFS_SUCCESS;
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("[XFS Manager] Open error: {0}", ex.Message));
                return WFS_ERR_INTERNAL_ERROR;
            }
        }

        /// <summary>
        /// WFSClose - Close a service
        /// </summary>
        public int WFSClose(int hService)
        {
            Console.WriteLine(String.Format("[XFS Manager] WFSClose called for handle: {0}", hService));

            if (!openServices.ContainsKey(hService))
            {
                return WFS_ERR_INVALID_HSERVICE;
            }

            try
            {
                var service = openServices[hService];
                service.Close();
                openServices.Remove(hService);

                Console.WriteLine(String.Format("[XFS Manager] Service closed: {0}", service.LogicalName));
                return WFS_SUCCESS;
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("[XFS Manager] Close error: {0}", ex.Message));
                return WFS_ERR_INTERNAL_ERROR;
            }
        }

        /// <summary>
        /// WFSExecute - Execute a command on a device
        /// </summary>
        public int WFSExecute(int hService, int command, object cmdData, int timeout)
        {
            Console.WriteLine(String.Format("[XFS Manager] WFSExecute - Handle: {0}, Command: {1}", hService, command));

            if (!openServices.ContainsKey(hService))
            {
                return WFS_ERR_INVALID_HSERVICE;
            }

            try
            {
                var service = openServices[hService];
                return service.Provider.Execute(command, cmdData, timeout);
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("[XFS Manager] Execute error: {0}", ex.Message));
                return WFS_ERR_INTERNAL_ERROR;
            }
        }

        /// <summary>
        /// WFSGetInfo - Get information from a device
        /// </summary>
        public int WFSGetInfo(int hService, int category, out object result)
        {
            result = null;

            Console.WriteLine(String.Format("[XFS Manager] WFSGetInfo - Handle: {0}, Category: {1}", hService, category));

            if (!openServices.ContainsKey(hService))
            {
                return WFS_ERR_INVALID_HSERVICE;
            }

            try
            {
                var service = openServices[hService];
                return service.Provider.GetInfo(category, out result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("[XFS Manager] GetInfo error: {0}", ex.Message));
                return WFS_ERR_INTERNAL_ERROR;
            }
        }

        /// <summary>
        /// Register a service provider
        /// </summary>
        private void RegisterServiceProvider(string logicalName, ServiceProvider provider)
        {
            serviceProviders[logicalName] = provider;
            provider.Initialize(eventBus);
            Console.WriteLine(String.Format("[XFS Manager] Registered service provider: {0}", logicalName));
        }
    }

    /// <summary>
    /// Represents an open XFS service
    /// </summary>
    internal class WFSService
    {
        public int Handle { get; set; }
        public string LogicalName { get; set; }
        public ServiceProvider Provider { get; set; }
        public bool IsOpen { get; set; }

        public void Close()
        {
            IsOpen = false;
            Provider.Close();
        }
    }

    /// <summary>
    /// Base class for all service providers (device drivers)
    /// </summary>
    public abstract class ServiceProvider
    {
        protected EventBus eventBus;
        protected bool isInitialized = false;

        public virtual void Initialize(EventBus eventBus)
        {
            this.eventBus = eventBus;
            isInitialized = true;
            Console.WriteLine("[" + GetType().Name + "] Initialized");
        }

        public virtual void Close()
        {
            isInitialized = false;
            Console.WriteLine("[" + GetType().Name + "] Closed");
        }

        public abstract int Execute(int command, object cmdData, int timeout);
        public abstract int GetInfo(int category, out object result);
    }

    /// <summary>
    /// Simple event bus for XFS events
    /// </summary>
    public class EventBus
    {
        private Dictionary<string, List<Action<XFSEvent>>> listeners;

        public EventBus()
        {
            listeners = new Dictionary<string, List<Action<XFSEvent>>>();
        }

        public void Subscribe(string eventType, Action<XFSEvent> handler)
        {
            if (!listeners.ContainsKey(eventType))
            {
                listeners[eventType] = new List<Action<XFSEvent>>();
            }
            listeners[eventType].Add(handler);
        }

        public void Publish(XFSEvent xfsEvent)
        {
            if (listeners.ContainsKey(xfsEvent.EventType))
            {
                foreach (var handler in listeners[xfsEvent.EventType])
                {
                    handler(xfsEvent);
                }
            }
        }
    }

    /// <summary>
    /// Represents an XFS event
    /// </summary>
    public class XFSEvent
    {
        public string EventType { get; set; }
        public int ServiceHandle { get; set; }
        public int EventId { get; set; }
        public object EventData { get; set; }
        public DateTime Timestamp { get; set; }

        public XFSEvent()
        {
            Timestamp = DateTime.Now;
        }
    }
}
