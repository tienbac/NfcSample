using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TN.NFC.Core.Entity
{
    public class VehicleData
    {
        public string StationInId { get; set; }
        public string LaneInId { get; set; }
        public StatusBlock1 Status { get; set; }
        public TypeBlock1 CardType { get; set; }
        public DateTime ArriveTime { get; set; }
        public string Plate { get; set; }
        public int VehicleType { get; set; }

        public VehicleData()
        {

        }

        public VehicleData(string stationInId, string laneInId, StatusBlock1 status, TypeBlock1 cardType, DateTime arriveTime, string plate, int vehicleType)
        {
            StationInId = stationInId;
            LaneInId = laneInId;
            Status = status;
            CardType = cardType;
            ArriveTime = arriveTime;
            Plate = plate;
            VehicleType = vehicleType;
        }

        public VehicleData(string dataCard, string time, string plate)
        {
            dataCard = dataCard.Replace("$", "");
            StationInId = dataCard.Substring(0, 4);
            LaneInId = dataCard.Substring(4, 3);
            Status = (StatusBlock1)Convert.ToInt32(dataCard.Substring(7, 3));
            CardType = (TypeBlock1)Convert.ToInt32(dataCard.Substring(10, 3));

            time = time.Replace("$", "");
            ArriveTime = DateTime.ParseExact(time, "yyyyMMddHHmmss", CultureInfo.CurrentCulture);

            plate = plate.Replace("$", "");
            Plate = plate.ToUpper();
        }

        public VehicleData(string data)
        {
            char[] separators = new char[] { ' ', ';', ',', '\r', '\t', '\n', '$' };

            string[] temp = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);


            StationInId = temp[0].Substring(0, 4);
            LaneInId = temp[0].Substring(4, 3);
            Status = (StatusBlock1)Convert.ToInt32(temp[0].Substring(7, 3));
            CardType = (TypeBlock1)Convert.ToInt32(temp[0].Substring(10, 3));
            VehicleType = Convert.ToInt32(temp[0].Substring(13, 1));

            var time = temp[1];
            ArriveTime = DateTime.ParseExact(time, "yyyyMMddHHmmss", CultureInfo.CurrentCulture);

            Plate = temp[2].ToUpper();
        }

        private string JoinString(string data)
        {
            var length = 16 - data.Length;
            var x = 0;
            do
            {
                data += "$";
                x += 1;
            } while (x < length);

            return data;
        }

        public override string ToString()
        {
            var block4 = JoinString($"{StationInId}{LaneInId}{Convert.ToInt32(Status):D3}{Convert.ToInt32(CardType):D3}{VehicleType}");
            var block5 = JoinString($"{ArriveTime:yyyyMMddHHmmss}");
            var block6 = JoinString($"{Plate}");
            var result = $"{block4}{block5}{block6}";
            return result;
        }
    }

    public enum StatusBlock1 : byte
    {
        Unpaid =0,
        Paid = 1,
        Specially = 2
    }

    public enum TypeBlock1 : byte
    {
        BinhThuong = 0,
        MatThe = 1,
        TheNhanVien = 2,
        TheUuTienDoan = 3,
        TheUuTien1XeCoDauHieu = 4,
        TheUuTien1XeKhongCoDauHieu = 5,
        TheUuTienBoTaiChinh = 6,
        TheUuTienCongAn = 7,
        TheUuTienQuanDoi = 8,
        TheSuCo = 9,
        TheThuPhiVeGiay = 10
    }
}
