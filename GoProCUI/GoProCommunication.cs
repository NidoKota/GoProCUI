using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GoProCSharpSample;

namespace GoProCUI
{
    public class GoProCommunication
    {
        public GoProData GoProData { get; private set; } = new GoProData();
        public event Action OnGoProDataChanged;
        
        private CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
        private readonly MainWindow _mainWindow = new MainWindow();
        private bool _isConnecting;
        
        public GoProCommunication()
        {
            _mainWindow.Devices.CollectionChanged += OnDevicesCollectionChanged;
            _mainWindow.OnChangeStatusText += OnOnChangeStatusText;
        }

        private void OnDevicesCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            GoProData.Devices.Clear();
            foreach (var dev in _mainWindow.Devices)
            {
                var goProDataDevice = new GoProData.Device {Name = dev.DeviceInfo.Name, Id = dev.DeviceInfo.Id};
                GoProData.Devices.Add(goProDataDevice);
            }
            OnGoProDataChanged?.Invoke();
        }
        
        private void OnOnChangeStatusText(string message)
        {
            Console.WriteLine($"Status : {DateTime.Now} {message}");
            GoProData.Message = message;
            OnGoProDataChanged?.Invoke();
        }
        
        private async Task WaitOtherConnecting(CancellationToken token)
        {
            try
            {
                while (_isConnecting) await Task.Delay(100, token);
            }
            catch (OperationCanceledException)
            {
            }
        }
        
        private async Task DeleteOldConnect()
        {
            if (!_isConnecting) return;
                
            _cancelTokenSource.Cancel();
            await WaitOtherConnecting(CancellationToken.None);
            _cancelTokenSource = new CancellationTokenSource();
        }
        
        public async void ScanBluetooth()
        {
            await DeleteOldConnect();

            try
            {
                _isConnecting = true;
                await _mainWindow.BtnScanBLE_Click(_cancelTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"ScanBluetooth Cancel : {DateTime.Now}");
            }
            finally
            {
                _isConnecting = false;
            }
        }
        
        public async Task ConnectBluetooth(GoProData.Device device)
        {
            await DeleteOldConnect();

            try
            {
                _isConnecting = true;
                MainWindow.GDeviceInformation gDeviceInformation = _mainWindow.Devices.FirstOrDefault(x => x.DeviceInfo.Id == device.Id);
                await _mainWindow.BtnConnect_Click(gDeviceInformation, _cancelTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"ConnectBluetooth Cancel : {DateTime.Now}");
            }
            finally
            {
                _isConnecting = false;
            }
        }
    }
}