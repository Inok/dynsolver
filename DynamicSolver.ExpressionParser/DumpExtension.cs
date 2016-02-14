using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DynamicSolver.ExpressionParser
{
    public class DumpSettings
    {
        public static DumpSettings Default { get; } = new DumpSettings();
        public static DumpSettings Inline { get; } = new DumpSettings() { Indented = false };

        public bool Indented { get; set; }

        public bool IgnoreLoopReferences { get; set; }

        public bool IgnoreExceptionsWhenSerialize { get; set; }

        public bool IgnoreNull { get; set; }

        public bool IncludeTypes { get; set; }

        public bool EnumAsString { get; set; }

        public DumpSettings()
        {
            Indented = true;
            IgnoreLoopReferences = true;
            IgnoreExceptionsWhenSerialize = true;
            IgnoreNull = false;
            IncludeTypes = true;
            EnumAsString = true;
        }
    }

    public static class DumpExtension
    {
        public static string Dump(this object _this)
        {
            return _this.Dump(DumpSettings.Default);
        }

        public static string DumpInline(this object _this)
        {
            return _this.Dump(DumpSettings.Inline);
        }

        public static string Dump(this object _this, [NotNull] DumpSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var serializerSettings = new JsonSerializerSettings()
            {
                Formatting = settings.Indented ? Formatting.Indented : Formatting.None,
                ReferenceLoopHandling = settings.IgnoreLoopReferences ? ReferenceLoopHandling.Ignore : ReferenceLoopHandling.Error,
                NullValueHandling = settings.IgnoreNull ? NullValueHandling.Ignore : NullValueHandling.Include,
                TypeNameHandling = settings.IncludeTypes ? TypeNameHandling.Auto : TypeNameHandling.None
            };

            if (settings.IgnoreExceptionsWhenSerialize)
            {
                serializerSettings.Error = (sender, args) => args.ErrorContext.Handled = true;
            }

            if (settings.EnumAsString)
            {
                serializerSettings.Converters.Add(new StringEnumConverter());
            }

            return JsonConvert.SerializeObject(_this, serializerSettings);
        }
    }
}