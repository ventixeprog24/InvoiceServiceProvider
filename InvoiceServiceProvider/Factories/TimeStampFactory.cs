using Google.Protobuf.WellKnownTypes;

namespace InvoiceServiceProvider.Factories
{
    public class TimeStampFactory
    {
        public static Timestamp ToTimeStamp(DateTime time)
        {
            DateTime date = time.Date;
            Timestamp timestamp = date.ToTimestamp();
            return timestamp;
        }

        public static DateTime ToDateTime(Timestamp timestamp)
        {
            DateTime dateTime = timestamp.ToDateTime().ToUniversalTime();
            DateTime dateOnly = dateTime.Date;
            return dateOnly;
        }
    }
}
