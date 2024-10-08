using System;
using System.Collections.Generic;

namespace Ranger.Common
{
    public class Breadcrumb
    {
        public Breadcrumb(string deviceId, string externalUserId, LngLat position, DateTime recordedAt, DateTime acceptedAt, IEnumerable<KeyValuePair<string, string>> metadata, double accuracy = 0, long id = 0)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                throw new ArgumentException($"{nameof(deviceId)} was null or whitespace");
            }
            if (position is null)
            {
                throw new ArgumentException($"{nameof(position)} was null or whitespace");
            }
            if (recordedAt.Equals(DateTime.MinValue) || recordedAt.Equals(DateTime.MaxValue))
            {
                throw new ArgumentException($"{nameof(recordedAt)} was not in a valid range");
            }
            if (accuracy < 0)
            {
                throw new ArgumentException($"{nameof(accuracy)} must be greater than or equal to 0");
            }
            if (id < 0)
            {
                throw new ArgumentException($"{nameof(id)} must be greater than or equal to 0");
            }

            this.Id = id;
            this.DeviceId = deviceId;
            this.ExternalUserId = externalUserId;
            this.Position = position ?? throw new ArgumentNullException(nameof(position));
            this.RecordedAt = recordedAt;
            this.AcceptedAt = acceptedAt;
            this.Accuracy = accuracy;
            this.Metadata = metadata;
        }

        public long Id { get; }
        public string DeviceId { get; }
        public string ExternalUserId { get; }
        public LngLat Position { get; }
        public double Accuracy { get; }
        public DateTime RecordedAt { get; }
        public DateTime AcceptedAt { get; }
        public IEnumerable<KeyValuePair<string, string>> Metadata { get; }
    }
}