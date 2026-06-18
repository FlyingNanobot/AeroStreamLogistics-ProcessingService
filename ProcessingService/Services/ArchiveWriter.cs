using Amazon.S3;
using Amazon.S3.Model;

namespace ProcessingService.Services
{
    public class ArchiveWriter
    {
        private readonly IAmazonS3 _s3;
        private readonly string _bucket;

        public ArchiveWriter(IAmazonS3 s3, IConfiguration config)
        {
            _s3 = s3;
            _bucket = config["S3:Bucket"];
        }

        public async Task AppendRawAsync(string rawJson)
        {
            var key = $"raw/{DateTime.UtcNow:yyyy/MM/dd/HH}/{Guid.NewGuid()}.json";

            await _s3.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _bucket,
                Key = key,
                ContentBody = rawJson
            });
        }
    }
}
