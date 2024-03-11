using Graphapi.Utils.Models;
using LanguageExt;
using LanguageExt.Common;
using Polly;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphapi.Utils.Services;
public class GraphApiGroupsRetriever
{
    private readonly ResiliencePipeline _resiliencePipeline;

    public GraphApiGroupsRetriever(
        ResiliencePipelineProvider<string> pipelineProvider)
    {
        _resiliencePipeline = pipelineProvider.GetPipeline(Constants.GroupsDownloadResiliencePipeline);
    }

    public EitherAsync<Error, IEnumerable<GraphApiGroup>> RetrieveAsync(
        AuthenticationOptions authenticationOptions) => 
        EitherAsync<Error, IEnumerable<GraphApiGroup>>.Bottom;
}
