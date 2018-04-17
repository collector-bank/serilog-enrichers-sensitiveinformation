using System.IO;

using Newtonsoft.Json;

using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Collector.Serilog.SensitiveInformation.UnitTest.Helpers
{
    public class JsonSink : ILogEventSink
    {
        public string Result { get; set; }

        public string Properties
        {
            get
            {
                var deserializedObject = JsonConvert.DeserializeObject<OnlyProperties>(Result);
                return JsonConvert.SerializeObject(deserializedObject.Properties, Formatting.None);
            }
        }

        public void Emit(LogEvent logEvent)
        {
            var test = new JsonFormatter();
            var stringWriter = new StringWriter();
            test.Format(logEvent, stringWriter);

            Result = stringWriter.ToString();
        }

        private class OnlyProperties
        {
            public object Properties { get; set; }
        }
    }
}