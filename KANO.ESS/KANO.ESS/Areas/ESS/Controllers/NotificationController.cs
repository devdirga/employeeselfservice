﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using KANO.Core.Lib;
using KANO.Core.Lib.Extension;
using KANO.Core.Model;
using KANO.Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using RestSharp;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KANO.ESS.Areas.ESS.Controllers
{
    [Area("ESS")]
    public class NotificationController : Controller
    {

        private IConfiguration Configuration;
        private IUserSession Session;
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly string api = "api/notification/";
        private readonly String ApiNotification = "api/notification/";
        private readonly String BearerAuth = "Bearer ";

        public NotificationController(IConfiguration config, IUserSession session)
        {
            Configuration = config;
            Session = session;
        }

        public async Task<IActionResult> Get([FromBody] FetchParam param)
        {
            param.Limit = (param.Limit <= 0)? 10: param.Limit;
            param.Offset = (param.Offset < 0)? 0: param.Offset;
            param.Filter = (string.IsNullOrEmpty(param.Filter))? "all": param.Filter;

            var employeeID = Session.Id();
            var client = new Client(Configuration);
            var request = new Request($"api/notification/get/{employeeID}/{param.Limit}/{param.Offset}/{param.Filter}", Method.GET);
            var response = client.Execute(request);           
            if (response.StatusCode != HttpStatusCode.OK &&  string.IsNullOrWhiteSpace(response.Content)) return ApiResult<object>.Error(response.StatusCode, response.StatusDescription);

            var result = JsonConvert.DeserializeObject<ApiResult<List<Notification>>.Result>(response.Content);
            return new ApiResult<List<Notification>>(result);
        }

        public async Task<IActionResult> Subscribe([FromBody] NotificationSubscription param)
        {
            if (string.IsNullOrWhiteSpace(param.Receiver)) {
                param.Receiver = Session.Id();
                param.ReceiverName = Session.DisplayName();
            }

            var client = new Client(Configuration);
            var request = new Request($"api/notification/subscribe", Method.POST);
            request.AddJsonParameter(param);

            var response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK && string.IsNullOrWhiteSpace(response.Content)) return ApiResult<object>.Error(response.StatusCode, response.StatusDescription);

            var result = JsonConvert.DeserializeObject<ApiResult<NotificationSubscription>.Result>(response.Content);
            return new ApiResult<NotificationSubscription>(result);
        }

        public async Task<IActionResult> Unubscribe([FromBody] NotificationSubscription param)
        {
            if (string.IsNullOrWhiteSpace(param.Receiver))
            {
                param.Receiver = Session.Id();
                param.ReceiverName = Session.DisplayName();
            }

            var client = new Client(Configuration);
            var request = new Request($"api/notification/unsubscribe", Method.POST);
            request.AddJsonParameter(param);

            var response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK && string.IsNullOrWhiteSpace(response.Content)) return ApiResult<object>.Error(response.StatusCode, response.StatusDescription);

            var result = JsonConvert.DeserializeObject<ApiResult<NotificationSubscription>.Result>(response.Content);
            return new ApiResult<NotificationSubscription>(result);
        }

        public async Task<IActionResult> SubscriptionCheck([FromBody] NotificationSubscription param)
        {

            var receiver = Session.Id();
            var client = new Client(Configuration);
            var request = new Request($"api/notification/subscription/check", Method.POST);
            request.AddJsonParameter(param);

            var response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK && string.IsNullOrWhiteSpace(response.Content)) return ApiResult<object>.Error(response.StatusCode, response.StatusDescription);

            var result = JsonConvert.DeserializeObject<ApiResult<bool>.Result>(response.Content);
            return new ApiResult<bool>(result);
        }

        /**
         * Function for ESS Mobile because ESS Mobile need Authentication except signin
         * Every function must authorize with token from signin function
         * This is for security
         */

        [HttpPost]
        [AllowAnonymous]
        public IActionResult MGet([FromBody] FetchParam param)
        {
            string bearerAuth = BearerAuth;
            if (Request.Headers.TryGetValue("Authorization", out StringValues authToken)) { bearerAuth = authToken; }
            param.Limit = (param.Limit <= 0) ? 10 : param.Limit;
            param.Offset = (param.Offset < 0) ? 0 : param.Offset;
            param.Filter = (string.IsNullOrEmpty(param.Filter)) ? "all" : param.Filter;
            var response = new Client(Configuration).Execute(new Request($"{ApiNotification}mget", Method.POST, param, "Authorization", bearerAuth));
            if (response.StatusCode != HttpStatusCode.OK && string.IsNullOrWhiteSpace(response.Content))
            {
                return ApiResult<object>.Error(response.StatusCode, response.StatusDescription);
            }
            return new ApiResult<List<Notification>>(JsonConvert.DeserializeObject<ApiResult<List<Notification>>.Result>(response.Content));
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult MSetRead([FromBody] Notification p)
        {
            string bearerAuth = BearerAuth;
            if (Request.Headers.TryGetValue("Authorization", out StringValues authToken)) { bearerAuth = authToken; }
            var response = new Client(Configuration).Execute(new Request($"{ApiNotification}msetread", Method.POST, p, "Authorization", bearerAuth));
            if (response.StatusCode != HttpStatusCode.OK && string.IsNullOrWhiteSpace(response.Content))
            {
                return ApiResult<Notification>.Error(response.StatusCode, response.StatusDescription);
            }
            return new ApiResult<Notification>(JsonConvert.DeserializeObject<ApiResult<Notification>.Result>(response.Content));
        }

    }
}