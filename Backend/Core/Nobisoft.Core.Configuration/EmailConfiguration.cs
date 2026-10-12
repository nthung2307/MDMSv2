using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable All

#pragma warning disable 8618

#pragma warning disable 1591

namespace Nobisoft.Core.Configuration
{
    public class EmailConfiguration
    {
        public List<Template> Templates { get; set; } = new List<Template>();
        public string Host { get; set; } = "smtp.gmail.com";
        public string Provider { get; set; } = "GMAIL";
        public string Sender { get; set; } = "sample@gmail.com";
        public string DisplayName { get; set; } = "Admin";
        public string Password { get; set; } = "";
        public bool IsBodyHtml { get; set; } = true;
        public bool EnableSsl { get; set; } = true;
        public int Port { get; set; } = 587;
    }
    public class Template
    {
        public string Name { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Body { get; set; } = "";
    }
}