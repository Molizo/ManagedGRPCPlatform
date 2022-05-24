using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Services
{
    public class AddMessageService : AddMessage.AddMessageBase
    {
        private readonly ApplicationDbContext _context;

        public AddMessageService(ApplicationDbContext context)
        {
            _context = context;
        }

        public override async Task<AddMessageReply> AddMessage(AddMessageRequest request, ServerCallContext serverCallContext)
        {
            Console.WriteLine("Received message from " + request.Serial + ": " + request.Message);

            if (!_context.Client.Any(c => c.SerialNr == request.Serial))
                return new AddMessageReply
                {
                    Message = "Invalid serial"
                };

            Client client = await _context.Client.Include(c => c.Messages).FirstAsync(c => c.SerialNr == request.Serial);
            if (client.Status != ClientStatus.Activated)
                return new AddMessageReply
                {
                    Message = "Current state of serial number does not authorize the client to perform this operation: " + client.Status.ToString()
                };

            Message msg = new Message()
            {
                ReceivedAt = DateTime.Now,
                Content = Decrypt(request.Message, client.ClientToServerPrivateKey)
            };
            client.Messages.Add(msg);
            client.LastActivityAt = DateTime.Now;
            _context.Client.Update(client);
            await _context.SaveChangesAsync();

            return new AddMessageReply
            {
                Message = "OK"
            };
        }

        private static UnicodeEncoding _encoder = new UnicodeEncoding();

        public static string Decrypt(string data, string privateKey)
        {
            var rsa = new RSACryptoServiceProvider();
            var dataArray = data.Split(new char[] { ',' });
            byte[] dataByte = new byte[dataArray.Length];
            for (int i = 0; i < dataArray.Length; i++)
            {
                dataByte[i] = Convert.ToByte(dataArray[i]);
            }

            int readPrivateKeyBytes;
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out readPrivateKeyBytes);
            var decryptedByte = rsa.Decrypt(dataByte, false);
            return _encoder.GetString(decryptedByte);
        }
    }
}