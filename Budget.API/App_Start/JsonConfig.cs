using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Web.Http;

namespace Budget.API.App_Start
{
    public static class JsonConfig
    {
        public static void DateFormat(HttpConfiguration config)
        {
            IsoDateTimeConverter converter = new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-dd" };
            ICollection<Newtonsoft.Json.JsonConverter> converters = config.Formatters.JsonFormatter.SerializerSettings.Converters;
            converters.Add(converter);
        }
    }
}