using Lexplosion.Global;
using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
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

		private struct UdpRowOwnerPid
		{
			public uint LocalAddr;
			public uint LocalPort;
			public uint OwningPid;
		}

		private enum TcpTableClass
		{
			TcpTableBasicListener,
			TcpTableBasicConnections,
			TcpTableBasicAll,
			TcpTableOwnerPidListener,
			TcpTableOwnerPidConnections,
			TcpTableOwnerPidAll,
			TcpTableOwnerModuleListener,
			TcpTableOwnerModuleConnections,
			TcpTableOwnerModuleAll
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct TcpRowOwnerPid
		{
			public TcpState State;
			public uint LocalAddr;
			public uint LocalPort;
			public uint RemoteAddr;
			public uint RemotePort;
			public uint OwningPid;
		}

		private static void ReadData<T>(IntPtr buffer, out T[] tTable)
		{
			Type rowType = typeof(T);
			int sizeRow = Marshal.SizeOf(rowType);
			long buffAddress = buffer.ToInt64();

			int count = Marshal.ReadInt32(buffer);
			int offcet = Marshal.SizeOf(typeof(Int32));

			tTable = new T[count];
			for (int i = 0; i < tTable.Length; i++)
			{
				//calc position for next array element
				var memoryPos = new IntPtr(buffAddress + offcet);
				//read element
				tTable[i] = (T)Marshal.PtrToStructure(memoryPos, rowType);

				offcet += sizeRow;
			}
		}

		[DllImport("iphlpapi.dll", SetLastError = true)]
		private static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool sort,
			IpVersion ipVersion, TcpTableClass tblClass, int reserved);

		[DllImport("iphlpapi.dll", SetLastError = true)]
		private static extern uint GetExtendedUdpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool sort, IpVersion ipVersion, UdpTableClass tblClass, int reserved);

		public static bool ContainsUdpPort(int pid, int port)
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
					return false;

				ReadData(buffer, out tTable);
			}
			catch
			{
				return false;
			}
			finally
			{
				// Free the Memory
				Marshal.FreeHGlobal(buffer);
			}

			for (int i = 0; i < tTable.Length; i++)
			{
				if (tTable[i].OwningPid == pid)
				{
					uint _port = tTable[i].LocalPort;
					var b = new byte[2];
					// high weight byte
					b[0] = (byte)(_port >> 8);
					// low weight byte
					b[1] = (byte)(_port & 255);

					if (BitConverter.ToUInt16(b, 0) == port)
					{
						return true;
					}
				}
			}

			return false;
		}

		public static bool ContainsTcpPort(int pid, int port)
		{
			TcpRowOwnerPid[] tTable;

			int buffSize = 0;

			// how much memory do we need?
			GetExtendedTcpTable(IntPtr.Zero, ref buffSize, false, IpVersion.IPv4, TcpTableClass.TcpTableOwnerPidAll, 0);

			IntPtr buffer = Marshal.AllocHGlobal(buffSize);
			try
			{
				uint retVal = GetExtendedTcpTable(buffer, ref buffSize, false, IpVersion.IPv4,
												  TcpTableClass.TcpTableOwnerPidAll, 0);

				while (retVal == ErrorInsufficientBuffer) //buffer should be greater?
				{
					buffer = Marshal.ReAllocHGlobal(buffer, new IntPtr(buffSize));
					retVal = GetExtendedTcpTable(buffer, ref buffSize, false, IpVersion.IPv4,
												 TcpTableClass.TcpTableOwnerPidAll, 0);
				}

				if (retVal != Successfully) return false;

				ReadData(buffer, out tTable);
			}
			catch (Exception)
			{
				return false;
			}
			finally
			{
				// Free the Memory
				Marshal.FreeHGlobal(buffer);
			}

			for (int i = 0; i < tTable.Length; i++)
			{
				if (tTable[i].OwningPid == pid)
				{
					uint _port = tTable[i].LocalPort;
					var b = new byte[2];
					// high weight byte
					b[0] = (byte)(_port >> 8);
					// low weight byte
					b[1] = (byte)(_port & 255);

					if (BitConverter.ToUInt16(b, 0) == port)
					{
						return true;
					}
				}
			}

			return false;
		}

		public static bool StartProcess(string command, ProcessExecutor executor, string javaPath = "")
		{
			string fileName = "";

			if (executor == ProcessExecutor.Cmd)
			{
				fileName = "cmd.exe";
				command = "/C " + command;
			}
			else if (executor == ProcessExecutor.Java)
			{
				fileName = javaPath;
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
				process.StartInfo.WorkingDirectory = GlobalData.GeneralSettings.GamePath;
				process.StartInfo.UseShellExecute = false;
#if DEBUG
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
                {
                    Runtime.DebugWrite("Process error: " + e.Data);
                };
                process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
                {
                    Runtime.DebugWrite("Process output: " + e.Data);
                };
#endif
				process.Start();
#if DEBUG
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
#endif
				return process.WaitForExit(300000); // ждём 5 минут
			}
			catch (Exception ex)
			{
				Runtime.DebugWrite(ex);
				return false;
			}
		}
	}
}
