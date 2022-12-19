using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;
using System.Reflection;


namespace RemoteServer_project
{
    public partial class Form1 : Form
    {

        static string path_public1 = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Server1_pub.txt");
        static string path_public2 = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Server2_pub.txt");
        static string path_private = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "MasterServer_pub_prv.txt");
        static string path_fake = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "fake.txt");
        static string queue = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "queue.txt");
        static string path_uploads = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).FullName + "\\downloads\\";

        
       // string filePathToWrite = "C:\\Users\\PC\\Downloads\\";



        string[] publicKey1 = File.ReadAllLines(path_public1);
        string[] publicKey2 = File.ReadAllLines(path_public2);
        string[] privateKey = File.ReadAllLines(path_private);
        string[] fakeKey = File.ReadAllLines(path_fake);

        List<byte[]> byteArrays = new List<byte[]>();

        bool terminating = false;
        bool listening = false;

        bool server1_connected = false;
        bool server2_connected = false;
        int connected_servers = 0;

        Byte[] server1_session_AES_key = new byte[16];
        Byte[] server1_session_AES_IV = new byte[16];
        Byte[] server1_session_HMAC = new byte[16];

        Byte[] server2_session_AES_key = new byte[16];
        Byte[] server2_session_AES_IV = new byte[16];
        Byte[] server2_session_HMAC = new byte[16];

        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        Socket server1Socket;
        Socket server2Socket;

        List<Socket> socketList = new List<Socket>(); // list of client socket connected to server

        // RSA encryption with varying bit length

        private async void ConstantQueueCheck()
        {
            while (true)
            {
                string[] filenames = File.ReadAllLines(queue);
                Thread.Sleep(1000);
                if (server1_connected && server2_connected && connected_servers>1 && filenames.Length > 0)
                {
                    await Task.Run(() => FileBroadCast(filenames[0]));
                    Thread.Sleep(1000);
                }

            }
        }

        private void AddQueue(string filename)
        {
            using (StreamWriter sw = File.AppendText(queue))
            {
                sw.WriteLine(filename);
            }
        }

        private void DeleteFromQueue(string filename)
        {
            var tempFile = Path.GetTempFileName();

            var linesToKeep = File.ReadLines(queue).Where(l => l != filename);

            File.WriteAllLines(tempFile, linesToKeep);

            File.Delete(queue);
            File.Move(tempFile, queue);
        }
        static byte[] encryptWithRSA(string input, int algoLength, string xmlStringKey)
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
                //true flag is set to perform direct RSA encryption using OAEP padding
                result = rsaObject.Encrypt(byteInput, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        // signing with RSA
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

        //RSA Decryption
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

        static string generateHexStringFromByteArray(byte[] input)
        {
            string hexString = BitConverter.ToString(input);
            return hexString.Replace("-", "");
        }

        public static byte[] hexStringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
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

        //Combine 2 byte arrays
        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }

        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }

        private void listen_button_Click(object sender, EventArgs e)
        {
            string portnum = port.Text;
            int port_num;

            

            Console.WriteLine(path_uploads);
            
            if (!Directory.Exists(path_uploads))
                System.IO.Directory.CreateDirectory(path_uploads);

            //Path.Combine(path_uploads, "\\uploads");
            Console.WriteLine(path_uploads);
            if (Int32.TryParse(portnum, out port_num))
            {
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, port_num));
                serverSocket.Listen(5);

                listening = true;
                listen_button.Enabled = false;
                Thread acceptThread = new Thread(new ThreadStart(Accept));
                acceptThread.Start();

                Thread checkThread = new Thread(new ThreadStart(ConstantQueueCheck));
                checkThread.Start();

                logs.AppendText("This is The Master Server!\n" + "Private Key is:" + string.Join("", privateKey));
                logs.AppendText("\nStarted listening at port 14.\n");
            }
            else
            {
                logs.AppendText("Check the port number. \n");
            }
        }

        private void Accept()
        {
            while (listening)
            {
                try
                {
                    Socket newClient = serverSocket.Accept();
                    socketList.Add(newClient);
                    logs.AppendText("A client is connected. \n");

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
                        logs.AppendText("Socket has stopped working. \n");
                    }
                }
            }
        }
        private void Receive(Socket thisSocket)
        {
            Socket s = socketList[socketList.Count - 1]; //client that is newly added
            bool connected = true;
            

            while (!terminating && connected)
            {
                try
                {
                    Byte[] buffer = new byte[384];
                    s.Receive(buffer);

                    string message = Encoding.Default.GetString(buffer);

                    //s.Send(signWithRSA(message, 3072, string.Join("", privateKey)));


                    if (message != null && message.Contains("\0"))
                        message = message.Substring(0, message.IndexOf("\0"));
                    else
                        logs.AppendText("receive message is null\n");


                    if (message.Contains("server-key-request")) // if master no non-master
                    {
                        var random = RandomNumberGenerator.Create();
                        var bytes = new byte[48]; // 
                        random.GetNonZeroBytes(bytes);

                       if(message.Split(' ')[1] == "1") {
                            Byte[] encryption = encryptWithRSA(Encoding.Default.GetString(bytes), 3072, string.Join("", publicKey1));
                            Byte[] signed = signWithRSA(Encoding.Default.GetString(bytes), 3072, string.Join("", privateKey));
                            Byte[] combination = Combine(encryption, signed);
                            server1_connected = true;
                            server1Socket = s;

                            Array.Copy(bytes, 0, server1_session_AES_key, 0, 16);
                            Array.Copy(bytes, 16, server1_session_AES_IV, 0, 16);
                            Array.Copy(bytes, 32, server1_session_HMAC, 0, 16);

                            s.Send(combination);
                        }

                       else if(message.Split(' ')[1] == "2") {
                            Byte[] encryption = encryptWithRSA(Encoding.Default.GetString(bytes), 3072, string.Join("", publicKey2));
                            Byte[] signed = signWithRSA(Encoding.Default.GetString(bytes), 3072, string.Join("", privateKey));
                            Byte[] combination = Combine(encryption, signed);
                            server2_connected = true;
                            server2Socket = s;

                            Array.Copy(bytes, 0, server2_session_AES_key, 0, 16);
                            Array.Copy(bytes, 16, server2_session_AES_IV, 0, 16);
                            Array.Copy(bytes, 32, server2_session_HMAC, 0, 16);

                            s.Send(combination);
                        }                       
                       
                    }
                    else if (message.Contains("file broadcast"))
                    {
                        string broadcasting_serverID = message.Split(':')[1];
                        byte[] current_key = new byte[16];
                        byte[] current_IV = new byte[16];
                        byte[] current_hmac = new byte[16];

                        if (broadcasting_serverID == "1")
                        {
                            current_key = server1_session_AES_key;
                            current_IV = server1_session_AES_IV;
                            current_hmac = server1_session_HMAC;
                            logs.AppendText("Server 1 started to share a file.\n");
                        }
                        else if (broadcasting_serverID == "2")
                        {
                            current_key =server2_session_AES_key;
                            current_IV = server2_session_AES_IV;
                            current_hmac =server2_session_HMAC;
                            logs.AppendText("Server 2 started to share a file.\n");

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
                    else if(message.Contains("server-to-server-connected"))
                    {
                        connected_servers++;
                        try
                        {
                            if (connected_servers == 2)
                            {
                                logs.AppendText("server to server key dist here\n");
                                //server to server key dist here
                                var random = RandomNumberGenerator.Create();
                                var bytes = new byte[48]; // 
                                random.GetNonZeroBytes(bytes);

                                Byte[] encryption1 = encryptWithRSA(Encoding.Default.GetString(bytes), 3072, string.Join("", publicKey1));
                                Byte[] signed1 = signWithRSA(Encoding.Default.GetString(bytes), 3072, string.Join("", privateKey));
                                Byte[] combination1 = Combine(encryption1, signed1);

                                Byte[] encryption2 = encryptWithRSA(Encoding.Default.GetString(bytes), 3072, string.Join("", publicKey2));
                                Byte[] signed2 = signWithRSA(Encoding.Default.GetString(bytes), 3072, string.Join("", privateKey));
                                Byte[] combination2 = Combine(encryption2, signed2);

                                string message_s2s = "server-to-server-session-key";
                                byte[] message_buffer = Encoding.Default.GetBytes(message_s2s);

                                server1Socket.Send(message_buffer);
                                server2Socket.Send(message_buffer);

                                server1Socket.Send(combination1);
                                server2Socket.Send(combination2);
                            }
                        }
                        catch
                        {
                            logs.AppendText("something went wrong in server to server key dist.\n");
                        }
                    }

                    else
                    {

                        Byte[] decryptionAESKEY = decryptWithRSA(Encoding.Default.GetString(buffer), 3072, string.Join("", privateKey));

                        Byte[] aeskey = new byte[16];
                        Byte[] aesiv = new byte[16];

                        Array.Copy(decryptionAESKEY,0, aeskey,0, 16);
                        Array.Copy(decryptionAESKEY,16, aesiv,0, 16);
                        logs.AppendText("aes key iv received and decrypted\n");

                        try
                        {
                            bool condition = true;
                            bool successful = false;
                            string fileName = "";


                            while (condition) // receive packages until finish or error
                            {
                                Byte[] fileBuffer = new byte[48000016];
                                s.Receive(fileBuffer);

                                string stringPacket = Encoding.Default.GetString(fileBuffer);

                                if (stringPacket.Contains("FILE NAME:")) // last package and filename 
                                {
                                    condition = false;
                                    string[] messageList = stringPacket.Split('\\');
                                    fileName = messageList[messageList.Length - 1];
                                    try
                                    {
                                        if (fileName != null && fileName.Contains("\0"))
                                        {
                                            fileName  = fileName.Substring(0, fileName.IndexOf("\0"));
                                        }

                                    }
                                    catch (Exception)
                                    {
                                        logs.AppendText("filename parsing is failed.\n");
                                    }

                                    logs.AppendText("Transfer is finished \n");
                                    logs.AppendText(fileName + "\n");
                                    //logs.AppendText(stringPacket.Split(':')[0]);
                                    condition = false;
                                    successful = true;

                                }
                                else if (stringPacket.Contains("NOT VERIFIED")) // if sign failed
                                {

                                    condition = false;
                                    successful = false;
                                }
                                else // sign and 
                                {
                                    Console.WriteLine("before sign of filename");
                                    s.Send(signWithRSA(Encoding.Default.GetString(fileBuffer), 3072, string.Join("", privateKey)));
                                    Console.WriteLine("after sign of filename");
                                    ////decrypt
                                    byte[] decryptedPacket = decryptWithAES128(Encoding.Default.GetString(fileBuffer), aeskey, aesiv);
                                    // write to the file 
                                    WriteToFileAsBytes(path_uploads + "incomingFile", decryptedPacket);

                                }
                            }
                            if (successful)
                            {
                                logs.AppendText("File is downloaded successfully\n");

                                AddQueue(fileName);

                                ChangeFileName(path_uploads + "incomingFile", path_uploads + fileName);

                                if (server1_connected && server2_connected)
                                {
                                    logs.AppendText("Starting to share the file:" + fileName + ", with other servers.\n");
                                    //FileBroadCast(fileName);
                                }
                                else if (server1_connected && !server2_connected)
                                {
                                    logs.AppendText("Cannot share the file because server 2 is disconnected.\n");
                                }
                                else if (!server1_connected && server2_connected)
                                    logs.AppendText("Cannot share the file because server 2 is disconnected.\n");
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
                            s.Send(signWithRSA("File cannot be received", 3072, string.Join("", privateKey)));
                        }

                    } // if client
                }
                catch
                {
                    if (!terminating)
                    {
                        if (s == server1Socket)
                        {
                            server1_connected = false;
                            connected_servers--;
                            logs.AppendText("Server 1 is disconnected. \n");
                        }
                        else if (s == server2Socket)
                        {
                            server2_connected = false;
                            connected_servers--;
                            logs.AppendText("Server 2 is disconnected. \n");
                        }
                        else
                            logs.AppendText("A client is disconnected. \n");
                    }                   
                    s.Close();
                    socketList.Remove(s);
                    connected = false;
                }
            }

        }

        private void FileBroadCast(string file_name)
        {
            byte[] message = Encoding.Default.GetBytes("file broadcast:" + "3");
            server1Socket.Send(message);
            server2Socket.Send(message);

            //encrypt and send file name to the other server together with HMAC
            byte[] fileName_server1_encrypt = encryptWithAES128(file_name, server1_session_AES_key, server1_session_AES_IV);
            byte[] fileName_server1_hmac = applyHMACwithSHA256(file_name, server1_session_HMAC);
            server1Socket.Send(Combine(fileName_server1_hmac, fileName_server1_encrypt));

            Thread.Sleep(50);


            //encrypt and send file name to the master server together with HMAC
            byte[] fileName_masterServer_encrypt = encryptWithAES128(file_name, server2_session_AES_key, server2_session_AES_IV);
            byte[] fileName_masterServer_hmac = applyHMACwithSHA256(file_name, server2_session_HMAC);
            server2Socket.Send(Combine(fileName_masterServer_hmac, fileName_masterServer_encrypt));

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
                        byte[] packet_hmac_otherServer = applyHMACwithSHA256(temp_str, server1_session_HMAC);
                        byte[] packet_encrypted_otherServer = encryptWithAES128(temp_str, server1_session_AES_key, server1_session_AES_IV);
                        byte[] sent = Combine(packet_encrypted_otherServer, packet_hmac_otherServer);
                        server1Socket.Send(sent);

                        //encrpted temp for master server and send together with HMAC
                        byte[] packet_hmac_masterServer = applyHMACwithSHA256(Encoding.Default.GetString(temp), server2_session_HMAC);
                        byte[] packet_encrypted_masterServer = encryptWithAES128(temp_str, server2_session_AES_key, server2_session_AES_IV);
                        byte[] sent2 = Combine(packet_encrypted_masterServer, packet_hmac_masterServer);
                        server2Socket.Send(sent2);
                    }//end while

                    //finally, send ending message to other servers to indicate that file sharing has ended.
                    string end_of_transfer = "+END+";
                    logs.AppendText("File Uploaded successfully\n");
                    server1Socket.Send(Encoding.Default.GetBytes(end_of_transfer));
                    server2Socket.Send(Encoding.Default.GetBytes(end_of_transfer));
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



        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listening = false;
            terminating = true;
            Environment.Exit(0);
        }
        public static void SaveByteArrayToFileWithFileStream(byte[] data, string filePath)
        {
            Console.WriteLine("file length :" + Buffer.ByteLength(data));
            File.WriteAllBytes(filePath, data);
        }

        private static byte[] CombineArray(List<byte[]> arrays)
        {
            byte[] ret = new byte[arrays.Sum(x => x.Length)];
            int offset = 0;
            foreach (byte[] data in arrays)
            {
                Buffer.BlockCopy(data, 0, ret, offset, data.Length);
                offset += data.Length;
            }
            return ret;
        }

        private void WriteToFileAsBytes(string decryptedFileNameStr, byte[] decryptedFileContentTemp)
        {
            using (FileStream data = new FileStream(decryptedFileNameStr, FileMode.Append))
            {
                data.Write(decryptedFileContentTemp, 0, decryptedFileContentTemp.Length);
            }
        }

        private void ChangeFileName(string decryptedFileNameStr, string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            System.IO.File.Move(decryptedFileNameStr, filename );
        }

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
    }

}
