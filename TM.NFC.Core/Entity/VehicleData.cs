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
        internal string StationInId { get; set; }
        internal string LaneInId { get; set; }
        internal StatusBlock1 Status { get; set; }
        internal TypeBlock1 CardType { get; set; }
        internal DateTime ArriveTime { get; set; }
        internal string Plate { get; set; }

        public VehicleData()
        {

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

        public override string ToString()
        {
            return $"{{\"StationInId\":\"{StationInId}\",\"LaneInId\":\"{LaneInId}\",\"Status\":{Convert.ToByte(Status)},\"CardType\":{Convert.ToByte(CardType)}}}";
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
