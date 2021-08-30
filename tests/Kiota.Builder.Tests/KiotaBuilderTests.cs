using Microsoft.OpenApi.Services;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Moq;
using Xunit;
using System.Collections.Generic;
using Microsoft.OpenApi.Any;

namespace Kiota.Builder.Tests
{
    public class KiotaBuilderTests
    {
        [Fact]
        public void Single_root_node_creates_single_request_builder_class()
        {
            var node = OpenApiUrlTreeNode.Create();
            var mockLogger = new Mock<ILogger<KiotaBuilder>>();
            var builder = new KiotaBuilder(mockLogger.Object, new GenerationConfiguration() { ClientClassName = "Graph" });
            var codeModel = builder.CreateSourceModel(node);

            Assert.Single(codeModel.GetChildElements(true));
        }
        [Fact]
        public void Single_path_with_get_collection()
        {
            var node = OpenApiUrlTreeNode.Create();
            node.Attach("tasks", new OpenApiPathItem() {
                Operations = {
                    [OperationType.Get] = new OpenApiOperation() { 
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse()
                            {
                                Content =
                                {
                                    ["application/json"] = new OpenApiMediaType()
                                    {
                                        Schema = new OpenApiSchema
                                        {
                                            Type = "array",
                                            Items = new OpenApiSchema
                                            {
                                                Type = "int"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                } 
            }, "default");
            var mockLogger = new Mock<ILogger<KiotaBuilder>>();
            var builder = new KiotaBuilder(mockLogger.Object, new GenerationConfiguration() { ClientClassName = "Graph" });
            var codeModel = builder.CreateSourceModel(node);

            var rootNamespace = codeModel.GetChildElements(true).Single();
            var rootBuilder = rootNamespace.GetChildElements(true).Single(e => e.Name == "Graph");
            var tasksProperty = rootBuilder.GetChildElements(true).OfType<CodeProperty>().Single(e => e.Name == "Tasks");
            var tasksRequestBuilder = tasksProperty.Type as CodeType;
            Assert.NotNull(tasksRequestBuilder);
            var getMethod = tasksRequestBuilder.TypeDefinition.GetChildElements(true).OfType<CodeMethod>().Single(e => e.Name == "Get");
            var returnType = getMethod.ReturnType;
            Assert.Equal(CodeTypeBase.CodeTypeCollectionKind.Array, returnType.CollectionKind);
        }
        [Fact]
        public void OData_doubles_as_any_of(){
            var node = OpenApiUrlTreeNode.Create();
            node.Attach("tasks", new OpenApiPathItem() {
                Operations = {
                    [OperationType.Get] = new OpenApiOperation() { 
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse()
                            {
                                Content =
                                {
                                    ["application/json"] = new OpenApiMediaType()
                                    {
                                        Schema = new OpenApiSchema
                                        {
                                            Type = "object",
                                            Properties = new Dictionary<string, OpenApiSchema> {
                                                {
                                                    "progress", new OpenApiSchema{
                                                        AnyOf = new List<OpenApiSchema>{
                                                            new OpenApiSchema{
                                                                Type = "number"
                                                            },
                                                            new OpenApiSchema{
                                                                Type = "string"
                                                            },
                                                            new OpenApiSchema {
                                                                Enum = new List<IOpenApiAny> { new OpenApiString("-INF"), new OpenApiString("INF"), new OpenApiString("NaN") }
                                                            }
                                                        },
                                                        Format = "double"
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                } 
            }, "default");
            var mockLogger = new Mock<ILogger<KiotaBuilder>>();
            var builder = new KiotaBuilder(mockLogger.Object, new GenerationConfiguration() { ClientClassName = "Graph" });
            var codeModel = builder.CreateSourceModel(node);
            var progressProp = codeModel.FindChildByName<CodeProperty>("progress", true);
            Assert.Equal("double", progressProp.Type.Name);
        }
        [Fact]
        public void Object_Arrays_are_supported() {
            var node = OpenApiUrlTreeNode.Create();
            var usersNode = node.Attach("users", new OpenApiPathItem() {
                
            }, "default");
            usersNode.Attach("{id}", new OpenApiPathItem() {
                Operations = {
                    [OperationType.Get] = new OpenApiOperation() {
                        Responses = new OpenApiResponses {
                            ["200"] = new OpenApiResponse() {
                                Content = {
                                    ["application/json"] = new OpenApiMediaType() {
                                        Schema = new OpenApiSchema {
                                            Type = "object",
                                            Properties = new Dictionary<string, OpenApiSchema> {
                                                {
                                                    "value", new OpenApiSchema {
                                                        Type = "array",
                                                        Items = new OpenApiSchema {
                                                            Type = "object",
                                                            Title = "user", // unit test fails if the title is not set
                                                            Properties = new Dictionary<string, OpenApiSchema> {
                                                                {
                                                                    "id", new OpenApiSchema {
                                                                        Type = "string"
                                                                    }
                                                                },
                                                                {
                                                                    "displayName", new OpenApiSchema {
                                                                        Type = "string"
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }, "default");
            var mockLogger = new Mock<ILogger<KiotaBuilder>>();
            var builder = new KiotaBuilder(mockLogger.Object, new GenerationConfiguration() { ClientClassName = "Graph" });
            var codeModel = builder.CreateSourceModel(node);
        }
    }
}
