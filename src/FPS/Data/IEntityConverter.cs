using System.Data.Common;

namespace FPS.Data
{
    public interface IEntityConverter<out T>
    {
        T ConvertToEntity(DbDataRecord record);
    }
}
