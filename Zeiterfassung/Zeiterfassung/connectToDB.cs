using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Linq;

namespace Zeiterfassung
{
    class connectToDB
    {
        List<Auszubildender> Auszubildende = new List<Auszubildender>();
        List<zeit> Zeiten = new List<zeit>();

        MySqlConnection conn = new MySqlConnection(@"server=localhost;userid=root;password=;database=zeiterfassung;Convert Zero Datetime=True;Allow Zero Datetime=True");
        MySqlCommand command;
        MySqlDataReader reader;

        public connectToDB()
        {
            try
            {
                conn.Open();

                command = new MySqlCommand("SELECT * FROM auszubildende", conn);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Auszubildende.Add(new Auszubildender
                    {
                        TAG = reader[0].ToString(),
                        Vorname = reader[1].ToString(),
                        Nachname = reader[2].ToString()
                    });
                }
                reader.Close();

                command = new MySqlCommand("SELECT * FROM zeiten", conn);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Zeiten.Add(new zeit
                    {
                        TAG = reader[0].ToString(),
                        eingetragen_um = Convert.ToDateTime(reader[1]),
                        ausgetragen_um = Convert.ToDateTime(reader[2])
                    });
                }
                reader.Close();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: {0}", ex);
            }
        }

        public void updateDB(string tag, DateTime zeiten)
        {
            var check = from zeit in Zeiten where zeit.eingetragen_um == zeiten select tag;//doesnt work need to search for same day

            try
            {
                if (check != null)
                {
                    command = new MySqlCommand("INSERT INTO zeiten (TAG, eingetragen_um) VALUES (@tag, @eingetragen)", conn);
                    command.Parameters.AddWithValue("@tag", tag);
                    command.Parameters.AddWithValue("@eingetragen", zeiten);
                }
                else
                {
                    command = new MySqlCommand("INSERT INTO zeiten (TAG, ausgetragen_um) VALUES (@tag, @ausgetragen)", conn);
                    command.Parameters.AddWithValue("@tag", tag);
                    command.Parameters.AddWithValue("@ausgetragen", zeiten);
                }

                command.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: {0}", ex);
            }
            
        }

        private void SearchForSameDate(DateTime zeiten)
        {

        }
    }
}
