using Lexplosion.Global;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Lexplosion.Logic.Management
{
    class Utils
    {
        private const int ErrorInsufficientBuffer = 122;
        private const int Successfully = 0;

        public enum IpVersion : uint
        {
            IPv4 = 2,
            IPv6 = 23
        }

        internal enum UdpTableClass
        {
            UdpTableBasic,
            UdpTableOwnerPid,
            UdpTableOwnerModule
        }

        public enum ProcessExecutor
        {
            Java,
            Cmd
        }

        public struct UdpRowOwnerPid
        {
            public uint LocalAddr;
            public uint LocalPort;
            public uint OwningPid;
        }

        [System.Runtime.InteropServices.DllImport("iphlpapi.dll", SetLastError = true)]
        private static extern uint GetExtendedUdpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool sort, IpVersion ipVersion, UdpTableClass tblClass, int reserved);

        public static List<ushort> GetProcessUdpPorts(int pid)
        {
            UdpRowOwnerPid[] tTable = null;

            int buffSize = 0;

            // how much memory do we need?
            GetExtendedUdpTable(IntPtr.Zero, ref buffSize, false, IpVersion.IPv4, UdpTableClass.UdpTableOwnerPid, 0);

            IntPtr buffer = Marshal.AllocHGlobal(buffSize);
            try
            {
                uint retVal = GetExtendedUdpTable(buffer, ref buffSize, false, IpVersion.IPv4, UdpTableClass.UdpTableOwnerPid, 0);

                while (retVal == ErrorInsufficientBuffer) //buffer should be greater?
                {
                    buffer = Marshal.ReAllocHGlobal(buffer, new IntPtr(buffSize));
                    retVal = GetExtendedUdpTable(buffer, ref buffSize, false, IpVersion.IPv4, UdpTableClass.UdpTableOwnerPid, 0);
                }

                if (retVal != Successfully)
                    return null;

                Type rowType = typeof(UdpRowOwnerPid);
                int sizeRow = Marshal.SizeOf(rowType);
                long buffAddress = buffer.ToInt64();

                int count = Marshal.ReadInt32(buffer);
                int offcet = Marshal.SizeOf(typeof(Int32));

                tTable = new UdpRowOwnerPid[count];
                for (int i = 0; i < count; i++)
                {
                    //calc position for next array element
                    var memoryPos = new IntPtr(buffAddress + offcet);
                    //read element
                    tTable[i] = (UdpRowOwnerPid)Marshal.PtrToStructure(memoryPos, rowType);

                    offcet += sizeRow;
                }

            }
            catch
            {
                return null;
            }
            finally
            {
                // Free the Memory
                Marshal.FreeHGlobal(buffer);
            }

            List<ushort> data = new List<ushort>();
            for (int i = 0; i < tTable.Length; i++)
            {
                if (tTable[i].OwningPid == pid)
                {
                    uint port = tTable[i].LocalPort;
                    var b = new byte[2];
                    // high weight byte
                    b[0] = (byte)(port >> 8);
                    // low weight byte
                    b[1] = (byte)(port & 255);

                    data.Add(BitConverter.ToUInt16(b, 0));
                }

            }

            return data;
        }

        public static bool StartProcess(string command, ProcessExecutor executor)
        {
            string fileName = "";

            if (executor == ProcessExecutor.Cmd)
            {
                fileName = "cmd.exe";
                command = "/C " + command;
            }
            else if (executor == ProcessExecutor.Java)
            {
                fileName = UserData.GeneralSettings.JavaPath;
            }
            else
            {
                return false;
            }

            try
            {
                Process process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = fileName;
                process.StartInfo.Arguments = command;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WorkingDirectory = UserData.GeneralSettings.GamePath;
                process.Start();
                return process.WaitForExit(300000); // ждём 5 минут
            }
            catch
            {
                return false;
            }
        }
    }
}
