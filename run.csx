#r "Newtonsoft.Json"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{

    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
    dynamic requestData = JsonConvert.DeserializeObject(requestBody);

    JObject jsonRequest = JsonConvert.DeserializeObject<JObject>(requestBody);
    string prompt = jsonRequest["prompt"].ToString();
    string theme = jsonRequest["theme"].ToString();

    string promptWithTheme = $"{prompt} in {theme}";


    JObject jsonResponse = new JObject
    {
        { "role", "user" },
        { "prompt", promptWithTheme }    
    };


dynamic enginedata = new
{
    dataSources = new List<dynamic>
    {
        new
        {
            type = "AzureCognitiveSearch",
            parameters = new
            {
                endpoint = "https://verstcarbon-cognitive-search.search.windows.net",
                key = "ikN2vh3d9Hf9Bkk5CdEJjiT4ErBSX7VUPKoSadTPDcAzSeCGjJnW",
                indexName = "verstcarbon-index",
                semanticConfiguration = "",
                queryType = "simple",
                fieldsMapping = new
                {
                    contentFieldsSeparator = "\n",
                    contentFields = new List<string>
                    {
                        "content"
                    },
                    filepathField = (string)null,
                    titleField = (string)null,
                    urlField = (string)null
                },
                inScope = false,
                roleInformation = "You are an Africa Climate Summit (ACS) assistant who's happy to help people. You provide information about organizations relating to the ACS thematic areas. Always provide your output in a numbered list format unless it is not suitable."
            }
        }
    },
    messages = new List<dynamic>
    {
        new
        {
            role = "user",
            content = promptWithTheme
        }
    },
    deployment = "verstcarbon-openai",
    temperature = 0,
    top_p = 1,
    max_tokens = 800,
    stop = (string)null,
    stream = false
};

 try
    {
        // Send the enginedata as a POST request and get the response
        dynamic response = await SendPostRequest(enginedata);
       
       
        string id = response.id;
        string promptt = promptWithTheme;
        string assistantResponse = response.choices[0].messages[1].content;
        string citation = response.choices[0].messages[0].content;
        DateTime time = DateTime.Now;

        // Create the modified response object
        dynamic modifiedResponse = new
        {
            id = id,
            prompt = promptt,
            response = assistantResponse,
            citation = citation,
            time = time
        };

        log.LogInformation($"Modified Response: {JsonConvert.SerializeObject(modifiedResponse.citation)}");

        // Return the modified response as JSON
        return new OkObjectResult(modifiedResponse);
        // Return the response
        // return new OkObjectResult(response);
    }
    catch (Exception ex)
    {
        return new BadRequestObjectResult(ex.Message);
    }
    //return new OkObjectResult(jsonResponse.ToString());
}



public static async Task<dynamic> SendPostRequest(dynamic data)
{
     var httpClient = new HttpClient();
    var requestUri = new Uri("https://verstcarbon-openai.openai.azure.com/openai/deployments/verstcarbon-openai/extensions/chat/completions?api-version=2023-06-01-preview");

    // Convert the dynamic object (enginedata) into JSON
    var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(data);
    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

    // Add the API key to the request headers
    httpClient.DefaultRequestHeaders.Add("api-key", "c711490482724177bf2a7aa4cd2768db");

    // Send POST request and get the response
    var response = await httpClient.PostAsync(requestUri, content);
    if (!response.IsSuccessStatusCode)
    {
        throw new Exception($"Error: {response.StatusCode} - {response.ReasonPhrase}");
    }

    // Read the response content as a dynamic object
    var responseContent = await response.Content.ReadAsStringAsync();
    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseContent);

    return responseObject;
}

