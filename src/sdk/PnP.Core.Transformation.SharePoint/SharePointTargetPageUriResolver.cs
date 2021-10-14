﻿using Microsoft.Extensions.Options;
using PnP.Core.Services;
using PnP.Core.Transformation.Services.Core;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.SharePoint
{
    /// <summary>
    /// Implementation of <see cref="ITargetPageUriResolver"/> for SharePoint sources
    /// </summary>
    public class SharePointTargetPageUriResolver : ITargetPageUriResolver
    {
        private readonly PageTransformationOptions defaultPageTransformationOptions;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="pageTransformationOptions">Page transformation options</param>
        /// <exception cref="ArgumentNullException"></exception>
        public SharePointTargetPageUriResolver(IOptions<PageTransformationOptions> pageTransformationOptions)
        {
            this.defaultPageTransformationOptions = pageTransformationOptions?.Value ?? throw new ArgumentNullException(nameof(pageTransformationOptions));
        }

        /// <summary>
        /// Resolves the SharePoint target uri for the specified source item
        /// </summary>
        /// <param name="sourceItem">The source item</param>
        /// <param name="targetContext">The target context</param>
        /// <param name="token">The cancellation token, if any</param>
        /// <returns>The resolved URI</returns>
        public Task<Uri> ResolveAsync(ISourceItem sourceItem, PnPContext targetContext, CancellationToken token = default)
        {
            if (sourceItem == null) throw new ArgumentNullException(nameof(sourceItem));
            if (targetContext == null) throw new ArgumentNullException(nameof(targetContext));
            if (!(sourceItem is SharePointSourceItem spItem))
                throw new ArgumentException($"Only source item of type {typeof(SharePointSourceItem)} is supported", nameof(sourceItem));

            var itemSiteLocalUri = new StringBuilder();
            foreach (var s in spItem.Id.Uri.Segments.Skip(3).Take(spItem.Id.Uri.Segments.Length - 4))
            {
                itemSiteLocalUri.Append(s);
            }

            itemSiteLocalUri.Append(this.defaultPageTransformationOptions.TargetPagePrefix);
            itemSiteLocalUri.Append(spItem.Id.Uri.Segments[spItem.Id.Uri.Segments.Length - 1]);

            // In case the source is a classi publishing portal
            // we also need to replace "pages" with "sitepages"
            var uri = new Uri((targetContext.Uri + "/" + 
                itemSiteLocalUri.ToString())
                .Replace("/pages/", "/sitepages/", StringComparison.InvariantCultureIgnoreCase));

            return Task.FromResult(uri);
        }
    }
}
