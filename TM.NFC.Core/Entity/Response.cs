using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace TN.NFC.Core.Entity
{
    public enum StatusResponse
    {
        OK = 1,
        NotOk = 0
    }

    public class Response
    {
        internal string Imei { get; set; }
        internal StatusResponse Status { get; set; }
        internal string ErrorMessage { get; set; }
        internal VehicleData VehicleData { get; set; }

        public Response()
        {
            
        }

        public Response(string imei, StatusResponse status, string errorMessage, VehicleData vehicleData)
        {
            Imei = imei;
            Status = status;
            ErrorMessage = errorMessage;
            VehicleData = vehicleData;
        }
    }
}
