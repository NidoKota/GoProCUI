using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using NamedPipeWrapper;

namespace GoProCUI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            TwoWayNamedPipe pipe = new TwoWayNamedPipe();
            pipe.OnRead += Console.WriteLine;

            await pipe.OnDisposeTask;
            
            Console.WriteLine("Dispose");
            
            await Task.Delay(1000);
        }
    }
}
