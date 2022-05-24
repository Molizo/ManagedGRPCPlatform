using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    public enum ClientStatus
    { Registered, Activated, Banned }

    public class Client
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string SerialNr { get; set; }

        public List<Message> Messages { get; set; }
        public string RegisteredBy { get; set; }

        public ClientStatus Status { get; set; }
        public string ClientToServerPublicKey { get; set; }
        public string ClientToServerPrivateKey { get; set; }
        public DateTime LastActivityAt { get; set; }
    }
}