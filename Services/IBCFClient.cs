using BIM42.Models.BCF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIM42.Services
{
    public interface IBCFClient
    {
        Task<List<IssueBoard>> GetIssueBoardsAsync(string bimsync_project_id);
        Task<List<Topic>> GetTopicsAsync(string project_id);
        Task<List<Comment>> GetComments(string project_id, string topic_guid);
        Task<List<Viewpoint>> GetViewpoints(string project_id, string topic_guid);
        Task<Viewpoint> GetViewpoint(string project_id, string topic_guid, string viewpoint_guid);
        Task<List<ExtensionStatus>> GetIssueBoardExtensionStatusesAsync(string project_id);
        Task<List<ExtensionType>> GetIssueBoardExtensionTypesAsync(string project_id);
    }
}