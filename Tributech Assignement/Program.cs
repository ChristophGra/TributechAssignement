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
				if (int.TryParse(args[0], out int port) && bool.TryParse(args[1], out bool listening))
				{
					SocketConn sock = SocketConn.GetSocket(listening, port);
					sock.Start();
					BackgroundWorker putDataInConsole = new BackgroundWorker();
					putDataInConsole.DoWork += PutDataInConsole_DoWork;
					putDataInConsole.RunWorkerAsync(sock);
					do
					{
						string input = Console.ReadLine();
						if (input == "Stop")
						{
							sock.CloseConnection();
							return;
						}
						if (input != null)
						{
							sock.QueueMessage(Encoding.ASCII.GetBytes(input));
							input = null;
						}
					} while (true);
				}
			}
		}

		private static void PutDataInConsole_DoWork(object sender, DoWorkEventArgs e)
		{
			SocketConn socket = (SocketConn)(e.Argument);
			if (socket != null)
			{
				while (true)
				{
					byte[] buf = socket.GetMessage();
					if (buf != null)
						Console.Write(Encoding.ASCII.GetString(buf));
					else
						Thread.Sleep(10);
				}
			}
		}
	}
}