using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace CG2021
{
    public class GraphQLComposer : IUserComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.AddNotificationHandler<ContentCopiedNotification, WebsiteRebuilder>();
            builder.AddNotificationHandler<ContentDeletedNotification, WebsiteRebuilder>();
            builder.AddNotificationHandler<ContentMovedNotification, WebsiteRebuilder>();
            builder.AddNotificationHandler<ContentPublishedNotification, WebsiteRebuilder>();
            builder.AddNotificationHandler<ContentUnpublishedNotification, WebsiteRebuilder>();

            builder.Services.AddOptions<Cg2021Options>().Bind(builder.Config.GetSection("CG2021"));
        }

        public class Cg2021Options
        {
            public string FrontEndBuildHook { get; set; }
        }

        private class WebsiteRebuilder :
            INotificationHandler<ContentCopiedNotification>,
            INotificationHandler<ContentDeletedNotification>,
            INotificationHandler<ContentMovedNotification>,
            INotificationHandler<ContentPublishedNotification>,
            INotificationHandler<ContentUnpublishedNotification>
        {
            private readonly ILogger<WebsiteRebuilder> _logger;
            private readonly IOptions<Cg2021Options> _options;

            public WebsiteRebuilder(ILogger<WebsiteRebuilder> logger, IOptions<Cg2021Options> options)
            {
                _logger = logger;
                _options = options;
            }

            public void Handle(ContentCopiedNotification notification) => RebuildWebsite();
            public void Handle(ContentDeletedNotification notification) => RebuildWebsite();
            public void Handle(ContentMovedNotification notification) => RebuildWebsite();
            public void Handle(ContentPublishedNotification notification) => RebuildWebsite();
            public void Handle(ContentUnpublishedNotification notification) => RebuildWebsite();

            private void RebuildWebsite()
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        var result = client.UploadString(_options.Value.FrontEndBuildHook, "");
                        _logger.LogInformation("Rebuild website triggered with a result of " + result);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not rebuild the website");
                }
            }
        }
    }
}
