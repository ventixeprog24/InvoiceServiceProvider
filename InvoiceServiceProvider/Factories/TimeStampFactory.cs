using Google.Protobuf.WellKnownTypes;

namespace InvoiceServiceProvider.Factories
{
    public class TimeStampFactory
    {
        public static Timestamp ToTimeStamp(DateTime time)
        {
            Timestamp timestamp = time.ToTimestamp();
            return timestamp;
        }

        public static DateTime ToDateTime(Timestamp timestamp)
        {
            DateTime dateTime = timestamp.ToDateTime();
            return dateTime;
        }
    }
}
