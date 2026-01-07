using Microsoft.EntityFrameworkCore;
using System.Data.SqlTypes;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FreedomDanceStudio.Data;

public class UtcDateTimeConverter: ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    { }
}