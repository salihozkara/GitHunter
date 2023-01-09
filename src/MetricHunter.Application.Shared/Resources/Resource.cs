using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Reflection;
using JsonNet.ContractResolvers;
using MetricHunter.Core.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Octokit;

namespace MetricHunter.Application.Resources;

// TODO: Refactor
public static class Resource
{
    private const string ResFolder = "Res";

    private static readonly string DynamicResFolder = GetOrCreateResFolder();

    public static ResValue<IEnumerable<Repository>> RepositoriesJson => new($"{DynamicResFolder}/repositories.json",
        JsonConvert
            .DeserializeObject<Repository[]>(
                File.ReadAllText(
                    $"{DynamicResFolder}/repositories.json")) ??
        Array.Empty<Repository>());

    private static string GetOrCreateResFolder()
    {
        var userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        const string slnName = "ProjectStatistics";
        var privateSlnFolderName = "." + slnName.ToLower();
        var resFolder = Path.Combine(userFolder, privateSlnFolderName, ResFolder);
        if (!Directory.Exists(resFolder)) Directory.CreateDirectory(resFolder);
        var baseResFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, ResFolder);
        foreach (var file in Directory.GetFiles(baseResFolder, "*.*", SearchOption.AllDirectories))
        {
            var relativePath = file[(baseResFolder.Length + 1)..];
            var targetFile = Path.Combine(resFolder, relativePath);
            if (File.Exists(targetFile)) continue;
            var targetFolder = Path.GetDirectoryName(targetFile);
            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder!);
            File.Copy(file, targetFile, true);
        }

        return resFolder;
    }

    public static class SourceMonitor
    {
        public static readonly ResValue<string> TemplateXml = new($"{DynamicResFolder}/SourceMonitor/template.xml",
            File.ReadAllText($"{DynamicResFolder}/SourceMonitor/template.xml"));

        public static readonly string XmlReportsFolder =
            PathHelper.BuildAndCreateFullPath($"{DynamicResFolder}/SourceMonitor/XmlReports");

        public static ResValue<Process> SourceMonitorExe => new($"{DynamicResFolder}/SourceMonitor/SourceMonitor.exe",
            new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "SourceMonitor.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = $"{DynamicResFolder}/SourceMonitor"
                }
            });
    }

    public static class Jsons
    {
        public static readonly JsonSerializerSettings JsonSerializerSettings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            MaxDepth = 100,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            DateParseHandling = DateParseHandling.DateTimeOffset,
            FloatFormatHandling = FloatFormatHandling.String,
            FloatParseHandling = FloatParseHandling.Double,
            StringEscapeHandling = StringEscapeHandling.Default,
            Culture = CultureInfo.InvariantCulture,
            CheckAdditionalContent = true,
            Converters = new JsonConverter[] { new StringEnumConverter() },
            ContractResolver = new PrivateSetterContractResolver()
        };

        public static string Path => $"{DynamicResFolder}/Jsons";

        public static Dictionary<string, dynamic?> Values
        {
            get
            {
                var jsons = new Dictionary<string, dynamic?>();
                var jsonsFolder = $"{DynamicResFolder}/Jsons";

                foreach (var file in Directory.GetFiles(jsonsFolder, "*.json", SearchOption.AllDirectories))
                {
                    var relativePath = file[(jsonsFolder.Length + 1)..];
                    var json = JsonConvert.DeserializeObject(File.ReadAllText(file), typeof(ExpandoObject),
                        JsonSerializerSettings);
                    jsons.Add(relativePath, json);
                }

                return jsons;
            }
        }

        public static ResValue<T?> GetJson<T>(string path)
        {
            return new ResValue<T?>($"{Path}/{path}",
                JsonConvert.DeserializeObject<T>(File.ReadAllText($"{Path}/{path}")));
        }

        public static void SaveJson<T>(string path, T value)
        {
            var json = JsonConvert.SerializeObject(value, JsonSerializerSettings);
            var pathToFile = $"{Path}/{path}";
            var folder = System.IO.Path.GetDirectoryName(pathToFile);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder!);

            File.WriteAllText(pathToFile, json);
        }
    }


    public static class Xmls
    {
        public static string Path => $"{DynamicResFolder}/Xmls";

        public static List<FileInfo> Files => new(Directory.GetFiles(Path, "*.xml", SearchOption.AllDirectories)
            .Select(x => new FileInfo(x)));
    }
}