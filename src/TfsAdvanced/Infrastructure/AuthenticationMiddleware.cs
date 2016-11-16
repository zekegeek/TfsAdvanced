﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TfsAdvanced.Data;

namespace TfsAdvanced.Infrastructure
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.GetUri().LocalPath;

            if (path.StartsWith("/data/Login"))
            {
                await _next.Invoke(context);
                return;
            }

            if (context.Request.GetUri().Host.Contains("localhost"))
            {
                await _next.Invoke(context);
                return;
            }
            
            byte[] value;
            if (context.Session.TryGetValue("AuthToken", out value))
            {
                var token = JsonConvert.DeserializeObject<AuthenticationToken>(ASCIIEncoding.ASCII.GetString(value));
                if (token?.access_token == null)
                {
                    context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
                    await context.Response.WriteAsync("Unable to read session.  User is not authenticated.");
                    return;
                }
                if (DateTime.Now > token.expiredTime)
                {
                    context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsync("Token is expired.  User needs to reauthenticate.");
                    return;
                }
                await _next.Invoke(context);
                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await context.Response.WriteAsync("Session does not have AuthToken set.  User is not authenticated.");
        }
    }
}