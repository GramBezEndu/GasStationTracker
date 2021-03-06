namespace Memory
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Information about the opened process.
    /// </summary>
    public class ProcessInfo
    {
        public Process Process { get; set; }

        public IntPtr Handle { get; set; }

        public bool Is64Bit { get; set; }

        public Dictionary<string, IntPtr> Modules { get; set; }

        public ProcessModule MainModule { get; set; }
    }
}
