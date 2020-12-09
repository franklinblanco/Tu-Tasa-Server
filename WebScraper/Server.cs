using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace WebScraper
{
    class Server
    {
        #region Singleton Pattern
        private static Server instance = new Server();
        private Server() { }

        public static Server Instance
        {
            get { return instance; }
        }
        #endregion

        #region Server Variables

        private const string serverip = "104.248.233.17";
        private const int serverport = 5001;
        private int connectedClients = 0;
        #endregion

        #region Connection Variables
        private TcpListener serverSocket = new TcpListener(IPAddress.Parse(serverip), serverport);
        #endregion


        public void InitializeServer()
        {
            StartServer();
        }

        private void StartServer()
        {
            //Cache stuff
            CacheManager.Instance.Initialize();

            //Server Connection stuff
            Thread ServerThread;
            OpenSocket();
            while (true)
            {
                Console.WriteLine("Awaiting for Incoming Connections...");
                TcpClient connectionSocket = serverSocket.AcceptTcpClient();
                ServerThread = new Thread(() => EstablishConnection(connectionSocket));
                ServerThread.Start();
            }
        }
        private void OpenSocket()
        {
            serverSocket.Start();
        }
        private void EstablishConnection(TcpClient clientSocket)
        {
            connectedClients += 1; //increment connected clients by 1
            Console.WriteLine("A client has connected. Connected clients: " + connectedClients);
            NetworkStream clientStream = clientSocket.GetStream(); //link networkstream to allow it to send and receive data.

            while (ProcessRequest(clientStream)){}
            connectedClients -= 1; //Decrement connected clients by 1
            clientSocket.Close();
            Console.WriteLine("A client has disconnected. Connected clients: " + connectedClients);
        }

        private bool ProcessRequest(NetworkStream clientStream)
        {
            //Okay, so. This is the request handling for now. It's a quick way, and contains SOOOOOO MUCH SECURITY ISSUES
            //But, for now. Leave it as is. The final product will come if enough people use it.

            string[] requestargs = ReceiveMessage(clientStream).Split(' '); //split the message into a string[] by the character specified.
            bool close = false; //don't close by default
            if (requestargs.Length <= 0) { return false; } //just close if there is less than 1 arg.

            switch (requestargs[0]) //Do the switch statement on the first element of the request
            {
                case "1000": //Close connection
                    close = true;
                    break;
                case "1001": //Read
                    close = false;
                    if (requestargs.Length <= 1) { SendMessage(clientStream, "2000");  break; } //send error message to client and just loop again.
                    switch (requestargs[1]) //another switch for the second argument.
                    {
                        case "mainpage":
                            string wholeMessage = "";
                            Dictionary<string, CurrencyRate> allRates = CacheManager.Instance.BankRates;
                            foreach (CurrencyRate rate in allRates.Values)
                            {
                                wholeMessage += rate.bankname + "-" + rate.currency + "-" + rate.buyrate + "-" + rate.sellrate + "/";
                            }
                            string wholemessagedone = wholeMessage.Remove(wholeMessage.Length - 1);
                            SendMessage(clientStream, wholemessagedone);
                            Console.WriteLine("WholeMessage sent: " + wholemessagedone);
                            break;
                        //List all the possible read requests
                        //all banks
                        
                    }
                    break;
            }
            return close;
            
        }

        #region Encoding Methods
        private string decodeMessage(byte[] message)
        {
            return Encoding.Default.GetString(message);
        }
        private byte[] encodeMessage(string message)
        {
            return Encoding.Default.GetBytes(message);
        }
        #endregion

        #region Message sending
        public bool SendMessage(NetworkStream clientStream, string message)
        {
            //This should be the correct way for sending messages to clients, as this involves a client socket approach and is simpler in general.
            try
            {
                clientStream.Write(encodeMessage(message));
                return true;
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return false;
        }
        #endregion

        #region Message receiveing
        public string ReceiveMessage(NetworkStream clientStream)
        {
            try
            {
                byte[] message = new byte[1024]; //Create an empty byte array that will hold the message sent by client.
                int messageLength = clientStream.Read(message, 0, message.Length); //Store the size of the byte[].
                Array.Resize(ref message, messageLength); //Resize byte[] to the size gotten earlier.
                return decodeMessage(message);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
                return "Stop";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return "Stop";
            }

        }
        #endregion


    }
}
