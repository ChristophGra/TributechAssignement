using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using Tributech_Assignement.Connection;

namespace Tributech_Assignement
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length > 1)
			{
				if(int.TryParse(args[0], out int port) && bool.TryParse(args[1], out bool listening))
				{
					foreach (SocketConn sock in SocketConn.GetSocket(listening, port))
					{
						if (listening)
						{
							BackgroundWorker putDataInConsole = new BackgroundWorker();
							putDataInConsole.DoWork += PutDataInConsole_DoWork;
							putDataInConsole.RunWorkerAsync(new DoWorkEventArgs(sock));
							while (putDataInConsole.IsBusy)
							{
								Thread.Sleep(10);
							}
						}
						else
						{
							while (true)
							{
								sock.QueueMessage(Encoding.ASCII.GetBytes(Console.ReadLine()));
							}
						}
					}
				}
			}
		}

		private static void PutDataInConsole_DoWork(object sender, DoWorkEventArgs e)
		{
			SocketConn socket = e.Argument as SocketConn;
			if (socket != null)
			{
				foreach (byte[] buf in socket.GetMessage())
				{
					Console.Write(Encoding.ASCII.GetString(buf));
				}
			}
		}
	}
}