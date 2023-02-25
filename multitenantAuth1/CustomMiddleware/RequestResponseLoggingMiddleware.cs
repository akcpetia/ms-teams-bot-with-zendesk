using Microsoft.Net.Http.Headers;
using multitenantAuth1.Model;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace multitenantAuth1.CustomMiddleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
      
        public RequestResponseLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
            
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = context.Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            if (!String.IsNullOrEmpty(token))
            {
                var jsonToken = handler.ReadToken(token);
                var tokenS = handler.ReadToken(token) as JwtSecurityToken;
                var id = tokenS.Claims.First(claim => claim.Type == "oid").Value;

                var tid = tokenS.Claims.First(claim => claim.Type == "tid").Value;

                CommonMethods.GetSession().tenantId = tid;
                CommonMethods.GetSession().InsertedBy = id;
                CommonMethods.GetSession().UpdatedBy = id;
            }
            else
            {
                CommonMethods.GetSession().InsertedBy = "Inserted By";
                CommonMethods.GetSession().UpdatedBy = "Update By";
            }

            String requestBody;
            String responseBody = "";
            DateTime requestTime = DateTime.UtcNow;
            Stopwatch stopwatch;

            context.Request.EnableBuffering();

            using (StreamReader reader = new StreamReader(context.Request.Body,
                                                          encoding: Encoding.UTF8,
                                                          detectEncodingFromByteOrderMarks: false,
                                                          leaveOpen: true))
            {
                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }
           
            Stream originalResponseStream = context.Response.Body;
            using (MemoryStream responseStream = new MemoryStream())
            {
                context.Response.Body = responseStream;

                stopwatch = Stopwatch.StartNew();
                await _next(context);
                stopwatch.Stop();

                context.Response.Body.Seek(0, SeekOrigin.Begin);
                responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin);

                await responseStream.CopyToAsync(originalResponseStream);
            }
            
        }
    }
}
