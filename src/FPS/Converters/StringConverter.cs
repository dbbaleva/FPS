using System.Data.Common;
using FPS.Data;

namespace FPS.Converters
{
    public class StringConverter : IEntityConverter<string>
    {
        public string ConvertToEntity(DbDataRecord record)
        {
            return record[0]?.ToString();
        }
    }
}
