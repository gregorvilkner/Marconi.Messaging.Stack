using GraphQL;
using GraphQL.NewtonsoftJson;
//using GraphQL.SystemTextJson;
using GraphQL.Transport;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using WinDir.GraphQLResolver;
using WinDir.GraphQLResolver.GraphQL;

ResolverEntry aEntry = new ResolverEntry();

var query = new GraphQLRequest
{
    Query="{hello}"
};

var folderList = "\"c:\\\\Program Files\\\\7-Zip\",\"c:\\\\Program Files\\\\Adobe\",\"c:\\\\Program Files\\\\Android\",\"c:\\\\Program Files\\\\Application Verifier\",\"c:\\\\Program Files\\\\Azure Data Studio\",\"c:\\\\Program Files\\\\Bonjour\",\"c:\\\\Program Files\\\\Common Files\",\"c:\\\\Program Files\\\\Cyberduck\",\"c:\\\\Program Files\\\\Dell\",\"c:\\\\Program Files\\\\DIFX\",\"c:\\\\Program Files\\\\dotnet\",\"c:\\\\Program Files\\\\Eclipse Foundation\",\"c:\\\\Program Files\\\\Git\",\"c:\\\\Program Files\\\\Goodix\",\"c:\\\\Program Files\\\\Google\",\"c:\\\\Program Files\\\\GraphiQL\",\"c:\\\\Program Files\\\\HPPrintScanDoctor\",\"c:\\\\Program Files\\\\Hyper-V\",\"c:\\\\Program Files\\\\IIS\",\"c:\\\\Program Files\\\\IIS Express\",\"c:\\\\Program Files\\\\Intel\",\"c:\\\\Program Files\\\\Internet Explorer\",\"c:\\\\Program Files\\\\IrfanView\",\"c:\\\\Program Files\\\\Java\",\"c:\\\\Program Files\\\\Killer Networking\",\"c:\\\\Program Files\\\\LogiOptionsPlus\",\"c:\\\\Program Files\\\\Logitech\",\"c:\\\\Program Files\\\\Microsoft\",\"c:\\\\Program Files\\\\Microsoft Analysis Services\",\"c:\\\\Program Files\\\\Microsoft Office\",\"c:\\\\Program Files\\\\Microsoft Office 15\",\"c:\\\\Program Files\\\\Microsoft SDKs\",\"c:\\\\Program Files\\\\Microsoft SQL Server\",\"c:\\\\Program Files\\\\Microsoft Visual Studio\",\"c:\\\\Program Files\\\\Microsoft Visual Studio 10.0\",\"c:\\\\Program Files\\\\Microsoft.NET\",\"c:\\\\Program Files\\\\ModifiableWindowsApps\",\"c:\\\\Program Files\\\\MSBuild\",\"c:\\\\Program Files\\\\nodejs\",\"c:\\\\Program Files\\\\NVIDIA Corporation\",\"c:\\\\Program Files\\\\OpenConnect\",\"c:\\\\Program Files\\\\OpenVPN Connect\",\"c:\\\\Program Files\\\\Oracle\",\"c:\\\\Program Files\\\\PI\",\"c:\\\\Program Files\\\\PIPC\",\"c:\\\\Program Files\\\\PostgreSQL\",\"c:\\\\Program Files\\\\PowerShell\",\"c:\\\\Program Files\\\\Reference Assemblies\",\"c:\\\\Program Files\\\\TAP-Windows\",\"c:\\\\Program Files\\\\UnifiedAutomation\",\"c:\\\\Program Files\\\\Uninstall Information\",\"c:\\\\Program Files\\\\VMware\",\"c:\\\\Program Files\\\\VS2010Schemas\",\"c:\\\\Program Files\\\\VS2012Schemas\",\"c:\\\\Program Files\\\\Windows Defender\",\"c:\\\\Program Files\\\\Windows Defender Advanced Threat Protection\",\"c:\\\\Program Files\\\\Windows Mail\",\"c:\\\\Program Files\\\\Windows Media Player\",\"c:\\\\Program Files\\\\Windows NT\",\"c:\\\\Program Files\\\\Windows Photo Viewer\",\"c:\\\\Program Files\\\\Windows Sidebar\",\"c:\\\\Program Files\\\\WindowsApps\",\"c:\\\\Program Files\\\\WindowsPowerShell\"";

var query1 = new GraphQLRequest
{
    //Query=$"query q1 {{ folders(aFolderDirList: [\"c:\\\\Program Files\", \"c:\\\\Program Files (x86)\"]) {{ dir name folders {{ dir name}} }} }}"
    Query = $"query q1 {{ folders(aFolderDirList: [{folderList}]) {{ dir name folders {{ dir name}} }} }}"
};

var query2 = JsonConvert.DeserializeObject<GraphQLRequest>(@"{ ""OperationName"":null,""Query"":""{\n  hello\n}\n\n\n"",""Variables"":{ ""MarconiNr"":""162-509-2553""},""Extensions"":null}");

var result = await aEntry.GetResultAsync(query1);

// using GraphQL.NewtonsoftJson
var stringWriter = new StringWriter();
new GraphQLSerializer().Write(stringWriter, result);

// using GraphQL.SystemTextJson
//// https://www.lucanatali.it/c-from-string-to-stream-and-from-stream-to-string/
//var ms2 = new MemoryStream();
//var sw2 = new StreamWriter(ms2);
//await new GraphQLSerializer().WriteAsync(ms2, result2);
//string resultJson2 = Encoding.ASCII.GetString(ms2.ToArray());



Console.WriteLine(stringWriter.ToString());


//Console.WriteLine(resultJson);
Console.WriteLine("Done.");
Console.ReadLine();
