using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1.Models
{
    class TransferResponse
    {
        public string Message { get; set; } // In case of ownership confirmation
        public Asset Asset { get; set; } // Details about the asset
        public string OwnerUsername { get; set; } // Username of the asset owner
    }
}
