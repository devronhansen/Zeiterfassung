using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Zeiterfassung
{
    class connectToDB
    {
        List<Auszubildender> Auszubildende = new List<Auszubildender>();

        public List<Auszubildender> readFromDB()
        {
            try
            {
                MySqlConnection conn = new MySqlConnection(@"server=localhost;userid=root;password=;database=zeiterfassung");
                conn.Open();

                MySqlCommand command = new MySqlCommand("SELECT * FROM auszubildende", conn);
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Auszubildende.Add(new Auszubildender
                    {
                        TAG = reader[0].ToString(),
                        Vorname = reader[1].ToString(),
                        Nachname = reader[2].ToString()
                    });
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: {0}", ex);
            }

            return Auszubildende;
        }

        public void insertIntoDB()
        {

        }

        public void updateDB()
        {

        }
    }
}
