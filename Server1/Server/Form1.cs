using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace Server
{
    public partial class Form1 : Form
    {

        static string path_masterPublic = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "MasterServer_pub.txt");
        
        static string path_server1_prv = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Server1_pub_prv.txt");
        static string path_server1_pub = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Server1_pub.txt");

        static string path_server2_prv = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Server2_pub_prv.txt");
        static string path_server2_pub = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Server2_pub.txt");
        static string queue1 = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "queue1.txt");
        static string queue2 = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "queue2.txt");
        //static string path_private = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "MasterServer_pub_prv.txt");
        //string[] publicKey1 = File.ReadAllLines(path_public1);

        string[] privateKey;
        string[] publicKey;
        static string serverID = "";
        int serverPort;
        

        string[] masterPublic = File.ReadAllLines(path_masterPublic);
        static string path_uploads;

        Byte[] server_session_AES_key = new byte[16];
        Byte[] server_session_AES_IV = new byte[16];
        Byte[] server_session_HMAC = new byte[16];

        Byte[] master_session_AES_key = new byte[16];
        Byte[] master_session_AES_IV = new byte[16];
        Byte[] master_session_HMAC = new byte[16];


        bool terminating = false;
        bool listening = false;
        bool connected_to_otherServer = false;
        bool connected_to_masterServer = false;

        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket remoteSocket;
        Socket otherServerSocket;
        //Socket otherServerSocket;
        List<Socket> socketList = new List<Socket>();

        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }

        private void listenButton_Click(object sender, EventArgs e)
        {
            
            Thread acceptThread;


            if (Int32.TryParse(clientPort.Text, out serverPort))
            {
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, serverPort));
                serverSocket.Listen(3);

                listening = true;
                listenButton.Enabled = false;

                if(serverPort == 11)
                {
                    privateKey = File.ReadAllLines(path_server1_prv);
                    publicKey = File.ReadAllLines(path_server1_pub);
                    path_uploads = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).FullName + "\\server1_downloads\\";
                    serverID = "1";
                    logs.AppendText("This is Server 1!\n");
                }
                else
                {
                    privateKey = File.ReadAllLines(path_server2_prv);
                    publicKey = File.ReadAllLines(path_server2_pub);
                    path_uploads = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).FullName + "\\server2_downloads\\";
                    serverID = "2";
                    logs.AppendText("This is Server 2!\n");
                }

                if (!Directory.Exists(path_uploads))
                    System.IO.Directory.CreateDirectory(path_uploads);

                Thread checkThread = new Thread(new ThreadStart(ConstantQueueCheck));
                checkThread.Start();

                acceptThread = new Thread(new ThreadStart(Accept));
                acceptThread.Start();

                logs.AppendText("Started listening on port: " + serverPort + "\n");
                logs.AppendText("Public key is:\n" + string.Join("", publicKey) + "\n");
            }
            else
            {
                logs.AppendText("Please check port number \n");
            }
        }

        private void Accept()
        {
            while (listening)
            {
                try
                {
                    Socket newClient = serverSocket.Accept();
                    socketList.Add(newClient); //accept clients
                    logs.AppendText("A client is connected \n");

                    Thread receiveThread = new Thread(() => Receive(newClient)); // updated
                    receiveThread.Start();
                }
                catch
                {
                    if (terminating)
                    {
                        listening = false;
                    }
                    else
                    {
                        logs.AppendText("The socket stopped working \n");
                    }
                }
            }
        }

        private void Receive(Socket thisSocket)
        {
            Console.WriteLine("inside receive");
            bool connected = true;
            while (!terminating && connected)
            {
                try
                {
                    Byte[] buffer = new byte[384]; // buffer for RSA encrypted AESKEY,IV
                    thisSocket.Receive(buffer);

                    string message = Encoding.Default.GetString(buffer);

                    

                    if (message != null && message.Contains("\0"))
                        message = message.Substring(0, message.IndexOf("\0"));
                    else
                        logs.AppendText("receive message is null\n");

                    Console.WriteLine(message);

                    if (message.Contains("file broadcast"))
                    {
                        string broadcasting_serverID = message.Split(':')[1];
                        
                        byte[] current_key = new byte[16];
                        byte[] current_IV = new byte[16];
                        byte[] current_hmac = new byte[16];

                        if (broadcasting_serverID == "1" || broadcasting_serverID == "2")
                        {
                            current_key = server_session_AES_key;
                            current_IV = server_session_AES_IV;
                            current_hmac = server_session_HMAC;
                            if (serverPort == 11)
                                logs.AppendText("Server 2 started to share a file.\n");
                            else if (serverPort == 12)
                                logs.AppendText("Server 1 started to share a file.\n");
                        }
                        else if (broadcasting_serverID == "3")
                        {
                            current_key = master_session_AES_key;
                            current_IV = master_session_AES_IV;
                            current_hmac = master_session_HMAC;
                            logs.AppendText("The Master Server started to share a file.\n");
                        }
                        else
                            logs.AppendText("A client is trying to broadcast a file, which is not allowed.\n");

                        byte[] encrypted_fileName_Hmac = new byte[32 + 512];
                        thisSocket.Receive(encrypted_fileName_Hmac);

                        byte[] file = new byte[512];
                        byte[] hmac = new byte[32];

                        Array.Copy(encrypted_fileName_Hmac, 0, hmac, 0, 32);
                        Array.Copy(encrypted_fileName_Hmac, 32, file, 0, 512);

                        file = Decode(file);

                        Console.WriteLine(Encoding.Default.GetString(file));

                        byte[] encrypted_file = decryptWithAES128(Encoding.Default.GetString(file), current_key, current_IV);
                        string decyrpted_string = Encoding.Default.GetString(encrypted_file);
                        byte[] hmacToBeVerified = applyHMACwithSHA256(Encoding.Default.GetString(encrypted_file), current_hmac);
                        if (ByteArrayCompare(hmac, hmacToBeVerified))
                        {
                            try
                            {
                                string message2 = String.Empty;
                                do
                                {
                                    Byte[] buffer2 = new Byte[48000000 + 48];
                                    thisSocket.Receive(buffer2);

                                    message2 = Encoding.Default.GetString(buffer2);
                                    /*if (message2 != null && message2.Contains("\0"))
                                        message2 = message2.Substring(0, message2.IndexOf("\0"));*/
                                    if (message2.Contains("+END+"))
                                    {
                                        logs.AppendText("File broadcasting is finished and " + decyrpted_string + " is downloaded.\n");
                                        ChangeFileName(path_uploads + "incomingFileBroadcast", path_uploads + decyrpted_string);
                                        break;
                                    }

                                    byte[] file_packet = new byte[48000016];
                                    byte[] file_hmac = new byte[32];

                                    Array.Copy(buffer2, 0, file_packet, 0, 48000016);
                                    Array.Copy(buffer2, 48000016, file_hmac, 0, 32);

                                    byte[] decrypted_file = decryptWithAES128(Encoding.Default.GetString(file_packet), current_key, current_IV);
                                    byte[] hmacToBeVerifiedFileVersion = applyHMACwithSHA256(Encoding.Default.GetString(decrypted_file), current_hmac);
                                    if (ByteArrayCompare(file_hmac, hmacToBeVerifiedFileVersion))
                                    {
                                        logs.AppendText("File packet is verified. Writing bytes to a new file.\n");
                                        decrypted_file = Decode(decrypted_file);
                                        WriteToFileAsBytes(path_uploads + "\\" + "incomingFileBroadcast", decrypted_file);
                                    }
                                } while (message2 != "+END+");
                            }
                            catch
                            {
                                logs.AppendText("Something went wrong while broadcasting the file.\n");
                            }
                        }
                        else
                        {
                            logs.AppendText("FILE IS NOT VERIFIED\n");
                        }


                    }
                    else if (message.Contains("Download Request:"))
                    {
                        string file_name = message.Split(':')[1];
                        if(File.Exists(path_uploads + "\\" + file_name))
                        {
                            logs.AppendText("requested file exists\n");
                            byte[] request_sign = signWithRSA("+FILE", 3072, string.Join("", privateKey));
                            thisSocket.Send(Combine(Encoding.Default.GetBytes("+FILE"), request_sign));
                            SentFileToClient(file_name, thisSocket);
                        }
                        else
                        {
                            logs.AppendText("requested file does not exist\n");
                            byte[] request_sign = signWithRSA("-FILE", 3072, string.Join("", privateKey));
                            thisSocket.Send(Combine(Encoding.Default.GetBytes("-FILE"), request_sign));
                        }

                    }
                    else if(message.Contains("server-to-server-session-key"))
                    {
                        Byte[] received = new Byte[768]; // receive the keys
                        thisSocket.Receive(received);

                        Byte[] encrypted = new Byte[384]; //  parse the RSA encrypted aes key and iv and decrypt
                        Buffer.BlockCopy(received, 0, encrypted, 0, 384);
                        string decrypted = Encoding.Default.GetString(decryptWithRSA(Encoding.Default.GetString(encrypted), 3072, string.Join("", privateKey)));

                        Byte[] signed = new Byte[384]; // parse the sign
                        Buffer.BlockCopy(received, 384, signed, 0, 384);

                        bool verification = verifyWithRSA(decrypted, 3072, string.Join("", masterPublic), signed);
                        if (verification)
                        {
                            logs.AppendText("Signature verification of other server is successful \n");

                            byte[] decrypted_bytes = Encoding.Default.GetBytes(decrypted);

                            Array.Copy(decrypted_bytes, 0, server_session_AES_key, 0, 16);
                            Array.Copy(decrypted_bytes, 16, server_session_AES_IV, 0, 16);
                            Array.Copy(decrypted_bytes, 32, server_session_HMAC, 0, 16);
                        }
                        else
                        {
                            logs.AppendText("Signature verification of other server is failed \n");
                        }
                    }
                    else
                    {
                        Byte[] decryptionAESKEY = decryptWithRSA(Encoding.Default.GetString(buffer), 3072, string.Join("", privateKey));

                        Byte[] aeskey = new byte[16];
                        Byte[] aesiv = new byte[16];

                        Array.Copy(decryptionAESKEY, 0, aeskey, 0, 16);
                        Array.Copy(decryptionAESKEY, 16, aesiv, 0, 16);
                        logs.AppendText("Aes key iv received and decrypted\n");

                        try
                        {
                            bool condition = true;
                            bool successful = false;
                            string fileName = "";

                            while (condition) // receive until error or finish the file
                            {
                                Byte[] fileBuffer = new byte[48000016]; // 1 package with aes decrypted
                                thisSocket.Receive(fileBuffer);

                                string stringPacket = Encoding.Default.GetString(fileBuffer);

                                if (stringPacket.Contains("FILE NAME:")) // if file name is contained, upload is finished
                                {
                                    condition = false;
                                    string[] messageList = stringPacket.Split('\\');
                                    fileName = messageList[messageList.Length - 1];
                                    try
                                    {
                                        if (fileName != null && fileName.Contains("\0"))
                                        {
                                            fileName = fileName.Substring(0, fileName.IndexOf("\0")); // take only filename not path, example.txt
                                        }

                                    }
                                    catch (Exception)
                                    {
                                        logs.AppendText("filename parsing is failed.\n");
                                    }

                                    logs.AppendText("Transfer finished. \n");
                                    logs.AppendText("file name is " + fileName + "\n");
                                    condition = false; // stop the loop with success
                                    successful = true;
                                }
                                else if (stringPacket.Contains("NOT VERIFIED")) // if sign failed
                                {

                                    condition = false; //stop the loop with error
                                    successful = false;
                                }
                                else // sign and send
                                {
                                    thisSocket.Send(signWithRSA(Encoding.Default.GetString(fileBuffer), 3072, string.Join("", privateKey)));
                                    //decrypt
                                    byte[] decryptedPacket = decryptWithAES128(Encoding.Default.GetString(fileBuffer), aeskey, aesiv);
                                    //write the package to the file

                                    decryptedPacket = Decode(decryptedPacket);

                                    WriteToFileAsBytes(path_uploads + "incomingFile", decryptedPacket);

                                }
                            }
                            if (successful)
                            {
                                logs.AppendText("File is downloaded successfully\n");

                                ChangeFileName(path_uploads + "incomingFile", path_uploads + fileName);

                                AddQueue(fileName);


                                if (connected_to_masterServer && connected_to_otherServer)
                                {
                                    logs.AppendText("Starting to share the file:" + fileName + ", with other servers.\n");

                                    //FileBroadCast(fileName);
                                }
                                else if (connected_to_masterServer && !connected_to_otherServer)
                                {
                                    if(serverPort == 11)
                                        logs.AppendText("Cannot share the file because server 2 is disconnected.\n");
                                    else
                                        logs.AppendText("Cannot share the file because server 1 is disconnected.\n");
                                }
                                else if (!connected_to_masterServer && connected_to_otherServer)
                                    logs.AppendText("Cannot share the file because master server is disconnected.\n");
                                else
                                    logs.AppendText("Cannot share the file because servers are disconnected.\n");



                            }
                            else
                            {
                                logs.AppendText("File could not downloaded.\n");
                            }

                        }
                        catch (Exception)
                        {
                            logs.AppendText("File could not downloaded.\n");

                            thisSocket.Send(signWithRSA("File cannot be received", 3072, string.Join("", privateKey)));
                        }

                    }    // client upload
                }
                catch
                {
                    if (!terminating)
                    {
                        if (thisSocket == otherServerSocket)
                        {
                            if(serverPort == 11)
                                logs.AppendText("Server 1 has disconnected.\n");
                            else if(serverPort == 12)
                                logs.AppendText("Server 2 has disconnected.\n");
                        }
                        else if(thisSocket == remoteSocket)
                            logs.AppendText("The Master Server has disconnected.\n");
                        else
                            logs.AppendText("A client has disconnected.\n");
                    }

                    thisSocket.Close();
                    socketList.Remove(thisSocket);
                    connected = false;
                }
            }

        }

        private void SentFileToClient(string file_name, Socket Client)
        {
            try
            {
                byte[] fileBytes = File.ReadAllBytes(path_uploads + "\\" + file_name); //read all the file as bytes
                //encrypt the file line by line with AES128 and send it to the server
                try
                {
                    int start = 0;
                    int length = 48000000;
                                 
                    while (start < fileBytes.Length) // iterate over the file buffer and send them by packages if everything is ok
                    {
                        byte[] temp = new byte[48000000];
                        if (!(start + 48000000 < fileBytes.Length))
                        {
                            Array.Copy(fileBytes, start, temp, 0, (fileBytes.Length - start - 1));
                        }
                        else
                            Array.Copy(fileBytes, start, temp, 0, length);

                        start += 48000000;
                        byte[] packet_signature = signWithRSA(Encoding.Default.GetString(temp), 3072, string.Join("", privateKey));
                        Client.Send(Combine(temp, packet_signature));

                    }

                    Thread.Sleep(100); // to prevent sync problems

                    //finally, send the file name to the server. Sending the file name indicates the end of file transfer
                    string end_of_transfer = "+END+";

                    logs.AppendText("File Uploaded successfully\n");

                    Client.Send(Encoding.Default.GetBytes(end_of_transfer));
                }
                catch
                {
                    logs.AppendText("Something went wrong while sending the file.\n");
                }
            }
            catch
            {
                logs.AppendText("Something went wrong while reading the file.\n");
            }
        }

        private async void ConstantQueueCheck()
        {
            while (true)
            {
                string queue;
                if (serverID == "1") queue = queue1;
                else queue = queue2;
                string[] filenames = File.ReadAllLines(queue);
                Thread.Sleep(1000);
                if (connected_to_otherServer && connected_to_masterServer && filenames.Length>0)
                {
                    await Task.Run( ()=> FileBroadCast(filenames[0]));
                    Thread.Sleep(1000);
                }

            }
        }

        private void AddQueue(string filename)
        {
            string queue;
            if (serverID == "1") queue = queue1;
            else queue = queue2;
            using (StreamWriter sw = File.AppendText(queue))
            {
                sw.WriteLine(filename);
            }
        }

        private void DeleteFromQueue(string filename)
        {
            var tempFile = Path.GetTempFileName();
            string queue;
            if (serverID == "1") queue = queue1;
            else queue = queue2;

            var linesToKeep = File.ReadLines(queue).Where(l => l !=  filename);

            File.WriteAllLines(tempFile, linesToKeep);

            File.Delete(queue);
            File.Move(tempFile, queue);
        }

        private void FileBroadCast(string file_name)
        {           
            byte[] message = Encoding.Default.GetBytes("file broadcast:"+ serverID);
            remoteSocket.Send(message);
            otherServerSocket.Send(message);

            //encrypt and send file name to the other server together with HMAC
            byte[] fileName_otherServer_encrypt = encryptWithAES128(file_name, server_session_AES_key, server_session_AES_IV);
            byte[] fileName_otherServer_hmac = applyHMACwithSHA256(file_name, server_session_HMAC);
            otherServerSocket.Send(Combine(fileName_otherServer_hmac, fileName_otherServer_encrypt));

            Thread.Sleep(50);


            //encrypt and send file name to the master server together with HMAC
            byte[] fileName_masterServer_encrypt = encryptWithAES128(file_name, master_session_AES_key, master_session_AES_IV);
            byte[] fileName_masterServer_hmac = applyHMACwithSHA256(file_name, master_session_HMAC);
            remoteSocket.Send(Combine(fileName_masterServer_hmac, fileName_masterServer_encrypt));

            try
            {
                byte[] file_bytes = File.ReadAllBytes(path_uploads + "\\" + file_name);
                try
                {
                    int start = 0;
                    int length = 48000000;

                    while (start < file_bytes.Length) // iterate over the file buffer and send them by packages if everything is ok
                    {
                        byte[] temp = new byte[48000000];
                        if (!(start + 48000000 < file_bytes.Length))
                        {
                            Array.Copy(file_bytes, start, temp, 0, (file_bytes.Length - start - 1));
                        }
                        else
                            Array.Copy(file_bytes, start, temp, 0, length);

                        start += 48000000;
                        string temp_str = Encoding.Default.GetString(temp);

                        //encrypt temp for other server and send together with HMAC
                        byte[] packet_hmac_otherServer = applyHMACwithSHA256(temp_str, server_session_HMAC);
                        byte[] packet_encrypted_otherServer = encryptWithAES128(temp_str, server_session_AES_key, server_session_AES_IV);
                        byte[] sent = Combine(packet_encrypted_otherServer, packet_hmac_otherServer);
                        otherServerSocket.Send(sent);
                        
                        //encrpted temp for master server and send together with HMAC
                        byte[] packet_hmac_masterServer = applyHMACwithSHA256(temp_str, master_session_HMAC);
                        byte[] packet_encrypted_masterServer = encryptWithAES128(temp_str, master_session_AES_key, master_session_AES_IV);
                        byte[] sent2 = Combine(packet_encrypted_masterServer, packet_hmac_masterServer);
                        remoteSocket.Send(sent2);
                    }//end while

                    //finally, send ending message to other servers to indicate that file sharing has ended.
                    string end_of_transfer = "+END+";
                    logs.AppendText("File broadcasted successfully\n");
                    remoteSocket.Send(Encoding.Default.GetBytes(end_of_transfer));
                    otherServerSocket.Send(Encoding.Default.GetBytes(end_of_transfer));
                    DeleteFromQueue(file_name);
                }
                catch
                {
                    logs.AppendText("Something went wrong while sending the file.\n");
                }
            }
            catch
            {
                logs.AppendText("Something went wrong while reading the file.\n");
            }


        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            remoteSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string IP = ipAdress.Text;
            int port = 14;
            
            try
            {
                logs.AppendText("trying to connect to the master server\n");
                remoteSocket.Connect(IP, port);
                

                connectButton.Enabled = false;
                //portNum.Clear();
                logs.AppendText("Connected to master server\n");
                connected_to_masterServer = true;

                string requestMessage = "server-key-request " + serverID; //server request message 

                Byte[] buffer = new Byte[64]; // send the request
                buffer = Encoding.Default.GetBytes(requestMessage);
                remoteSocket.Send(buffer);
                    
                Byte[] received = new Byte[768]; // receive the keys
                remoteSocket.Receive(received);

                Byte[] encrypted = new Byte[384]; //  parse the RSA encrypted aes key and iv and decrypt
                Buffer.BlockCopy(received, 0,encrypted, 0, 384);
                string decrypted = Encoding.Default.GetString(decryptWithRSA(Encoding.Default.GetString(encrypted), 3072, string.Join("", privateKey)));

                Byte[] signed = new Byte[384]; // parse the sign
                Buffer.BlockCopy(received, 384,signed, 0, 384);

                bool verification = verifyWithRSA(decrypted, 3072, string.Join("", masterPublic), signed); 
                    

                if (verification)
                {
                    logs.AppendText("Signature verification of master server is successful \n");

                    byte[] decrypted_bytes = Encoding.Default.GetBytes(decrypted);

                    Array.Copy(decrypted_bytes, 0, master_session_AES_key, 0, 16);
                    Array.Copy(decrypted_bytes, 16, master_session_AES_IV, 0, 16);
                    Array.Copy(decrypted_bytes, 32, master_session_HMAC, 0, 16);

                    Thread masterReceiveThread = new Thread(() => Receive(remoteSocket)); // master'dan receive almak için
                    masterReceiveThread.Start();
                }
                else
                {
                    logs.AppendText("Signature verification of master server is failed \n");
                }

            }
            catch
            {
                logs.AppendText("Could not connect to remote server\n");
            }
            
        }

        private void button_connectServer_Click(object sender, EventArgs e)
        {
            otherServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string IP = String.Empty;
            int otherServerPortNum = 0;

            if (serverPort == 11)
                otherServerPortNum = 12;
            else
                otherServerPortNum = 11;

            try
            {
                logs.AppendText("trying to connect to the other server.\n");
                otherServerSocket.Connect(IP, otherServerPortNum);

                button_connectServer.Enabled = false;
                logs.AppendText("Connected to the other server.\n");
                connected_to_otherServer = true;

                string message = "server-to-server-connected";
                byte[] buffer = Encoding.Default.GetBytes(message);

                Thread otherServerReceiveThread = new Thread(() => Receive(otherServerSocket)); // master'dan receive almak için
                otherServerReceiveThread.Start();

                remoteSocket.Send(buffer);
            }
            catch
            {
                logs.AppendText("Could not connect to the other server\n");
            }

        }

        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listening = false;
            terminating = true;
            Environment.Exit(0);
        }

        static byte[] decryptWithRSA(string input, int algoLength, string xmlStringKey)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlStringKey);
            byte[] result = null;

            try
            {
                result = rsaObject.Decrypt(byteInput, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }


        static bool verifyWithRSA(string input, int algoLength, string xmlString, byte[] signature)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlString);
            bool result = false;
          
            try
            {
                result = rsaObject.VerifyData(byteInput, "SHA256", signature);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

            }
            

            return result;
        }

        static byte[] signWithRSA(string input, int algoLength, string xmlString)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlString);
            byte[] result = null;

            try
            {
                result = rsaObject.SignData(byteInput, "SHA256");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        static byte[] decryptWithAES128(string input, byte[] key, byte[] IV)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);

            // create AES object from System.Security.Cryptography
            RijndaelManaged aesObject = new RijndaelManaged();
            // since we want to use AES-128
            aesObject.KeySize = 128;
            // block size of AES is 128 bits
            aesObject.BlockSize = 128;

            // mode -> CipherMode.*
            aesObject.Mode = CipherMode.CFB;
            // feedback size should be equal to block size
            // aesObject.FeedbackSize = 128;
            // set the key
            aesObject.Key = key;
            // set the IV
            aesObject.IV = IV;
            // create an encryptor with the settings provided
            ICryptoTransform decryptor = aesObject.CreateDecryptor();
            byte[] result = null;

            try
            {

                {
                    result = decryptor.TransformFinalBlock(byteInput, 0, byteInput.Length);
                }
            }
            catch (Exception e) // if encryption fails
            {
                Console.WriteLine(e.Message); // display the cause
            }

            return result;
        }

        private void ChangeFileName(string decryptedFileNameStr, string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            System.IO.File.Move(decryptedFileNameStr, filename);
        }

        private void WriteToFileAsBytes(string decryptedFileNameStr, byte[] decryptedFileContentTemp)
        {
            using (FileStream data = new FileStream(decryptedFileNameStr, FileMode.Append))
            {
                data.Write(decryptedFileContentTemp, 0, decryptedFileContentTemp.Length);
            }
        }

        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }


        // encryption with AES-128
        static byte[] encryptWithAES128(string input, byte[] key, byte[] IV)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);

            // create AES object from System.Security.Cryptography
            RijndaelManaged aesObject = new RijndaelManaged();
            // since we want to use AES-128
            aesObject.KeySize = 128;
            // block size of AES is 128 bits
            aesObject.BlockSize = 128;
            // mode -> CipherMode.*
            aesObject.Mode = CipherMode.CFB;
            // feedback size should be equal to block size
            aesObject.FeedbackSize = 128;
            // set the key
            aesObject.Key = key;
            // set the IV
            aesObject.IV = IV;
            // create an encryptor with the settings provided
            ICryptoTransform encryptor = aesObject.CreateEncryptor();
            byte[] result = null;

            try
            {
                result = encryptor.TransformFinalBlock(byteInput, 0, byteInput.Length);
            }
            catch (Exception e) // if encryption fails
            {
                Console.WriteLine(e.Message); // display the cause
            }

            return result;
        }

        // HMAC with SHA-256
        static byte[] applyHMACwithSHA256(string input, byte[] key)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create HMAC applier object from System.Security.Cryptography
            HMACSHA256 hmacSHA256 = new HMACSHA256(key);
            // get the result of HMAC operation
            byte[] result = hmacSHA256.ComputeHash(byteInput);

            return result;
        }

        // hash function: SHA-256
        static byte[] hashWithSHA256(string input)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create a hasher object from System.Security.Cryptography
            SHA256CryptoServiceProvider sha256Hasher = new SHA256CryptoServiceProvider();
            // hash and save the resulting byte array
            byte[] result = sha256Hasher.ComputeHash(byteInput);

            return result;
        }

        public byte[] Decode(byte[] packet)
        {
            var i = packet.Length - 1;
            while (packet[i] == 0)
            {
                --i;
            }
            var temp = new byte[i + 1];
            Array.Copy(packet, temp, i + 1);
            return temp;
        }

        static bool ByteArrayCompare(byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; i++)
                if (a1[i] != a2[i])
                    return false;

            return true;
        }
    }
}
