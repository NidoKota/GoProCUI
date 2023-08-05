using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using NamedPipeWrapper;

namespace GoProCUI
{
    class Program
    {
        static TwoWayNamedPipe _pipe;
        static CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();

        static async Task Main(string[] args)
        {
            if (_pipe != null)
            {
                _pipe.Dispose();
                _cancelTokenSource.Cancel();

                while (_pipe != null) await Task.Delay(100);
            }
            
            _cancelTokenSource = new CancellationTokenSource();

            using (_pipe = new TwoWayNamedPipe())
            {
                await _pipe.Connect();
                _pipe.OnRead += Console.WriteLine;
                _pipe.OnDisconnect += () =>
                {
                    if (_pipe == null) return;
                    Console.WriteLine("OnDisconnect");
                    PipeEnd();
                    
                    _cancelTokenSource.Cancel();
                };

                async Task WriteTask(CancellationToken token)
                {
                    try
                    {
                        await Task.Delay(3000, token);
                        await Task.Delay(1000, token);
                        await _pipe.Write("11");
                        await Task.Delay(1000, token);
                        await _pipe.Write("22");
                        await Task.Delay(1000, token);
                        await _pipe.Write("33");
                        
                        if (_pipe == null) return;
                        Console.WriteLine("WriteEnd Dispose");
                        PipeEnd();
                    }
                    catch (OperationCanceledException)
                    {
                        if (_pipe == null) return;
                        Console.WriteLine("Cancel");
                        PipeEnd();
                    }
                }

                await WriteTask(_cancelTokenSource.Token);
            }

            Console.ReadKey();
        }
        
        static void PipeEnd()
        {
            _pipe = null;
        }
    }
}