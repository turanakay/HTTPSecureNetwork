using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Security.Cryptography;
//using System.Threading.Tasks;

namespace Client_project
{
    public partial class Form1 : Form
    {
        //globals
        bool terminating = false;
        bool connected = false;
        Socket clientSocket;
        const string ERROR = "ERROR";
        const string SUCCESS = "SUCCESS";
        
        string path_uploads;

        string connected_server_PK = String.Empty;

        string Master_PK = "<RSAKeyValue><Modulus>2hIMsj1pdyJfmpz8yN/EfbSl3pe9QxRx3nm2FTafwzmQnxxB9O35sLANTPIpAmRM8oi730SEYunA0+1T/" +
            "p9e9EB7iJGwKB7LmaApWk8BRQ5jJwge7tA8wkmciV52jHBzYZk+tHOvuHd+AsMXYB1a/Y/8mpfzjECjU7A0P6ucrVyMkZIxAB0ozHAyABoXi9G57O6tkXAUXqDz" +
            "6zgRcSIzZ1vkiYoqXKKKxKdXj4HskxzGDwx7w7UF9wDz0Ek2Qtz9CmrROcJhFD22792rqmEYF00WKHw9uuBXaRjEdgmgwcY8loVvFxLOtRMAC7RZiU1oYpTMS" +
            "sIj7fVh017x63aGzlY8uqDEPz6teeAw52CV1gw1/glzpKJ6DIQBg7MS16xm7pIAh4UciuZUggmhtOrIeD7sT4ZN+MtWwCvV+tf0lnhdAJyEo9rtaoBIicfOA1uge3Pr" +
            "/QAfXI/Gd8GFryBuKSMJXgknphMVcrp5x+nx7bvpVHAIz/NnzyxF1d3JdIcp</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        string Server1_PK = "<RSAKeyValue><Modulus>+ShLQhyrcdyqN9KQO5aS4uLZaptlswGz7tIlkJeEIulE/kYvs3o0LQyixudd8j8heCDX50UyH02SaYULyJR3n9mT" +
            "+JJ6lOKz/rSqjcfTp32Anv09WtEBtsY4nFHgxJTRvRHIhSO600sSzIt4ftRc5jheyBHD6TrNKzJzJoBoxs74Y28kzp2ER0rRqd9h4Sls/dLlHKEzHY0WuQgjEg1eSw8q" +
            "0OCE4YJxRknadsdGh6dW7CcZuNmJFYL8qFhmUS1Vzy75NEZsEgye0h2oUC/2ediOsHvRZzI2QdDeyweKNK/CRVpXYlDnVi6wFCweWoHrSF5eBESyg7wl/DFC5S4pU28i2g" +
            "Euqqy3a0D0XCjPuuy9d3ElZUo5NGzZNJUdsFQgjy3tNyX73S6wJ0dG1SZKKbveD+1+Cg+JN5L3RMWRg48KNu/mzLGjjn0xnAI6RS+VIxo9GHjtdtebzmqDzSOUCG7eDubRI" +
            "H0BwBbsy93+yYW423qfGd08lJyZg4tdtuQJ</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        string Server2_PK = "<RSAKeyValue><Modulus>s9Mh7p3Eqx6mBfgRRtgmRoK2rXhTvO6STPTlKhyP2V+AiXXUURBHIblMTv+gnjPxfs+xafKnQUDD7PR1TFetDEe0Pi" +
            "VDW4v1E6Hz20f+S2XKXnIucByyhqePEmLC3dLHFmr4OjmCFTGXM1rw9LwggVOUxCIz31ZbhVzSe5ZgO9rHsH9Lk9FW8VDaJS8AmePmvbMW75lGx+7M4YqK7jawayM1daXH" +
            "gkNQ2/sB6cgtulos6U1EZW4EcM5nDFQvj1HB5/aG5iGRACRDzErUp9J252xXaPFdkZQrX+mpNH2eNdyl2aUHgHAcJ8UTQHbGNELcQt6HlMW/IdgCjhegn+l1CUUoiYOX5zc" +
            "Hc9pvrwkQ8mF3dlv+Yo+L32sYxw6bjlmLWmCa2QGkLZ0KF+ZaShCb/Rp/jnpb30QtHx8pehC3tRMWDsH1mor7ZO2tvoPShAtke2RpUpbhIc/IKTcMg29ULlt/SeMNgB0S/" +
            "2Gn8YIsGAGuZNst9EGX9NxoN0XfYSF9</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        
        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }

        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            connected = false;
            terminating = true;
            Environment.Exit(0);
        }

        private void button_connect_Click(object sender, EventArgs e)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string ip = IP.Text;
            string portNum = port.Text;
            int port_num;
            if (Int32.TryParse(portNum, out port_num))
            {
                try
                {
                    clientSocket.Connect(ip, port_num);
                    connected = true;
                    terminating = false;

                    button_connect.Enabled = false;
                    button_disconnect.Enabled = true;
                    button_upload.Enabled = true;
                    button_download.Enabled = true;

                    if (portNum == "11")// adjust the key according to connected server
                    {
                        logs.AppendText("Connected to server 1.\n");
                        connected_server_PK = Server1_PK;
                    }
                    else if (portNum == "12")
                    {
                        logs.AppendText("Connected to server 2.\n");
                        connected_server_PK = Server2_PK;
                    }
                    else if (portNum == "14")
                    {
                        logs.AppendText("Connected to the master server.\n");
                        connected_server_PK = Master_PK;
                    }

                    path_uploads = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).FullName + "\\downloads\\";
                    if (!Directory.Exists(path_uploads))
                        System.IO.Directory.CreateDirectory(path_uploads);

                    
                    //logs.AppendText("Server's public key in HexString format: " + connected_server_PK + "\n");

                    //Thread receiveThread = new Thread(new ThreadStart(Receive));
                    //receiveThread.Start();
                }
                catch 
                {
                    logs.AppendText("Could not connect to the server. \n");
                }
            }
            else {
                logs.AppendText("Check the port number. \n");
            }
        }

 
        private void button_disconnect_Click(object sender, EventArgs e)
        {
            string disconnect_request = "disconnect_request";
            Byte[] disconnect_request_buffer = Encoding.Default.GetBytes(disconnect_request);
            //clientSocket.Send(disconnect_request_buffer);

            connected = false;
            terminating = true;
            port.Clear();
            button_connect.Enabled = true;
            button_upload.Enabled = false;
            button_disconnect.Enabled = false;
            button_download.Enabled = false;

            clientSocket.Close();

            logs.AppendText("You have disconnected from the server.\n");
        }


        private void button_upload_Click(object sender, EventArgs e)
        {
            string fileName = String.Empty;
            OpenFileDialog openFileDialog1 = new OpenFileDialog(); //find the file
            if (openFileDialog1.ShowDialog() == DialogResult.OK) // Test result.
            {
                //Generate random 128-bit byte arrays for AES key and IV
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                byte[] AES128_key = new byte[16];
                rng.GetBytes(AES128_key);

                byte[] AES128_IV = new byte[16];
                rng.GetBytes(AES128_IV);

                //encrypt AES128 credentials with RSA3072 using the connected server's PK and send it to the server
                byte[] combinedAES = Combine(AES128_key, AES128_IV);
                string keyIV = Encoding.Default.GetString(combinedAES);
                byte[] encrypted_key_and_IV = encryptWithRSA(
                    keyIV,
                    3072,
                    connected_server_PK);

                clientSocket.Send(encrypted_key_and_IV); 

                Thread.Sleep(100);
                fileName = openFileDialog1.FileName;
                Upload(fileName, AES128_key, AES128_IV);
                
            }
            else
                logs.AppendText("Could not find the file. \n");
        }

        private void button_download_Click(object sender, EventArgs e)
        {
            string file_to_be_downloaded = textBox_download.Text;
            textBox_download.Clear();
            byte[] download_request = Encoding.Default.GetBytes("Download Request:" + file_to_be_downloaded);
            clientSocket.Send(download_request);

            byte[] request_reply = new byte[389];
            clientSocket.Receive(request_reply);
            
            byte[] signature = new byte[384];
            Array.Copy(request_reply, 5, signature, 0, 384);


            if (Encoding.Default.GetString(request_reply).Contains("+FILE"))
            {
                if(verifyWithRSA("+FILE", 3072, connected_server_PK, signature))
                {
                    logs.AppendText("File download is starting.\n");
                    Download(file_to_be_downloaded);
                }
                else
                {
                    logs.AppendText("Cannot verify.\n");
                }
                              
            }
            else if(Encoding.Default.GetString(request_reply).Contains("-FILE"))               
            {
                if(verifyWithRSA("-FILE", 3072, connected_server_PK, signature))
                    logs.AppendText("Cannot download file, does not exist.\n");
                else
                    logs.AppendText("Cannot verify.\n");               
            }
            else
            {
                logs.AppendText("Message is not meaningful\n");
            }

        }

        private void Upload(string file_to_be_uploaded, byte[] AES128_key, byte[] AES128_IV)
        {
            //generate IEnumerable string in order to iterate over the file
            try
            {
                button_upload.Enabled = false;
                button_download.Enabled = false;
                byte[] fileByte = File.ReadAllBytes(file_to_be_uploaded); //read all the file as bytes

                //encrypt the file line by line with AES128 and send it to the server
                try
                {
                    int start = 0;
                    int length = 48000000;
                    bool valid = true;
                    byte[] encrypted_file_line_byte_array;
                    while (start < fileByte.Length && valid) // iterate over the file buffer and send them by packages if everything is ok
                    {
                        byte[] temp = new byte[48000000];
                        if (!(start + 48000000 < fileByte.Length))
                        {
                            Array.Copy(fileByte, start, temp, 0, (fileByte.Length - start - 1));
                        }
                        else
                            Array.Copy(fileByte, start, temp, 0, length);

                        start += 48000000;


                        // encrypt parsed file and send
                        encrypted_file_line_byte_array = encryptWithAES128(Encoding.Default.GetString(temp), AES128_key, AES128_IV);
                        clientSocket.Send(encrypted_file_line_byte_array);
                        // receive signed version of sent package
                        byte[] receivedSign = new byte[384];
                        clientSocket.Receive(receivedSign);
                        //verify 
                        valid = verifyWithRSA(Encoding.Default.GetString(encrypted_file_line_byte_array), 3072, connected_server_PK, receivedSign);
                        //if valid continue otherwise stop
                        Console.WriteLine(valid);

                    }
                    if (!valid)
                    {
                        logs.AppendText("Sign is not valid \n");
                    }
                    else
                    {
                        logs.AppendText("Sign is valid \n");
                    }

                    Thread.Sleep(100); // to prevent sync problems

                    //finally, send the file name to the server. Sending the file name indicates the end of file transfer
                    string end_of_transfer = "FILE NAME:" + file_to_be_uploaded;

                    logs.AppendText("File Uploaded successfully\n");

                    clientSocket.Send(Encoding.Default.GetBytes(end_of_transfer));
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
 
            button_upload.Enabled = true;
            button_download.Enabled = true;
        }

        private void Download(string file_to_be_downloaded)
        {
            button_upload.Enabled = false;
            button_download.Enabled = false;

            if (connected)
            {
                string message = String.Empty;
                do
                {
                    Byte[] buffer = new Byte[48000000 + 384];
                    clientSocket.Receive(buffer);

                    message = Encoding.Default.GetString(buffer);
                    //if (message != null && message.Contains("\0"))
                    //    message = message.Substring(0, message.IndexOf("\0"));

                    if (message.Contains("+END+"))
                    {
                        logs.AppendText("File broadcasting is finished and " + file_to_be_downloaded + " is downloaded.\n");
                        ChangeFileName(path_uploads + "incomingFileDownload", path_uploads + file_to_be_downloaded);
                        break;
                    }

                    byte[] file_packet = new byte[48000000];
                    byte[] file_signature = new byte[384];

                    Array.Copy(buffer, 0, file_packet, 0, 48000000);
                    Array.Copy(buffer, 48000000, file_signature, 0, 384);

                    if(verifyWithRSA(Encoding.Default.GetString(file_packet), 3072, connected_server_PK, file_signature))
                    {
                        logs.AppendText("File packet is verified. Writing bytes to a new file.\n");

                        /*string final_str = Encoding.Default.GetString(file_packet);
                          
                        final_str = final_str.Substring(0, final_str.IndexOf('\0'));
                        byte[] packet = Encoding.Default.GetBytes(final_str);*/
                        file_packet = Decode(file_packet);

                        WriteToFileAsBytes(path_uploads + "\\" + "incomingFileDownload", file_packet);
                    }
                } while (!message.Contains("+END+"));

                

                /*clientSocket.Close();
                connected = false;
                button_connect.Enabled = true;
                button_upload.Enabled = false;
                button_disconnect.Enabled = false;*/
                
            }

            button_upload.Enabled = true;
            button_download.Enabled = true;
        }

        // HELPER FUNCTIONS
        static string generate_HexString_from_ByteArray(byte[] input)
        {
            string hexString = BitConverter.ToString(input);
            return hexString.Replace("-", "");
        }

        public static byte[] generate_ByteArray_from_HexString(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
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


        // RSA encryption with varying bit length
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

        // verifying with RSA
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

        /*private static byte[] DecryptWithRSA(string input, int algoLength, string xmlStringKey)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlStringKey);
            //byte[] result = null

        }*/

        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }

        private void WriteToFileAsBytes(string decryptedFileNameStr, byte[] decryptedFileContentTemp)
        {
            using (FileStream data = new FileStream(decryptedFileNameStr, FileMode.Append))
            {
                data.Write(decryptedFileContentTemp, 0, decryptedFileContentTemp.Length);
            }
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

        private void ChangeFileName(string decryptedFileNameStr, string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            System.IO.File.Move(decryptedFileNameStr, filename);
        }

    }

    
}
