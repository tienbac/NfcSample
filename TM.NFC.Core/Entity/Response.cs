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
        public string Imei { get; set; }
        public StatusResponse Status { get; set; }
        public string ErrorMessage { get; set; }
        public VehicleData VehicleData { get; set; }

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
