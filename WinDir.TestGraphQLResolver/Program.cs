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

var query1 = new GraphQLRequest
{
    Query="query q1 {hello}"
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
