﻿using CronExpressionDescriptor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Models
{
    /// <summary>
    /// 任务参数
    /// </summary>
    public class JobParamer
    {
        /// <summary>
        /// 队列名称
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// 插件名称
        /// </summary>
        public string PluginName { get; set; }

        /// <summary>
        /// 程序集昵称
        /// </summary>
        public string AssemblyTitle { get; set; }

        /// <summary>
        /// 程序集全称
        /// </summary>
        public string AssemblyFullName { get; set; }

        /// <summary>
        /// 程序集名称
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// 任务昵称
        /// </summary>
        public string JobTitle { get; set; }

        /// <summary>
        /// 任务全称
        /// </summary>
        public string JobFullName { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        public string JobName { get; set; }

        /// <summary>
        /// 任务参数
        /// </summary>
        public object[] JobParamers { get; set; }

        /// <summary>
        /// 推出执行间隔
        /// </summary>
        public TimeSpan JobDelay { get; set; }

        /// <summary>
        /// 任务周期，cron 格式
        /// </summary>
        public string JobPeriod { get; set; }

        /// <summary>
        /// 判断 JobPeriod 格式是否符合要求
        /// </summary>
        public bool IsPeriod
        {
            get
            {
                try
                {
                    ExpressionDescriptor.GetDescription(JobPeriod);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }


    }
}