csharp veterinariweb\Middleware\SanitizeJsonBodyMiddleware.cs
using System.Text;
namespace veterinariweb.Middleware
{
    public class SanitizeJsonBodyMiddleware
    {
        private readonly RequestDelegate _next;
        public SanitizeJsonBodyMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.ContentType != null
                && context.Request.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase)
                && context.Request.ContentLength > 0
                && context.Request.Body.CanRead)
            {
                context.Request.EnableBuffering();

                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync().ConfigureAwait(false);
                context.Request.Body.Position = 0;

                // Si el JSON ya es válido rápido continue
                if (!string.IsNullOrEmpty(body) && (body.Contains('\n') || body.Contains('\r')))
                {
                    var sanitized = EscapeLineBreaksInsideStrings(body);

                    if (!ReferenceEquals(sanitized, body))
                    {
                        var bytes = Encoding.UTF8.GetBytes(sanitized);
                        context.Request.Body = new MemoryStream(bytes);
                        context.Request.ContentLength = bytes.Length;
                    }
                }
            }

            await _next(context).ConfigureAwait(false);
        }

        private static string EscapeLineBreaksInsideStrings(string input)
        {
            var sb = new StringBuilder(input.Length);
            bool inString = false;
            bool escape = false;

            foreach (var ch in input)
            {
                if (!inString)
                {
                    if (ch == '"')
                    {
                        inString = true;
                        sb.Append(ch);
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }
                else // inString == true
                {
                    if (escape)
                    {
                        sb.Append(ch);
                        escape = false;
                    }
                    else if (ch == '\\')
                    {
                        sb.Append(ch);
                        escape = true;
                    }
                    else if (ch == '"')
                    {
                        inString = false;
                        sb.Append(ch);
                    }
                    else if (ch == '\n')
                    {
                        sb.Append("\\n");
                    }
                    else if (ch == '\r')
                    {
                        sb.Append("\\r");
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }
            }

            return sb.ToString();
        }
    }
}
