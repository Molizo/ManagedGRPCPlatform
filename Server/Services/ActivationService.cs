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
    public class ActivationService : Activation.ActivationBase
    {
        private readonly ApplicationDbContext _context;

        public ActivationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public override async Task<ActivationReply> Activation(ActivationRequest request, ServerCallContext serverCallContext)
        {
            Console.WriteLine("Received activation request from " + request.Serial);

            if (!_context.Client.Any(c => c.SerialNr == request.Serial))
                return new ActivationReply
                {
                    Message = "Invalid serial"
                };

            Client client = await _context.Client.Include(c => c.Messages).FirstAsync(c => c.SerialNr == request.Serial);
            if (client.Status != ClientStatus.Registered)
                return new ActivationReply
                {
                    Message = "This serial number is not in a state which allows activation: " + client.Status.ToString()
                };

            RSA rsa = RSA.Create();
            client.ClientToServerPublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
            client.ClientToServerPrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

            client.Status = ClientStatus.Activated;
            client.LastActivityAt = DateTime.Now;
            _context.Client.Update(client);
            await _context.SaveChangesAsync();

            return new ActivationReply
            {
                Message = "OK",
                ClientToServerPublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey())
            };
        }
    }
}