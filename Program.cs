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
            int counter = 0;
            SqlConnection conn = new SqlConnection(@"data source = DESKTOP-DJ7RAJ3; integrated security = true; database = emailReminderSystemDB");
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            string s0s2 = "select * from clientAppointment where state = 0 and '" + DateTime.Now.AddDays(3).ToString("yyyy/MM/dd") + "' > appointmentTime";
            bool reminderSystem = true;

            while(reminderSystem)
            {
                string s0s1 = "update clientAppointment set state = 1 where state = 0";
                string s1s2 = "update clientAppointment set state = 2 where state = 1";
            //    string s2s4 = "select * from clientAppointment where state = 0 and '" + DateTime.Now.AddDays(1).ToString("yyyy/MM/dd") + "' > appointmentTime";
            //    string s2s3 = "update clientAppointment set state = 3 where state = 2";
            //    string s3s4 = "update clientAppointment set state = 4 where state = 3";
                try
                {
                    conn.Open();
                    cmd = new SqlCommand(s0s2, conn);
                    reader = cmd.ExecuteReader();
                    
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            counter++;
                            Console.WriteLine(reader[0] + " - " + reader[1] + " " + reader[2] + " " + reader[3] + " " + reader[4]);
                            
                            Console.WriteLine("Email sent to: " + reader[1]);
                            
                            Console.WriteLine(reader[0] + " - " + reader[1] + " " + reader[2] + " " + reader[3] + " " + reader[4] + "\n");
                        }

                    }
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                }
                finally
                {
                    conn.Close();
                }

                Console.WriteLine("====================================");
                Console.WriteLine("Number of clients :" + counter);
                Console.WriteLine("====================================");

                counter = 0;

                Thread.Sleep(5000);
            }
        }
    }
}
