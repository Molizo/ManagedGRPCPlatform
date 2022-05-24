using System.Threading.Tasks;
using Grpc.Net.Client;
using Client;
using System.Security.Cryptography;
using System.Text;

namespace Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.Write("Please enter serial number: ");
            string serial = Console.ReadLine();
            Console.Write("Please enter server address: ");
            string address = Console.ReadLine();

            using var channel = GrpcChannel.ForAddress(address);
            Console.WriteLine("Connected to server");
            Console.WriteLine();

            var activationClient = new Activation.ActivationClient(channel);
            string clientToServerPK = "";
            try
            {
                clientToServerPK = System.IO.File.ReadAllText(serial + "_PublicKey.txt");
            }
            catch { }
            if (clientToServerPK == "")
            {
                Console.Write("Would you like to perform automatic activation for this serial number? (Y/N): ");
                if (Console.ReadLine().ToLower().Contains('y'))
                {
                    var reply = activationClient.Activation(
                      new ActivationRequest { Serial = serial });
                    if (reply.Message == "OK")
                    {
                        clientToServerPK = reply.ClientToServerPublicKey;
                        System.IO.File.WriteAllText(serial + "_PublicKey.txt", clientToServerPK);
                    }
                    Console.WriteLine("Activation request reply: " + reply.Message);
                }
                else
                {
                    Console.Write("Please enter the client to server public key: ");
                    clientToServerPK = Console.ReadLine();
                }
            }

            if (clientToServerPK != "")
            {
                Console.WriteLine("Configuration finished, press any key to continue...");
                Console.ReadKey();
                Console.Clear();

                var messagesClient = new AddMessage.AddMessageClient(channel);
                while (true)
                {
                    Console.Write("Enter message to send: ");
                    string msg = Console.ReadLine();
                    var reply = messagesClient.AddMessage(
                      new AddMessageRequest { Serial = serial, Message = Encrypt(msg, clientToServerPK) });
                    Console.WriteLine("Message reply: " + reply.Message);
                    Console.WriteLine();
                }
            }

            Console.WriteLine("This program has ended, press any key to continue...");
            Console.ReadKey();
        }

        private static UnicodeEncoding _encoder = new UnicodeEncoding();

        public static string Encrypt(string data, string publicKey)
        {
            var rsa = new RSACryptoServiceProvider();
            int readPublicKeyBytes;
            rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out readPublicKeyBytes);
            var dataToEncrypt = _encoder.GetBytes(data);
            var encryptedByteArray = rsa.Encrypt(dataToEncrypt, false).ToArray();
            var length = encryptedByteArray.Count();
            var item = 0;
            var sb = new StringBuilder();
            foreach (var x in encryptedByteArray)
            {
                item++;
                sb.Append(x);

                if (item < length)
                    sb.Append(",");
            }

            return sb.ToString();
        }
    }
}