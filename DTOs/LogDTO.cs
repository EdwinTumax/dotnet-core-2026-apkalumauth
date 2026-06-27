using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiKalumAuth.DTOs
{
    public class LogDTO
    {
        public string Name {get;set;}
        public string HostName {get;set;}
        public string ApiKey {get;set;}
        public string Uri {get;set;}
        public string Method {get;set;}
        public int ResponseCode {get;set;}
        public long ResponseTime {get;set;}
        public string ClientIp {get;set;}
        public string Pid {get;set;}
        public int Level {get;set;}
        public string Message {get;set;}
        public string DateTime {get;set;}
        public int Version {set;get;}
    }
}