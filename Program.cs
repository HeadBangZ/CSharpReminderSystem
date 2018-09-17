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
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(reader.GetOrdinal("ID"));

                        int affectedRows = UpdateState(connection, id, 0, 1);

                        if (affectedRows > 0) // only execute if instance was allowed to update to state 1
                        {
                            SendMail(reader["EMAIL"].ToString(), "Remember the appointment");
                            UpdateState(connection, id, 1, 2);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No valid s0s2 transitions found.");
                }
                reader.Close();
            }
        }

        static int UpdateState(SqlConnection connection, int id, int stateOld, int stateNew)
        {
            Console.Write("Updating state for id:"+id+" "+stateOld+"->"+stateNew);
            // SQL Query update state
            using (var updCmds0s1 = new SqlCommand("update clientReminderSystemTable set state = "+stateNew+" where state = "+stateOld+" and id =" + id, connection))
            {
                int affectedRows = updCmds0s1.ExecuteNonQuery();
                Console.WriteLine(" (Affected rows:" + affectedRows+")");

                return affectedRows;
            }
        }

        static void SendMail(string email, string message)
        {
            Console.WriteLine("Send email to:" + email + " : " + message);
        }

        static void SendSMS(string msisdn, string message)
        {
            Console.WriteLine("Send SMS to:" + msisdn + " : " + message);
        }
    }
}
