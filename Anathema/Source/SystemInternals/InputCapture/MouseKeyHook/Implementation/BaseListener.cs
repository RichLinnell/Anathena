using Anathema.Source.SystemInternals.InputCapture.MouseKeyHook.WinApi;
using System;

namespace Anathema.Source.SystemInternals.InputCapture.MouseKeyHook.Implementation
{
    internal abstract class BaseListener : IDisposable
    {
        protected BaseListener(Subscribe Subscribe)
        {
            Handle = Subscribe(Callback);
        }

        protected HookResult Handle { get; set; }

        public void Dispose()
        {
            Handle.Dispose();
        }

        protected abstract bool Callback(CallbackData Data);

    } // End class

} // End namespace