using GraphQL;
using GraphQL.SystemTextJson;
using Newtonsoft.Json;
using WinDir.GraphQLResolver;
using WinDir.GraphQLResolver.GraphQL;

ResolverEntry aEntry = new ResolverEntry();

//string graphQlQueryJsonString = @"{""query"": ""{\n hello \n}\n""}";

string query = @"
                    {
                        hello
                    }
                ";

string graphQlQueryJsonString = $@"{{""query"": {JsonConvert.SerializeObject(query)}}}";

GraphQLQuery graphQLQuery = JsonConvert.DeserializeObject<GraphQLQuery>(graphQlQueryJsonString);
var result = await aEntry.GetResultAsync(graphQLQuery);

var resultString = await new DocumentWriter().WriteToStringAsync(result);

Console.WriteLine(resultString);
Console.WriteLine("Done.");
Console.ReadLine();
