using System.Net.Http.Headers;

namespace Warehouse.Api.IntegrationTests.TestUtilities.Helpers;

public static class MultipartFormHelper
{
    public static MultipartFormDataContent CreateFileContent(
        string fieldName, string fileName, string contentType, byte[] fileBytes)
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(fileContent, fieldName, fileName);
        return content;
    }
}