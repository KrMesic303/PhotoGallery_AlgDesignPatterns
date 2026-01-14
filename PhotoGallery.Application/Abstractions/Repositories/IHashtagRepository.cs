using PhotoGallery.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoGallery.Application.Abstractions.Repositories
{
    public interface IHashtagRepository
    {
        Task<Hashtag> GetOrCreateAsync(string normalizedValue, CancellationToken cancellationToken = default);
    }
}
