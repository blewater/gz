﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using gzDAL.Models;
using gzDAL.Repos.Interfaces;
using SendGrid;
using RazorEngine;
using RazorEngine.Templating;

namespace gzDAL.Repos
{
    public class SendGridEmailService : IEmailService
    {
        public SendGridEmailService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SendEmail(string templateCode, MailAddress fromAddress, string toAddress, Dictionary<string,object> data)
        {
            if (String.IsNullOrEmpty(templateCode)) throw new ArgumentException("templateCode");
            if (fromAddress==null) throw new ArgumentException("fromAddress");
            if (String.IsNullOrEmpty(toAddress)) throw new ArgumentException("toAddress");

            var template = _dbContext.EmailTemplates.SingleOrDefault(x => x.Code == templateCode);
            if (template == null)
                throw new KeyNotFoundException(String.Format("Template with code: '{0}' not found.", templateCode));

            var viewBag = data==null ? new DynamicViewBag(): new DynamicViewBag(data);
            var compiledSubject = Engine.Razor.RunCompile(template.Subject, String.Format("{0}_Subject", template.Code), null, viewBag);
            var compiledBody = Engine.Razor.RunCompile(template.Body, String.Format("{0}_Body", template.Code), null, viewBag);

            await SendEmail(fromAddress, toAddress, compiledSubject, compiledBody);
        }

        public async Task SendEmail(MailAddress fromAddress, string toAddress, string subject, string body)
        {
            try
            {
                var message = new SendGridMessage
                              {
                                      From = fromAddress,
                                      Subject = subject,
                                      Html = body
                              };
                message.AddTo(toAddress);

                await SendMessage(message);
            }
            catch (Exception exception)
            {
                throw;
            }
        }
        
        private async Task SendMessage(SendGridMessage message)
        {
            await new Web("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").DeliverAsync(message);
        }

        private readonly ApplicationDbContext _dbContext;
    }
}