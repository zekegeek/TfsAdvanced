﻿using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Builds;
using TfsAdvanced.ServiceRequests;

namespace TfsAdvanced.Controllers
{
    [Route("data/BuildDefinitions")]
    public class BuildDefinitionsController : Controller
    {
        private readonly BuildDefinitionRequest buildDefinitionRequest;
        private readonly RequestData requestData;

        public BuildDefinitionsController(BuildDefinitionRequest buildDefinitionRequest, RequestData requestData)
        {
            this.buildDefinitionRequest = buildDefinitionRequest;
            this.requestData = requestData;
        }

        [HttpPost]
        public IActionResult BuildDefinitions([FromBody] List<int> definitionIds)
        {
            if (!definitionIds.Any())
                return NotFound();

            var definitions =
                buildDefinitionRequest.GetAllBuildDefinitions(requestData)
                    .Where(x => definitionIds.Contains(x.id))
                    .ToList();
            var builds = buildDefinitionRequest.LaunchBuild(requestData, definitions);

            buildDefinitionRequest.InvalidateBuildCache(definitions);
            return Ok(builds);
        }
        
        [HttpGet]
        public IList<BuildDefinition> Index()
        {
            return buildDefinitionRequest.GetAllBuildDefinitions(requestData);
        }
    }
}