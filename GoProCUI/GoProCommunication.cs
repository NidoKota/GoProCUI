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
        private MainWindow _mainWindow;
        private bool _isConnectiong;
        
        public GoProCommunication()
        {
            _mainWindow = new MainWindow();
            
            _mainWindow.Devices.CollectionChanged += OnDevicesCollectionChanged;
            
            _mainWindow.OnChangeStatusText += message =>
            {
                Console.WriteLine($"Status : {DateTime.Now} {message}");
            };
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
        
        private async Task WaitOtherConnecting(CancellationToken token)
        {
            try
            {
                while (_isConnectiong) await Task.Delay(100, token);
            }
            catch (OperationCanceledException)
            {
            }
        }
        
        private async Task DeleteOldConnect()
        {
            if (!_isConnectiong) return;
                
            _cancelTokenSource.Cancel();
            await WaitOtherConnecting(CancellationToken.None);
            _cancelTokenSource = new CancellationTokenSource();
        }
        
        public async void ScanBluetooth()
        {
            await DeleteOldConnect();

            try
            {
                _isConnectiong = true;
                await _mainWindow.BtnScanBLE_Click(_cancelTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"ScanBluetooth Cancel : {DateTime.Now}");
            }
            finally
            {
                _isConnectiong = false;
            }
        }
        
        public async Task ConnectBluetooth(GoProData.Device device)
        {
            await DeleteOldConnect();

            try
            {
                _isConnectiong = true;
                MainWindow.GDeviceInformation gDeviceInformation = _mainWindow.Devices.FirstOrDefault(x => x.DeviceInfo.Id == device.Id);
                await _mainWindow.BtnConnect_Click(gDeviceInformation, _cancelTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"ConnectBluetooth Cancel : {DateTime.Now}");
            }
            finally
            {
                _isConnectiong = false;
            }
        }
    }
}