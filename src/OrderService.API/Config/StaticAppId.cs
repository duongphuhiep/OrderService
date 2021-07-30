using System;

namespace OrderService.API.Config
{
    /// <summary>
    /// A random AppId in case you run many instances of this application
    /// </summary>
    public static class StaticAppId
    {
        public static readonly string Value = Guid.NewGuid().ToString("N").Substring(0, 7);
    }
}
