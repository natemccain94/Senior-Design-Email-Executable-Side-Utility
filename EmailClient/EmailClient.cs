using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EmailClient
{
    /// <summary>
    /// 
    /// </summary>
    public class EmailClient
    {
        // Construct an EmailClient object.
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailClient"/> class.
        /// </summary>
        public EmailClient()
        {
            emailConnectionToDatabase = new SQL_Connection();

            client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("realestateutilityuahteamseven@gmail.com", "r01lT!de"),
                EnableSsl = true
            };
        }

        // Prepare the hitcounts prior to emailing the agents.
        // This is the only function that needs to be called by an outside class.
        /// <summary>
        /// Prepares the hit counts prior to emails.
        /// </summary>
        public void PrepareHitCountsPriorToEmails()
        {
            try
            {
                // Connect to the database.
                emailConnectionToDatabase.openConnection();
                // Update all lifetime hit counts.
                emailConnectionToDatabase.UpdateLifetimeHitCount();
                // Prepare the emails!!!
                GatherAgentsToEmail();
            }
            catch (Exception e) { } // Do nothing.
        }

        // Gather all agents from the database.
        /// <summary>
        /// Gathers the agents to email.
        /// </summary>
        private void GatherAgentsToEmail()
        {
            var AgentTable = new DataTable();
            AgentTable = emailConnectionToDatabase.GetAllAgents();
            TrimEmailList(AgentTable);
        }

        // Get only agents with listings in the database.
        /// <summary>
        /// Trims the email list.
        /// </summary>
        /// <param name="AgentTable">The agent table.</param>
        private void TrimEmailList(DataTable AgentTable)
        {
            DataTable AgentsWithProperties = new DataTable();
            AgentsWithProperties.Columns.Add("agent_id");
            AgentsWithProperties.Columns["agent_id"].DataType = typeof(int);
            AgentsWithProperties.Columns.Add("agent_Fname");
            AgentsWithProperties.Columns.Add("agent_Lname");
            AgentsWithProperties.Columns.Add("agent_number");
            AgentsWithProperties.Columns.Add("agent_email");
            AgentsWithProperties.Columns.Add("agency_id");
            foreach (DataRow row in AgentTable.Rows)
            {
                if (emailConnectionToDatabase.GetTotalNumberOfListingsFromAgent((int)row[0]) >= 1)
                {
                    //AgentsWithProperties.ImportRow(row);
                    AgentsWithProperties.Rows.Add(row.ItemArray);
                }
            }
            PrepareSpamEmails(AgentsWithProperties);
        }

        // Get all the required information for each email to be sent.
        /// <summary>
        /// Prepares the spam emails.
        /// </summary>
        /// <param name="AgentTable">The agent table.</param>
        private void PrepareSpamEmails(DataTable AgentTable)
        {
            DataTable PropertiesTableForSpecificAgent = new DataTable();
            PropertiesTableForSpecificAgent.Columns.Add("listing_street");
            PropertiesTableForSpecificAgent.Columns.Add("listing_city");
            PropertiesTableForSpecificAgent.Columns.Add("listing_state");
            PropertiesTableForSpecificAgent.Columns.Add("listing_zip");
            PropertiesTableForSpecificAgent.Columns.Add("listing_hitCount");
            PropertiesTableForSpecificAgent.Columns["listing_hitCount"].DataType = typeof(int);
            PropertiesTableForSpecificAgent.Columns.Add("listingLifetimeHitCount");
            PropertiesTableForSpecificAgent.Columns["listingLifetimeHitCount"].DataType = typeof(int);
            int helper;
            foreach (DataRow row in AgentTable.Rows)
            {
                helper = (int) row[0];
                PropertiesTableForSpecificAgent = emailConnectionToDatabase.GetAllListingsForEmailToSpecificAgent((int)row[0]);
                EmailAnAgent(row, PropertiesTableForSpecificAgent);
            }
            ResetDailyHitCounts();
        }

        // Prepare and send an email to an agent.
        /// <summary>
        /// Emails an agent.
        /// </summary>
        /// <param name="AgentToEmail">The agent to email.</param>
        /// <param name="Properties">The properties.</param>
        private void EmailAnAgent(DataRow AgentToEmail, DataTable Properties)
        {
            String firstLine = String.Concat("Hello ", (String)AgentToEmail[1],
                " ", (String)AgentToEmail[2], ". Here is a list of how your \n",
                "properties are performing: \n \n");

            String agentEmailAddress = (String)AgentToEmail[4];

            String propertyListHeader =
                "Address                    Daily Hit Count                    Lifetime Hit Count \n\n";

            String row;

            List<String> listingStats = new List<String>();

            int daily, lifetime, requiredNumberOfWhitespaces;

            foreach (DataRow propertyRow in Properties.Rows)
            {
                daily = (int)propertyRow[4];
                lifetime = (int) propertyRow[5];

                row = String.Concat((String) propertyRow[0]);
                requiredNumberOfWhitespaces = 41 - row.Length;

                while (requiredNumberOfWhitespaces > 0)
                {
                    row = String.Concat(row, " ");
                    requiredNumberOfWhitespaces--;
                }

                row = String.Concat(row, daily.ToString());
                //requiredNumberOfWhitespaces = propertyListHeader.Length - 9;
                requiredNumberOfWhitespaces = 43;

                while (requiredNumberOfWhitespaces > 0)
                {
                    row = String.Concat(row, " ");
                    requiredNumberOfWhitespaces--;
                }

                row = String.Concat(row, lifetime.ToString(), "\n");

                // Add the first line of the property info to the list.
                listingStats.Add(row);
                // Now begin adding the rest of the address to the listings thing.
                row = String.Concat(propertyRow[1], ", ", propertyRow[2], " ",
                    propertyRow[3], "\n\n");
                // Add the rest to the list.
                listingStats.Add(row);
            }

            // Put the email body together.
            String emailBody = String.Concat(firstLine, propertyListHeader);
            foreach (String item in listingStats)
            {
                emailBody = String.Concat(emailBody, item);
            }
            emailBody = String.Concat(emailBody, "End of Email. \n");

            client.Send("realestateutilityuahteamseven@gmail.com", agentEmailAddress, "Your Property Statistics", emailBody);

        }

        // Reset the daily hit counts for all listings.
        /// <summary>
        /// Resets the daily hit counts.
        /// </summary>
        private void ResetDailyHitCounts()
        {
            // Call to function to reset all daily hit counts.
            emailConnectionToDatabase.ResetDailyHitCount();
            // Close the connection.
            FinishedSendingEmails();
        }

        // Close the connection to the server.
        /// <summary>
        /// Finisheds the sending emails.
        /// </summary>
        private void FinishedSendingEmails()
        {
            emailConnectionToDatabase.closeConnection();
        }

        /// <summary>
        /// The client
        /// </summary>
        private SmtpClient client;
        /// <summary>
        /// The email connection to database
        /// </summary>
        private SQL_Connection emailConnectionToDatabase;
    }
}
