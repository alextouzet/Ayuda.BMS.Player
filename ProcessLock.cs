using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Ayuda.Diagnostics;

namespace Ayuda.BMS.Player
{
    internal class ProcessLock : IDisposable
    {
        private string _appName;
        private Mutex _mutex;
        private bool _created;
        private bool _disposed;

        public bool Exists
        {
            get { return !_created; }
        }

        public ProcessLock(string appName)
        {
            _appName = appName;
            TryLock();
        }

        public bool TryLock()
        {
            if (_disposed)
                throw new ObjectDisposedException("ProcessLock");
            if (_created)
                throw new InvalidOperationException();
            if (_mutex != null)
                _mutex.Close();

            _mutex = new Mutex(true, _appName, out _created);

            return _created;
        }

        public void Unlock()
        {
            if (_disposed)
                throw new ObjectDisposedException("ProcessLock");
            if (!_created)
                throw new InvalidOperationException();

            Asserter.Assert(_mutex != null, "_mutex != null", "_mutex cannot be null");

            _mutex.ReleaseMutex();
            _mutex = null;
            _created = false;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_mutex != null)
            {
                if (_created)
                {
                    Unlock();
                }
                else
                {
                    _mutex.Close();
                    _mutex = null;
                }
            }

            _disposed = true;
        }

        #endregion
    }
}
