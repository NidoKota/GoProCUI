using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using NamedPipeWrapper;

namespace GoProCUI
{
    class GoProCUI
    {
        private static readonly TaskCompletionSource<int> Source = new TaskCompletionSource<int>();

        static async Task Main(string[] args)
        {
            GoProCUI goProCui = new GoProCUI();
            goProCui.Init();
            goProCui.InitGoProCommunication();

            await Source.Task;
            Console.ReadKey();
        }

        private static TwoWayNamedPipe _pipe;
        private static CancellationTokenSource _cancelOldPipeTokenSource;
        private static bool _isWriting;

        private async void Init()
        {
            await DeleteOldPipe();

            _cancelOldPipeTokenSource = new CancellationTokenSource();
            _pipe = new TwoWayNamedPipe(debug: true);
            {
                await _pipe.Connect();

                _pipe.OnRead += OnPipeRead;
                _pipe.OnDisconnect += OnPipeDisconnect;
            }
        }

        private async void OnPipeDisconnect()
        {
            if (_pipe != null) await PipeEnd();
        }

        private async Task DeleteOldPipe()
        {
            if (_pipe != null) await PipeEnd();
        }

        private async Task WaitOtherWriting(CancellationToken token)
        {
            try
            {
                while (_isWriting) await Task.Delay(100, token);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task PipeEnd()
        {
            _cancelOldPipeTokenSource.Cancel();
            await WaitOtherWriting(CancellationToken.None);

            _pipe.Dispose();
            _pipe = null;

            Source.SetResult(0);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private GoProCommunication _goProCommunication = new GoProCommunication();

        private void InitGoProCommunication()
        {
            _goProCommunication.OnGoProDataChanged += OnGoProDataChanged;
        }

        private void OnGoProDataChanged()
        {
            WriteGoProData();
        }

        private async void OnPipeRead(string message)
        {
            switch (message)
            {
                case "ScanBluetooth":
                    _goProCommunication.ScanBluetooth();
                    break;
                case "ConnectBluetooth":
                    await _goProCommunication.ConnectBluetooth(_goProCommunication.GoProData.Devices[0]);
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        private async void WriteGoProData()
        {
            string json = JsonConvert.SerializeObject(_goProCommunication.GoProData);
            
            try
            {
                await WaitOtherWriting(_cancelOldPipeTokenSource.Token);
                
                _isWriting = true;
                await _pipe.Write(json, _cancelOldPipeTokenSource.Token); 
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _isWriting = false;
            }
        }
    }
}