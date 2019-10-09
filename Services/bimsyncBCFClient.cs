using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using BIM42.Models.BCF;
using Newtonsoft.Json;

namespace BIM42.Services
{
    class BimsyncBCFClient : IBCFClient
    {
        private HttpClient _client;

        public BimsyncBCFClient(HttpClient client)
        {
            _client = client;
            _client.BaseAddress = new Uri("https://bcf.bimsync.com/bcf/beta/");
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "jQg3g5knpDCjair");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public async Task<List<IssueBoard>> GetIssueBoardsAsync(string bimsync_project_id)
        {
            string path = String.Format("projects?bimsync_project_id={0}", bimsync_project_id);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, path);

            List<IssueBoard> issueBoards = await SendRequest<List<IssueBoard>>(request, new CancellationToken());
            return issueBoards;
        }

        public async Task<List<Topic>> GetTopicsAsync(string project_id)
        {
            string path = String.Format("projects/{0}/topics", project_id);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, path);

            List<Topic> topics = await SendRequest<List<Topic>>(request, new CancellationToken());
            return topics;
        }

        public async Task<Topic> CreateTopicsAsync(string project_id, Topic topic)
        {
            string path = String.Format("projects/{0}/topics", project_id);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, path);
            string jsonTopic = JsonConvert.SerializeObject(topic,
                            Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });
            request.Content = new StringContent(jsonTopic.ToString(), Encoding.UTF8, "application/json");

            Topic newTopic = await SendRequest<Topic>(request, new CancellationToken());
            return newTopic;
        }

        public async Task<List<Comment>> GetComments(string project_id, string topic_guid)
        {
            string path = String.Format("projects/{0}/topics/{1}/comments", project_id, topic_guid);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, path);

            List<Comment> comments = await SendRequest<List<Comment>>(request, new CancellationToken());
            return comments;
        }

        public async Task<List<Viewpoint>> GetViewpoints(string project_id, string topic_guid)
        {
            string path = String.Format("projects/{0}/topics/{1}/viewpoints", project_id, topic_guid);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, path);

            List<Viewpoint> viewpoints = await SendRequest<List<Viewpoint>>(request, new CancellationToken());
            return viewpoints;
        }

        public async Task<Viewpoint> GetViewpoint(string project_id, string topic_guid, string viewpoint_guid)
        {
            string path = String.Format("projects/{0}/topics/{1}/viewpoints/{2}", project_id, topic_guid, viewpoint_guid);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, path);

            Viewpoint viewpoint = await SendRequest<Viewpoint>(request, new CancellationToken());
            return viewpoint;
        }

        public async Task<List<ExtensionStatus>> GetIssueBoardExtensionStatusesAsync(string project_id)
        {
            string path = String.Format("projects/{0}/extensions/statuses", project_id);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, path);

            List<ExtensionStatus> extensionStatuses = await SendRequest<List<ExtensionStatus>>(request, new CancellationToken());
            return extensionStatuses;
        }

        public async Task<List<ExtensionType>> GetIssueBoardExtensionTypesAsync(string project_id)
        {
            string path = String.Format("projects/{0}/extensions/types", project_id);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, path);

            List<ExtensionType> extensionTypes = await SendRequest<List<ExtensionType>>(request, new CancellationToken());
            return extensionTypes;
        }

        private async Task<T> SendRequest<T>(HttpRequestMessage request, CancellationToken cancellationToken)
        {

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            using (HttpResponseMessage response = await _client.SendAsync(request,
              HttpCompletionOption.ResponseHeadersRead,
              cancellationToken))
            {
                Stream stream = await response.Content.ReadAsStreamAsync();
                response.EnsureSuccessStatusCode();
                using (var streamReader = new StreamReader(stream))
                {
                    using (JsonTextReader jsonTextReader = new JsonTextReader(streamReader))
                    {
                        var jsonSerializer = new JsonSerializer();
                        return jsonSerializer.Deserialize<T>(jsonTextReader);
                    }
                }
            }
        }
    }
}