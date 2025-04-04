using TrackAndTrace_API.Models.DBModel;

namespace TrackAndTrace_API.Models.RequestModel
{
    public class UserAttendanceDto
    {
        public int id { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string address { get; set; }
        public string image { get; set; }
        public string attendance_type { get; set; }
        public string attendance_timestamp { get; set; }
    }
    public class DeviceAttendanceDto
    {
        public int device_user_id { get; set; }
        public string device_user_name { get; set; }
        public string address { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string attendance_timestamp { get; set; }
    }
}