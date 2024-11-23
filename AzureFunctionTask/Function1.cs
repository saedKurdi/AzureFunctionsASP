using AzureFunctionTask.Services.Abstract;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AzureFunctionTask;

public class Function1
{
    private readonly ILogger<Function1> _logger;
    private readonly IEmailService emailService;

    public Function1(ILogger<Function1> logger, IEmailService emailService)
    {
        _logger = logger;
        this.emailService = emailService;
    }

    [Function("SendMail")]
    public async Task<HttpResponseData> Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "SendMail/{email}")] HttpRequestData req,
    string email,
    FunctionContext context)
    {
        var logger = context.GetLogger("Function1");
        logger.LogInformation("C# HTTP trigger function processed a request.");

        if (string.IsNullOrWhiteSpace(email))
        {
            var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Please provide a valid email address.");
            return badRequestResponse;
        }

        string basePath = Environment.CurrentDirectory;
        string templatePath = Path.Combine(basePath, "EmailTemplates", "EmailTemplate.html");
        string emailBody = File.ReadAllText(templatePath).Replace("[toEmail]", email.Split('@')[0]);

        await emailService.SendEmailAsync(email, "Message From AzureFunction", emailBody);

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteStringAsync($"Welcome to Azure Functions! Email received: {email}");
        return response;
    }

}
