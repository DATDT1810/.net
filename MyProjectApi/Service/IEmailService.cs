using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyProjectApi.Helpter;

namespace MyProjectApi.Service
{
    public interface IEmailService
    {
        Task SendEmailAsync(Mailrequest mailrequest);
    }
}