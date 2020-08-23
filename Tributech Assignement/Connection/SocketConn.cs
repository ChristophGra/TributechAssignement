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
		private Socket _ConnectedSocket;
	
		private BackgroundWorker receiveWorker;
		private BackgroundWorker sendWorker;
		private Queue<byte[]> receivedQueue;
		private Queue<byte[]> sendQueue;
		private SocketConn() 
		{
			sendQueue = new Queue<byte[]>();
			receivedQueue = new Queue<byte[]>();
			receiveWorker = new BackgroundWorker();
			receiveWorker.DoWork += ReceiveWorker_DoWork;
		}
		public IEnumerable<byte[]> GetMessage()
		{
			byte[] buf = null;
			while (true)
			{
				lock (receivedQueue)
				{
					if (receivedQueue.Count > 0)
						buf = receivedQueue.Dequeue();
				}
				if (buf != null)
					yield return buf;
				else
					Thread.Sleep(10);
			}
		}
		public void QueueMessage(byte[] message)
		{
			lock(sendQueue)
			{
				sendQueue.Enqueue(message);
			}
		}
		private void ReceiveWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			byte[] buffer = new byte[1024];
			while(true)
			{
				if (_ConnectedSocket.Receive(buffer) > 0)
				{
					lock (receivedQueue)
					{
						receivedQueue.Enqueue(buffer);
					}
				}
				else
					Thread.Sleep(10);
			}
		}

		public static IEnumerable<SocketConn> GetSocket(bool listener, int port, string hostAdress = "localhost")
		{
			if (listener)
				return GetListeningSocket(port, hostAdress);
			else
				return GetInitiatingSocket(port, hostAdress);
		}
		public static IEnumerable<SocketConn> GetListeningSocket(int port, string hostAdress)
		{
			Socket tempsocket = new Socket(SocketType.Stream, ProtocolType.Tcp);

			tempsocket.Bind(CreateEndPoint(port, hostAdress));
			tempsocket.Listen(10);
			while (true)
			{
				SocketConn connection = new SocketConn();
				connection._ConnectedSocket = tempsocket.Accept();
				yield return connection;
			}
		}
		private static IPEndPoint CreateEndPoint(int port, string hostAddress  )
		{
			IPHostEntry host = Dns.GetHostEntry(hostAddress);
			IPAddress ipAddress = host.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddress, port);
			return endPoint;
		}
		public static IEnumerable<SocketConn> GetInitiatingSocket(int port, string hostAdress)
		{
			var remoteEP = CreateEndPoint(port, hostAdress);
			while (true)
			{
				SocketConn connection = new SocketConn();
				connection._ConnectedSocket = new Socket(remoteEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				connection._ConnectedSocket.Connect(remoteEP);
				yield return connection;
			}
		}
	}
}
