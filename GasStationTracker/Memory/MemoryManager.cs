namespace Memory
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.IO.Pipes;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Text;
    using static Memory.Imports;

    /// <summary>
    /// Memory.dll class. Full documentation at https://github.com/erfg12/memory.dll/wiki.
    /// </summary>
    public partial class MemoryManager
    {
        private ProcessInfo processInfo = new ProcessInfo();

        public static void SuspendProcess(int pid)
        {
            Process process = System.Diagnostics.Process.GetProcessById(pid);

            if (process.ProcessName == string.Empty)
            {
                return;
            }

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);
                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                SuspendThread(pOpenThread);
                CloseHandle(pOpenThread);
            }
        }

        public static void ResumeProcess(int pid)
        {
            Process process = System.Diagnostics.Process.GetProcessById(pid);
            if (process.ProcessName == string.Empty)
            {
                return;
            }

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);
                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                int suspendCount;
                do
                {
                    suspendCount = ResumeThread(pOpenThread);
                }
                while (suspendCount > 0);
                CloseHandle(pOpenThread);
            }
        }

        /// <summary>
        /// Convert a byte array to hex values in a string.
        /// </summary>
        /// <param name="ba">Your byte array to convert.</param>
        /// <returns>Hex string.</returns>
        public static string ByteArrayToHexString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            int i = 1;
            foreach (byte b in ba)
            {
                if (i == 16)
                {
                    hex.AppendFormat("{0:x2}{1}", b, Environment.NewLine);
                    i = 0;
                }
                else
                {
                    hex.AppendFormat("{0:x2} ", b);
                }

                i++;
            }

            return hex.ToString().ToUpper();
        }

        public static string ByteArrayToString(byte[] byteArray)
        {
            StringBuilder hex = new StringBuilder(byteArray.Length * 2);
            foreach (byte b in byteArray)
            {
                hex.AppendFormat("{0:x2} ", b);
            }

            return hex.ToString();
        }

        /// <summary>
        /// Open the PC game process with all security and access rights.
        /// </summary>
        /// <param name="pid">Use process name or process ID here.</param>
        /// <returns>Process opened successfully or failed.</returns>
        public bool OpenProcess(int pid)
        {
            if (processInfo == null)
            {
                processInfo = new ProcessInfo();
            }

            if (!IsAdmin())
            {
                Debug.WriteLine("WARNING: This program may not be running with raised privileges! Visit https://github.com/erfg12/memory.dll/wiki/Administrative-Privileges");
            }

            if (pid <= 0)
            {
                Debug.WriteLine("ERROR: OpenProcess given proc ID 0.");
                return false;
            }

            if (processInfo.Process != null && processInfo.Process.Id == pid)
            {
                return true;
            }

            try
            {
                processInfo.Process = System.Diagnostics.Process.GetProcessById(pid);

                if (processInfo.Process != null && !processInfo.Process.Responding)
                {
                    Debug.WriteLine("ERROR: OpenProcess: Process is not responding or null.");
                    return false;
                }

                processInfo.Handle = Imports.OpenProcess(0x1F0FFF, true, pid);

                try
                {
                    System.Diagnostics.Process.EnterDebugMode();
                }
                catch (Win32Exception)
                {
                    Debug.WriteLine("WARNING: You are not running with raised privileges! Visit https://github.com/erfg12/memory.dll/wiki/Administrative-Privileges");
                }

                if (processInfo.Handle == IntPtr.Zero)
                {
                    int eCode = Marshal.GetLastWin32Error();
                    Debug.WriteLine("ERROR: OpenProcess has failed opening a handle to the target process (GetLastWin32ErrorCode: " + eCode + ")");
                    System.Diagnostics.Process.LeaveDebugMode();
                    processInfo = null;
                    return false;
                }

                // Lets set the process to 64bit or not here (cuts down on api calls)
                processInfo.Is64Bit = Environment.Is64BitOperatingSystem && IsWow64Process(processInfo.Handle, out bool retVal) && !retVal;

                processInfo.MainModule = processInfo.Process.MainModule;

                GetModules();

                Debug.WriteLine("Process #" + processInfo.Process + " is now open.");

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERROR: OpenProcess has crashed. " + ex);
                return false;
            }
        }

        /// <summary>
        /// Open the PC game process with all security and access rights.
        /// </summary>
        /// <param name="proc">Use process name or process ID here.</param>
        /// <returns>Process opened successfully or failed.</returns>
        public bool OpenProcess(string proc)
        {
            return OpenProcess(GetProcIdFromName(proc));
        }

        /// <summary>
        /// Check if program is running with administrative privileges.
        /// Read about it here: https://github.com/erfg12/memory.dll/wiki/Administrative-Privileges.
        /// </summary>
        /// <returns>True if running with administrative privileges.</returns>
        public bool IsAdmin()
        {
            try
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                Debug.WriteLine("ERROR: Could not determin if program is running as admin. Is the NuGet package \"System.Security.Principal.Windows\" missing?");
                return false;
            }
        }

        /// <summary>
        /// Builds the process modules dictionary (names with addresses).
        /// </summary>
        public void GetModules()
        {
            if (processInfo.Process == null)
            {
                return;
            }

            if (processInfo.Is64Bit && IntPtr.Size != 8)
            {
                Debug.WriteLine("WARNING: Game is x64, but your Trainer is x86! You will be missing some modules, change your Trainer's Solution Platform.");
            }
            else if (!processInfo.Is64Bit && IntPtr.Size == 8)
            {
                Debug.WriteLine("WARNING: Game is x86, but your Trainer is x64! You will be missing some modules, change your Trainer's Solution Platform.");
            }

            processInfo.Modules = new Dictionary<string, IntPtr>();

            foreach (ProcessModule module in processInfo.Process.Modules)
            {
                if (!string.IsNullOrEmpty(module.ModuleName) && !processInfo.Modules.ContainsKey(module.ModuleName))
                {
                    processInfo.Modules.Add(module.ModuleName, module.BaseAddress);
                }
            }

            Debug.WriteLine("Found " + processInfo.Modules.Count() + " process modules.");
        }

        public void SetFocus()
        {
            SetForegroundWindow(processInfo.Process.MainWindowHandle);
        }

        /// <summary>
        /// Get the process ID number by process name.
        /// </summary>
        /// <param name="name">Example: "eqgame". Use task manager to find the name. Do not include ".exe".</param>
        /// <returns>Process Id or 0 when not found.</returns>
        public int GetProcIdFromName(string name)
        {
            Process[] processlist = Process.GetProcesses();

            if (name.ToLower().Contains(".exe"))
            {
                name = name.Replace(".exe", string.Empty);
            }

            if (name.ToLower().Contains(".bin"))
            {
                name = name.Replace(".bin", string.Empty);
            }

            foreach (System.Diagnostics.Process theprocess in processlist)
            {
                if (theprocess.ProcessName.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                {
                    return theprocess.Id;
                }
            }

            return 0;
        }

        /// <summary>
        /// Get code. If just the ini file name is given with no path, it will assume the file is next to the executable.
        /// </summary>
        /// <param name="name">label for address or code.</param>
        /// <param name="iniFile">path and name of ini file.</param>
        /// <returns>Code.</returns>
        public string LoadCode(string name, string iniFile)
        {
            StringBuilder returnCode = new StringBuilder(1024);

            if (iniFile != string.Empty)
            {
                if (File.Exists(iniFile))
                {
                    GetPrivateProfileString(
                        "codes",
                        name,
                        string.Empty,
                        returnCode,
                        (uint)returnCode.Capacity,
                        iniFile);
                }
                else
                {
                    Debug.WriteLine("ERROR: ini file \"" + iniFile + "\" not found!");
                }
            }
            else
            {
                returnCode.Append(name);
            }

            return returnCode.ToString();
        }

        /// <summary>
        /// Make a named pipe (if not already made) and call to a remote function.
        /// </summary>
        /// <param name="func">remote function to call.</param>
        /// <param name="name">name of the thread.</param>
        public void ThreadStartClient(string func, string name)
        {
            using (NamedPipeClientStream pipeStream = new NamedPipeClientStream(name))
            {
                if (!pipeStream.IsConnected)
                {
                    pipeStream.Connect();
                }

                using (StreamWriter sw = new StreamWriter(pipeStream))
                {
                    if (!sw.AutoFlush)
                    {
                        sw.AutoFlush = true;
                    }

                    sw.WriteLine(func);
                }
            }
        }

        public bool ChangeProtection(string code, MemoryProtection newProtection, out MemoryProtection oldProtection, string file = "")
        {
            UIntPtr theCode = GetCode(code, file);
            if (theCode == UIntPtr.Zero
                || processInfo.Handle == IntPtr.Zero)
            {
                oldProtection = default;
                return false;
            }

            return VirtualProtectEx(processInfo.Handle, theCode, (IntPtr)(processInfo.Is64Bit ? 8 : 4), newProtection, out oldProtection);
        }

        /// <summary>
        /// Convert code from string to real address. If path is not blank, will pull from ini file.
        /// </summary>
        /// <param name="name">label in ini file or code.</param>
        /// <param name="path">path to ini file (OPTIONAL).</param>
        /// <param name="size">size of address (default is 8).</param>
        /// <returns>Address.</returns>
        public UIntPtr GetCode(string name, string path = "", int size = 8)
        {
            string theCode = string.Empty;
            if (processInfo.Is64Bit)
            {
                if (size == 8)
                {
                    // change to 64bit
                    size = 16;
                }

                // jump over to 64bit code grab
                return Get64BitCode(name, path, size);
            }

            if (path != string.Empty)
            {
                theCode = LoadCode(name, path);
            }
            else
            {
                theCode = name;
            }

            if (theCode == string.Empty)
            {
                Debug.WriteLine("ERROR: LoadCode returned blank. NAME:" + name + " PATH:" + path);
                return UIntPtr.Zero;
            }

            // remove spaces
            if (theCode.Contains(" "))
            {
                theCode = theCode.Replace(" ", string.Empty);
            }

            if (!theCode.Contains("+") && !theCode.Contains(","))
            {
                return new UIntPtr(Convert.ToUInt32(theCode, 16));
            }

            string newOffsets = theCode;

            if (theCode.Contains("+"))
            {
                newOffsets = theCode[(theCode.IndexOf('+') + 1) ..];
            }

            byte[] memoryAddress = new byte[size];

            if (newOffsets.Contains(','))
            {
                List<int> offsetsList = new List<int>();

                string[] newerOffsets = newOffsets.Split(',');
                foreach (string oldOffsets in newerOffsets)
                {
                    string test = oldOffsets;
                    if (oldOffsets.Contains("0x"))
                    {
                        test = oldOffsets.Replace("0x", string.Empty);
                    }

                    int preParse = 0;
                    if (!oldOffsets.Contains("-"))
                    {
                        preParse = Int32.Parse(test, NumberStyles.AllowHexSpecifier);
                    }
                    else
                    {
                        test = test.Replace("-", string.Empty);
                        preParse = Int32.Parse(test, NumberStyles.AllowHexSpecifier);
                        preParse *= -1;
                    }

                    offsetsList.Add(preParse);
                }

                int[] offsets = offsetsList.ToArray();

                if (theCode.Contains("base") || theCode.Contains("main"))
                {
                    ReadProcessMemory(processInfo.Handle, (UIntPtr)((int)processInfo.MainModule.BaseAddress + offsets[0]), memoryAddress, (UIntPtr)size, IntPtr.Zero);
                }
                else if (!theCode.Contains("base") && !theCode.Contains("main") && theCode.Contains("+"))
                {
                    string[] moduleName = theCode.Split('+');
                    IntPtr altModule = IntPtr.Zero;
                    if (!moduleName[0].ToLower().Contains(".dll") && !moduleName[0].ToLower().Contains(".exe") && !moduleName[0].ToLower().Contains(".bin"))
                    {
                        string theAddr = moduleName[0];
                        if (theAddr.Contains("0x"))
                        {
                            theAddr = theAddr.Replace("0x", string.Empty);
                        }

                        altModule = (IntPtr)Int32.Parse(theAddr, NumberStyles.HexNumber);
                    }
                    else
                    {
                        try
                        {
                            altModule = processInfo.Modules[moduleName[0]];
                        }
                        catch
                        {
                            Debug.WriteLine("Module " + moduleName[0] + " was not found in module list!");
                            Debug.WriteLine("Modules: " + string.Join(",", processInfo.Modules));
                        }
                    }

                    ReadProcessMemory(processInfo.Handle, (UIntPtr)((int)altModule + offsets[0]), memoryAddress, (UIntPtr)size, IntPtr.Zero);
                }
                else
                {
                    ReadProcessMemory(processInfo.Handle, (UIntPtr)offsets[0], memoryAddress, (UIntPtr)size, IntPtr.Zero);
                }

                // ToUInt64 causes arithmetic overflow.
                uint num1 = BitConverter.ToUInt32(memoryAddress, 0);

                UIntPtr base1 = (UIntPtr)0;

                for (int i = 1; i < offsets.Length; i++)
                {
                    base1 = new UIntPtr(Convert.ToUInt32(num1 + offsets[i]));
                    ReadProcessMemory(processInfo.Handle, base1, memoryAddress, (UIntPtr)size, IntPtr.Zero);

                    // ToUInt64 causes arithmetic overflow.
                    num1 = BitConverter.ToUInt32(memoryAddress, 0);
                }

                return base1;
            }
            else
            {
                int trueCode = Convert.ToInt32(newOffsets, 16);
                IntPtr altModule = IntPtr.Zero;
                if (theCode.ToLower().Contains("base") || theCode.ToLower().Contains("main"))
                {
                    altModule = processInfo.MainModule.BaseAddress;
                }
                else if (!theCode.ToLower().Contains("base") && !theCode.ToLower().Contains("main") && theCode.Contains("+"))
                {
                    string[] moduleName = theCode.Split('+');
                    if (!moduleName[0].ToLower().Contains(".dll") && !moduleName[0].ToLower().Contains(".exe") && !moduleName[0].ToLower().Contains(".bin"))
                    {
                        string theAddr = moduleName[0];
                        if (theAddr.Contains("0x"))
                        {
                            theAddr = theAddr.Replace("0x", string.Empty);
                        }

                        altModule = (IntPtr)Int32.Parse(theAddr, NumberStyles.HexNumber);
                    }
                    else
                    {
                        try
                        {
                            altModule = processInfo.Modules[moduleName[0]];
                        }
                        catch
                        {
                            Debug.WriteLine("Module " + moduleName[0] + " was not found in module list!");
                            Debug.WriteLine("Modules: " + string.Join(",", processInfo.Modules));
                        }
                    }
                }
                else
                {
                    altModule = processInfo.Modules[theCode.Split('+')[0]];
                }

                return (UIntPtr)((int)altModule + trueCode);
            }
        }

        /// <summary>
        /// Convert code from string to real address. If path is not blank, will pull from ini file.
        /// </summary>
        /// <param name="name">label in ini file OR code.</param>
        /// <param name="path">path to ini file (OPTIONAL).</param>
        /// <param name="size">size of address (default is 16).</param>
        /// <returns>Address.</returns>
        public UIntPtr Get64BitCode(string name, string path = "", int size = 16)
        {
            string theCode = string.Empty;
            if (path != string.Empty)
            {
                theCode = LoadCode(name, path);
            }
            else
            {
                theCode = name;
            }

            if (theCode == string.Empty)
            {
                return UIntPtr.Zero;
            }

            // remove spaces
            if (theCode.Contains(" "))
            {
                theCode.Replace(" ", string.Empty);
            }

            string newOffsets = theCode;
            if (theCode.Contains("+"))
            {
                newOffsets = theCode[(theCode.IndexOf('+') + 1) ..];
            }

            byte[] memoryAddress = new byte[size];

            if (!theCode.Contains("+") && !theCode.Contains(","))
            {
                return new UIntPtr(Convert.ToUInt64(theCode, 16));
            }

            if (newOffsets.Contains(','))
            {
                List<Int64> offsetsList = new List<Int64>();

                string[] newerOffsets = newOffsets.Split(',');
                foreach (string oldOffsets in newerOffsets)
                {
                    string test = oldOffsets;
                    if (oldOffsets.Contains("0x"))
                    {
                        test = oldOffsets.Replace("0x", string.Empty);
                    }

                    Int64 preParse = 0;
                    if (!oldOffsets.Contains("-"))
                    {
                        preParse = Int64.Parse(test, NumberStyles.HexNumber);
                    }
                    else
                    {
                        test = test.Replace("-", string.Empty);
                        preParse = Int64.Parse(test, NumberStyles.AllowHexSpecifier);
                        preParse *= -1;
                    }

                    offsetsList.Add(preParse);
                }

                Int64[] offsets = offsetsList.ToArray();

                if (theCode.Contains("base") || theCode.Contains("main"))
                {
                    ReadProcessMemory(processInfo.Handle, (UIntPtr)((Int64)processInfo.MainModule.BaseAddress + offsets[0]), memoryAddress, (UIntPtr)size, IntPtr.Zero);
                }
                else if (!theCode.Contains("base") && !theCode.Contains("main") && theCode.Contains("+"))
                {
                    string[] moduleName = theCode.Split('+');
                    IntPtr altModule = IntPtr.Zero;
                    if (!moduleName[0].ToLower().Contains(".dll") && !moduleName[0].ToLower().Contains(".exe") && !moduleName[0].ToLower().Contains(".bin"))
                    {
                        altModule = (IntPtr)Int64.Parse(moduleName[0], System.Globalization.NumberStyles.HexNumber);
                    }
                    else
                    {
                        try
                        {
                            altModule = processInfo.Modules[moduleName[0]];
                        }
                        catch
                        {
                            Debug.WriteLine("Module " + moduleName[0] + " was not found in module list!");
                            Debug.WriteLine("Modules: " + string.Join(",", processInfo.Modules));
                        }
                    }

                    ReadProcessMemory(processInfo.Handle, (UIntPtr)((Int64)altModule + offsets[0]), memoryAddress, (UIntPtr)size, IntPtr.Zero);
                }
                else
                {
                    // No offsets case
                    ReadProcessMemory(processInfo.Handle, (UIntPtr)offsets[0], memoryAddress, (UIntPtr)size, IntPtr.Zero);
                }

                Int64 num1 = BitConverter.ToInt64(memoryAddress, 0);

                UIntPtr base1 = (UIntPtr)0;

                for (int i = 1; i < offsets.Length; i++)
                {
                    base1 = new UIntPtr(Convert.ToUInt64(num1 + offsets[i]));
                    ReadProcessMemory(processInfo.Handle, base1, memoryAddress, (UIntPtr)size, IntPtr.Zero);
                    num1 = BitConverter.ToInt64(memoryAddress, 0);
                }

                return base1;
            }
            else
            {
                Int64 trueCode = Convert.ToInt64(newOffsets, 16);
                IntPtr altModule = IntPtr.Zero;
                if (theCode.Contains("base") || theCode.Contains("main"))
                {
                    altModule = processInfo.MainModule.BaseAddress;
                }
                else if (!theCode.Contains("base") && !theCode.Contains("main") && theCode.Contains("+"))
                {
                    string[] moduleName = theCode.Split('+');
                    if (!moduleName[0].ToLower().Contains(".dll") && !moduleName[0].ToLower().Contains(".exe") && !moduleName[0].ToLower().Contains(".bin"))
                    {
                        string theAddr = moduleName[0];
                        if (theAddr.Contains("0x"))
                        {
                            theAddr = theAddr.Replace("0x", string.Empty);
                        }

                        altModule = (IntPtr)Int64.Parse(theAddr, NumberStyles.HexNumber);
                    }
                    else
                    {
                        try
                        {
                            altModule = processInfo.Modules[moduleName[0]];
                        }
                        catch
                        {
                            Debug.WriteLine("Module " + moduleName[0] + " was not found in module list!");
                            Debug.WriteLine("Modules: " + string.Join(",", processInfo.Modules));
                        }
                    }
                }
                else
                {
                    altModule = processInfo.Modules[theCode.Split('+')[0]];
                }

                return (UIntPtr)((Int64)altModule + trueCode);
            }
        }

        /// <summary>
        /// Close the process when finished.
        /// </summary>
        public void CloseProcess()
        {
            if (processInfo.Handle == null)
            {
                return;
            }

            CloseHandle(processInfo.Handle);
            processInfo = null;
        }

        public byte[] FileToBytes(string path, bool dontDelete = false)
        {
            byte[] newArray = File.ReadAllBytes(path);
            if (!dontDelete)
            {
                File.Delete(path);
            }

            return newArray;
        }

        public string MSize()
        {
            if (processInfo.Is64Bit)
            {
                return "x16";
            }
            else
            {
                return "x8";
            }
        }

        private int LoadIntCode(string name, string path)
        {
            try
            {
                int intValue = Convert.ToInt32(LoadCode(name, path), 16);
                if (intValue >= 0)
                {
                    return intValue;
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                Debug.WriteLine("ERROR: LoadIntCode function crashed!");
                return 0;
            }
        }
    }
}
