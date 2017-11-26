using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailClient
{
    /// <summary>
    /// 
    /// </summary>
    public class SendEmailsToAgents
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        public static void Main()
        {
            EmailClient emailClient = new EmailClient();
            emailClient.PrepareHitCountsPriorToEmails();
        }
    }
}
