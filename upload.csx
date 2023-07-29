#r "Newtonsoft.Json"
#r "AWSSDK.S3"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Amazon.S3;
using Amazon.S3.Model;

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    log.LogInformation("C# HTTP trigger function processed a request.");

    // Set your AWS credentials and region.
    string accessKey = "YOUR_ACCESS_KEY";
    string secretKey = "YOUR_SECRET_ACCESS_KEY";
    string bucketName = "acs-dmp-bucket";
    string region = "us-east-1";

    // Set up AWS credentials and region
    var credentials = new Amazon.Runtime.BasicAWSCredentials(accessKey, secretKey);
    var config = new AmazonS3Config
    {
        RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region)
    };

    // Create S3 client
    using (var s3Client = new AmazonS3Client(credentials, config))
    {
        try
        {
            // Request to list objects in the bucket
            ListObjectsV2Request request = new ListObjectsV2Request
            {
                BucketName = bucketName
            };

            // List the objects in the bucket
            ListObjectsV2Response response;
            do
            {
                response = await s3Client.ListObjectsV2Async(request);

                // Process the objects
                foreach (S3Object entry in response.S3Objects)
                {
                    log.LogInformation($"File: {entry.Key}");
                }

                // Check if there are more objects to retrieve
                request.ContinuationToken = response.NextContinuationToken;
            } while (response.IsTruncated);

            // Return a success response
            return new OkObjectResult("Listed objects in the S3 bucket successfully.");
        }
        catch (AmazonS3Exception amazonS3Exception)
        {
            // Handle any S3 exceptions
            log.LogError(amazonS3Exception, "Error encountered when listing objects from S3.");
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
        catch (Exception e)
        {
            // Handle any other exceptions
            log.LogError(e, "Unknown error encountered when listing objects from S3.");
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }
}
