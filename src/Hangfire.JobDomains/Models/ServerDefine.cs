﻿using Hangfire.JobDomains.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Models
{

    public class ServerDefine
    {

        public ServerDefine(string name) { Name = name; }

        public ServerDefine(string name, string path, string description = "")
        {
            Name = name;
            Description = description;
            PlugPath = path;
        }

        public string Name { get; private set; } 

        public string Description { get; set; }

        public string PlugPath { get; set; }

        public List<DomainDefine> Domains { get; set; }

        public List<QueueDefine> Queues { get; set; }

    }


}