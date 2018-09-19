using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Net;
using System.Net.Mime;
using System.Net.Mail;

namespace ReminderSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                SqlConnection conn = new SqlConnection(@"data source = DESKTOP-DJ7RAJ3; integrated security = true; database = emailReminderSystemDB; MultipleActiveResultSets = True");
                CheckRemindersS0S2(conn);
                Thread.Sleep(5000);
            }
        }

        static void CheckRemindersS0S2(SqlConnection connection)
        {
            using (connection)
            {
                string s0s2 = "select * from clientReminderSystemTable where state = 0 and '" + DateTime.Now.AddDays(3).ToString("yyyy/MM/dd") + "' >= appointmentTime";
                SqlCommand command = new SqlCommand(s0s2, connection);
                string s2s4 = "select * from clientReminderSystemTable where state = 2 and '" + DateTime.Now.AddDays(1).ToString("yyyy/MM/dd") + "' >= appointmentTime";
                SqlCommand commandTwo = new SqlCommand(s2s4, connection);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                SqlDataReader readerTwo = commandTwo.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(reader.GetOrdinal("ID"));

                        int affectedRows = UpdateState(connection, id, 0, 1);

                        if (affectedRows > 0) // only execute if instance was allowed to update to state 1
                        {
                            SendMail(reader["EMAIL"].ToString(), "Dentist Reminder" ,"Remember the appointment, at " + reader["APPOINTMENTTIME"] + ", " + reader["NAME"] + "\nBest regards \nDentist");
                            UpdateState(connection, id, 1, 2);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No valid transitions found.");
                    CheckRemindersS2S4(readerTwo, connection);
                }
                reader.Close();
                connection.Close();
            }
        }

        static void CheckRemindersS2S4(SqlDataReader readerTwo, SqlConnection connection)
        {
            if (readerTwo.HasRows)
            {
                while (readerTwo.Read())
                {
                    int id = readerTwo.GetInt32(readerTwo.GetOrdinal("ID"));

                    int affectedRows = UpdateState(connection, id, 2, 3);

                    if (affectedRows > 0)
                    {
                        SendSMS(readerTwo["MOBILE"].ToString(), "Remember the appointment, at " + readerTwo["APPOINTMENTTIME"] + ", " + readerTwo["NAME"] + "\nBest regards \nDentist", "Dentist");
                        UpdateState(connection, id, 3, 4);
                    }
                }
            }
            else
            {
                Console.WriteLine("No valid transitions found.");
            }
        }


        static int UpdateState(SqlConnection connection, int id, int stateOld, int stateNew)
        {
            Console.Write("Updating state for id: " + id + " " + stateOld + " -> " + stateNew);
            // SQL Query update state
            using (var updCmdState = new SqlCommand("update clientReminderSystemTable set state = "+stateNew+" where state = "+stateOld+" and id =" + id, connection))
            {
                int affectedRows = updCmdState.ExecuteNonQuery();
                Console.WriteLine(" (Affected rows:" + affectedRows+")");

                return affectedRows;
            }
        }

        static void SendMail(string email, string subject, string body)
        {
            string from = "mail";
            string pwd = "xxxxxxxx";

            MailMessage mail = new MailMessage(from, email);
            mail.Subject = subject;
            mail.Body = body;

            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.EnableSsl = true;
            client.Timeout = 30000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(from, pwd);
            client.Host = "smtp.gmail.com";
            client.Send(mail);

            Console.WriteLine("Send email to: " + email + " : " + subject + " : " + body);
        }

        public static string SendSMS(string msisdn, string message, string from)
        {
            string username = "user for cpsms";
            string apikey = "apikey";
            string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + apikey));
            // Netværksfejl noget galt med url
            string url = "https://api.cpsms.dk/v2/simplesend/" + "45" + msisdn + "/" + System.Uri.EscapeDataString(message) + "/" + System.Uri.EscapeDataString(from);

            using (WebClient client = new WebClient())
            {
                client.Headers["AUTHORIZATION"] = "Basic " + auth;
                return client.DownloadString(url);
            }

            //Console.WriteLine("Send SMS to: " + msisdn + " : " + message);
        }
    }
}
