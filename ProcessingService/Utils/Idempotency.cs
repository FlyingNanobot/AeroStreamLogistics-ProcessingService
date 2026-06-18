using System.Security.Cryptography;
using System.Text;

namespace ProcessingService.Utils
{
    /// <summary>
    /// Utility helpers to support idempotency-related operations.
    ///
    /// Theory: Many distributed systems create an idempotency key for messages so that retries
    /// can be detected and duplicates ignored. A common approach is to compute a stable hash
    /// over canonicalized message content and use that as the key in a deduplication store.
    /// </summary>
    internal static class Idempotency
    {
        /// <summary>
        /// Compute a SHA256 hex digest for the provided input string. This can be used as a
        /// deterministic message identifier for idempotency checks.
        /// </summary>
        /// <param name="input">The canonicalized message payload.</param>
        /// <returns>Hex-encoded SHA256 digest.</returns>
        public static string ComputeHash(string input)
        {
            if (input == null) return string.Empty;

            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(bytes);
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
