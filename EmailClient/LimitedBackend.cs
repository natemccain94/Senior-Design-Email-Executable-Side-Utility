using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace EmailClient
{
    /// <summary>
    /// 
    /// </summary>
    public class SQL_Connection
    {
        // Empty constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="SQL_Connection"/> class.
        /// </summary>
        public SQL_Connection() { }

        // Open the connection to the server.
        /// <summary>
        /// Opens the connection.
        /// </summary>
        public void openConnection()
        {
            connection = new SqlConnection();
            connection.ConnectionString =
                "Data Source=DESKTOP-08QT6IE;Initial Catalog=Housing;Integrated Security=True";
                //"Data Source=DESKTOP-QM2SFGD;Initial Catalog=Housing;Integrated Security=True";
            connection.Open();
        }

        // Close the connection to the server.
        /// <summary>
        /// Closes the connection.
        /// </summary>
        public void closeConnection()
        {
            connection.Close();
        }

        // Reset the daily hit count for all listings.
        /// <summary>
        /// Resets the daily hit count.
        /// </summary>
        public void ResetDailyHitCount()
        {
            using (var command = new SqlCommand())
            {
                try
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "UPDATE dbo.listing SET listing_hitCount = 0";

                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        // Update the lifetime hit count of all listings.
        /// <summary>
        /// Updates the lifetime hit count.
        /// </summary>
        public void UpdateLifetimeHitCount()
        {
            using (var command = new SqlCommand())
            {
                try
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "UPDATE dbo.listing SET listingLifetimeHitCount = listing_hitCount + listingLifetimeHitCount";

                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        // Get the total number of listings for a specified agent.
        /// <summary>
        /// Gets the total number of listings from agent.
        /// </summary>
        /// <param name="agent_id">The agent identifier.</param>
        /// <returns></returns>
        public int GetTotalNumberOfListingsFromAgent(int agent_id)
        {
            int result = 0;
            using (var command = new SqlCommand())
            {
                try
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Concat("SELECT COUNT(*) FROM dbo.listing WHERE agent_id = ",
                        agent_id);
                    result = Convert.ToInt32(command.ExecuteScalar());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    result = -1;
                }
            }
            return result;
        }

        // Get all listings for a specified agent.
        /// <summary>
        /// Gets all listings for email to specific agent.
        /// </summary>
        /// <param name="agent_id">The agent identifier.</param>
        /// <returns></returns>
        public DataTable GetAllListingsForEmailToSpecificAgent(int agent_id)
        {
            var table = new DataTable();
            using (var command = new SqlCommand())
            {
                try
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = String.Concat("SELECT listing_street, listing_city, listing_state, ",
                        "listing_zip, listing_hitCount, listingLifetimeHitCount FROM dbo.listing WHERE agent_id = ",
                        "@agent_id");

                    command.Parameters.Add("@agent_id", SqlDbType.Int);
                    command.Parameters["@agent_id"].Value = agent_id;

                    table.Columns.Add("listing_street");
                    table.Columns.Add("listing_city");
                    table.Columns.Add("listing_state");
                    table.Columns.Add("listing_zip");
                    table.Columns.Add("listing_hitCount");
                    table.Columns["listing_hitCount"].DataType = typeof(int);
                    table.Columns.Add("listingLifetimeHitCount");
                    table.Columns["listingLifetimeHitCount"].DataType = typeof(int);

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(table);

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            return table;
        }

        // Get all agents from the agent database.
        /// <summary>
        /// Gets all agents.
        /// </summary>
        /// <returns></returns>
        public DataTable GetAllAgents()
        {
            var table = new DataTable();
            using (var command = new SqlCommand())
            {
                try
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText =
                        string.Concat("SELECT agent_Fname, agent_Lname, agent_number, agent_email, agent_id, ",
                            "agency_id FROM dbo.agent");

                    table.Columns.Add("agent_id");
                    table.Columns["agent_id"].DataType = typeof(int);
                    table.Columns.Add("agent_Fname");
                    table.Columns.Add("agent_Lname");
                    table.Columns.Add("agent_number");
                    table.Columns.Add("agent_email");
                    table.Columns.Add("agency_id");


                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(table);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            return table;
        }

        /// <summary>
        /// The connection
        /// </summary>
        private SqlConnection connection;
        /// <summary>
        /// The command
        /// </summary>
        private SqlCommand command;
    }
}
