using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace GrillBot.Core.Database.ValueConverters;

public class JsonValueConverter<TEntityPropertyType>(JsonSerializerOptions? options = null) : ValueConverter<TEntityPropertyType, string>(
    model => JsonSerializer.Serialize(model, options),
    json => JsonSerializer.Deserialize<TEntityPropertyType>(json, options)!
);
