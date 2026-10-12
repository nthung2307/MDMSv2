using System.Diagnostics.CodeAnalysis;

// ReSharper disable All

#pragma warning disable 8618

#pragma warning disable 1591

namespace Nobisoft.Core.Configuration
{
    /// <summary>
    /// To manage how Files can be managed, uploaded and served in the system
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class FileConfiguration
    {
        /// <summary>
        /// Max file size in Mb
        /// </summary>
        public int MaxSize { get; set; } = 4;

        public List<string> AllowedExtensions { get; set; } = new List<string>();
        public List<string> AllowedContentTypes { get; set; } = new List<string>();
    }
}