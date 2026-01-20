using Microsoft.EntityFrameworkCore;
using PhotoGallery.Application.Abstractions.Metrics;
using PhotoGallery.Domain.Entities;
using PhotoGallery.Infrastructure.DbContext;

namespace PhotoGallery.Infrastructure.Metrics
{
    public sealed class EfAppMetricStore : IAppMetricStore
    {
        private readonly ApplicationDbContext _context;

        public EfAppMetricStore(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task IncrementAsync(string key, long delta, CancellationToken cancellationToken = default)
        {
            var metric = await _context.AppMetrics.FirstOrDefaultAsync(m => m.Key == key, cancellationToken);

            if (metric == null)
            {
                metric = new AppMetric { Key = key, Value = 0 };
                _context.AppMetrics.Add(metric);
            }

            metric.Value += delta;
            metric.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
