﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Exceptionless.Core.Caching;
using Exceptionless.Core.Messaging;
using Exceptionless.Models;
using MongoDB.Driver;

namespace Exceptionless.Core.Repositories {
    public class MongoRepositoryOwnedByProjectAndStack<T> : MongoRepositoryOwnedByProject<T>, IRepositoryOwnedByStack<T> where T : class, IOwnedByProject, IIdentity, IOwnedByStack, new() {
        public MongoRepositoryOwnedByProjectAndStack(MongoDatabase database, ICacheClient cacheClient = null, IMessagePublisher messagePublisher = null)
            : base(database, cacheClient, messagePublisher) {}

        protected override void BeforeAdd(ICollection<T> documents) {
            if (documents.Any(d => String.IsNullOrEmpty(d.StackId)))
                throw new ArgumentException("StackIds must be set.");

            base.BeforeAdd(documents);
        }

        public virtual ICollection<T> GetByStackId(string stackId, PagingOptions paging = null, bool useCache = false, TimeSpan? expiresIn = null) {
            return Find<T>(new MultiOptions()
                .WithOrganizationId(stackId)
                .WithPaging(paging)
                .WithCacheKey(useCache ? String.Concat("stack:", stackId) : null)
                .WithExpiresIn(expiresIn));
        }

        public async Task RemoveAllByStackIdAsync(string stackId) {
            await Task.Run(() => RemoveAll(new QueryOptions().WithStackId(stackId)));
        }
    }
}
