using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Framework.Core
{
    public class Disposable : IDisposable
    {
        // Flag: Has Dispose already been called?
        private bool isDisposed;

        // Instantiate a SafeHandle instance.
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        ~Disposable()
        {
            Dispose(false);
        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        private void Dispose(bool disposing)
        {
            if (!isDisposed && disposing)
            {
                handle.Dispose();

                // Free any other managed objects here.
                DisposeCore();
            }

            // Free any unmanaged objects.
            isDisposed = true;
        }

        // Protected implementation of Dispose pattern.
        protected virtual void DisposeCore()
        {
        } 
    }
}

