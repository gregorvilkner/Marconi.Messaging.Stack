﻿using GraphQL;
using GraphQL.SystemTextJson;
using GraphQL.Transport;
using Newtonsoft.Json;
using System.Text;
using WinDir.GraphQLResolver;
using WinDir.GraphQLResolver.GraphQL;

ResolverEntry aEntry = new ResolverEntry();

var query = @"{
 ""query"": ""{ hello }""
}";

var request = new GraphQLSerializer().Deserialize<GraphQLRequest>(query);

var result = await aEntry.GetResultAsync(request);

// https://www.lucanatali.it/c-from-string-to-stream-and-from-stream-to-string/
var ms = new MemoryStream();
var sw = new StreamWriter(ms);

await new GraphQLSerializer().WriteAsync(ms, result);

string resultJson = Encoding.ASCII.GetString(ms.ToArray());

// folks say the above can memory out and this may be better
// https://stackoverflow.com/questions/78181/how-do-you-get-a-string-from-a-memorystream

//memoryStream.Position = 0;
//var aStreamReader = new StreamReader(memoryStream);
//var myStr = aStreamReader.ReadToEnd();





Console.WriteLine(resultJson);
Console.WriteLine("Done.");
Console.ReadLine();
