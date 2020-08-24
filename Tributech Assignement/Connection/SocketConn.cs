using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Tributech_Assignement.Connection
{
	public class SocketConn
	{
		#region Members
		//Readonly to ensure that the references stay intact
		private readonly Socket _ConnectedSocket;
		private readonly BackgroundWorker receiveWorker;
		private readonly BackgroundWorker sendWorker;
		private readonly Queue<byte[]> receivedQueue;
		private readonly Queue<byte[]> sendQueue;
		#endregion
		#region Init
		private SocketConn(Socket socket)
		{
			_ConnectedSocket = socket;
			sendQueue = new Queue<byte[]>();
			receivedQueue = new Queue<byte[]>();
			receiveWorker = new BackgroundWorker();
			receiveWorker.DoWork += ReceiveWorker_DoWork;
			sendWorker = new BackgroundWorker();
			sendWorker.DoWork += SendWorker_DoWork;
		}
		public void Start()
		{
			receiveWorker.RunWorkerAsync();
			sendWorker.RunWorkerAsync();
		}
		public static SocketConn GetSocket(bool listener, int port, string hostAdress = "localhost")
		{
			if (listener)
				return GetListeningSocket(port, hostAdress);
			else
				return GetInitiatingSocket(port, hostAdress);
		}
		private static IPEndPoint CreateEndPoint(int port, string hostAddress)
		{
			IPHostEntry host = Dns.GetHostEntry(hostAddress);
			IPAddress ipAddress = host.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddress, port);
			return endPoint;
		}
		public static SocketConn GetListeningSocket(int port, string hostAdress)
		{
			Socket tempsocket = new Socket(SocketType.Stream, ProtocolType.Tcp);

			tempsocket.Bind(CreateEndPoint(port, hostAdress));
			tempsocket.Listen(10);
			SocketConn connection = new SocketConn(tempsocket.Accept());
			return connection;
		}
		public static SocketConn GetInitiatingSocket(int port, string hostAdress)
		{
			var remoteEP = CreateEndPoint(port, hostAdress);
			Socket tempSocket = new Socket(remoteEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			SocketConn connection = new SocketConn(tempSocket);
			try
			{
			connection._ConnectedSocket.Connect(remoteEP);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
			return connection;
		}
		#endregion
		#region Workers
		private void SendWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			byte[] messagebuffer = null;
			while (true)
			{
				lock (sendQueue)
				{
					if (sendQueue.Count > 0)
					{
						messagebuffer = sendQueue.Dequeue();
					}
				}
				if (messagebuffer != null)
				{
					_ConnectedSocket.Send(messagebuffer);
					messagebuffer = null;
				}
			}
		}
		private void ReceiveWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			byte[] buffer = new byte[1024];
			byte[] buf;
			int len;
			while ((len = _ConnectedSocket.Receive(buffer)) > 0)
			{
				buf = new byte[len];
				Array.Copy(buffer, buf, len);
				lock (receivedQueue)
				{
					receivedQueue.Enqueue(buf);
				}
				Thread.Sleep(10);
			}
		}
		#endregion
		#region MessageIO
		public byte[] GetMessage()
		{
			byte[] messageBuffer = null;
			lock (receivedQueue)
			{
				if (receivedQueue.Count > 0)
					messageBuffer = receivedQueue.Dequeue();
			}
			return messageBuffer;
		}
		public void QueueMessage(byte[] message)
		{
			lock (sendQueue)
			{
				sendQueue.Enqueue(message);
			}
		}
		#endregion
		#region ConnectionClose
		public void CloseConnection()
		{
			_ConnectedSocket.Close();
		}
		#endregion
	}
}
