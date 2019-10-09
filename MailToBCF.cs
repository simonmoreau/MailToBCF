using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using BIM42.Models;
using BIM42.Models.BCF;

using BIM42.Services;
namespace BIM42
{
    public static class MailToBCF
    {
        [FunctionName("MailToBCF")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            IFormCollection formdata = await req.ReadFormAsync();
            String body = await req.ReadAsStringAsync();
            log.LogInformation($"The ReadAsStringAsync contains :{body}");

            log.LogInformation($"The subject is {formdata["subject"]}");
            log.LogInformation($"The sender is {formdata["from"]}");

            string Dkim = string.IsNullOrEmpty(formdata["dkim"]) ? "" : formdata["dkim"].ToString();

            Email email = new Email
            {
                Dkim = string.IsNullOrEmpty(formdata["dkim"]) ? "" : formdata["dkim"].ToString(),
                To = string.IsNullOrEmpty(formdata["to"]) ? "" : formdata["to"].ToString(),
                Html = string.IsNullOrEmpty(formdata["html"]) ? "" : formdata["html"].ToString(),
                From = string.IsNullOrEmpty(formdata["from"]) ? "" : formdata["from"].ToString(),
                Text = string.IsNullOrEmpty(formdata["text"]) ? "" : formdata["text"].ToString(),
                SenderIp = string.IsNullOrEmpty(formdata["sender_ip"]) ? "" : formdata["sender_ip"].ToString(),
                Envelope = string.IsNullOrEmpty(formdata["envelope"]) ? "" : formdata["envelope"].ToString(),
                Attachments = string.IsNullOrEmpty(formdata["attachments"]) ? 0 : Convert.ToInt32(formdata["attachments"]),
                Subject = string.IsNullOrEmpty(formdata["subject"]) ? "" : formdata["subject"].ToString(),
                Charsets = string.IsNullOrEmpty(formdata["charsets"]) ? "" : formdata["charsets"].ToString(),
                Spf = string.IsNullOrEmpty(formdata["spf"]) ? "" : formdata["spf"].ToString(),
            };

            Topic topic = new Topic(email.Subject);
            topic.description = email.Text;

            Regex regex = new Regex(@"<.*?>", RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(email.To);
            string assigned_to = matches[0].Value.Replace("<","").Replace("","");
            topic.assigned_to = assigned_to;

            HttpClient httpClient = new  HttpClient();
            BimsyncBCFClient bcfClient = new BimsyncBCFClient(httpClient);
            await bcfClient.CreateTopicsAsync("12bd226d-2e3c-4bcd-8987-b900d232da89",topic);

            return email != null
                ? (ActionResult)new OkObjectResult($"Hello, {email.Subject}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }

    }
}
