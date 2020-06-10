using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tax
{
    class Loading
    {
        public string con = "Data Source=LAPTOP-HB6MUTQ3;Database=Tax;Trusted_Connection=True;MultipleActiveResultSets=true";
        public static object locker = new object();
        public int i = 0;

        public void Load()
        {            
            using (StreamReader r = new StreamReader("file.txt"))
            {                
                while (true)
                {
                    string temp = r.ReadLine();
                    if (temp == null)
                        break;
                           
                    var items = JsonConvert.DeserializeObject<Data>(temp);
                    
                    bool acquiredLock = false;
                    try
                    {
                        Monitor.Enter(locker, ref acquiredLock);                        
                        Insert(items.id, items.fio, items.city, items.email, items.phone, items.date);
                    }
                    finally
                    {
                        if (acquiredLock) Monitor.Exit(locker);
                    }
                }      
            }
        }

        public void Insert(int id, string fio, string city, string email, string phone, string date)
        {            
            string sql = @"
            DECLARE @count INT, @id INT, @fio NVARCHAR(255), @city NVARCHAR(50), @email NVARCHAR(50),  @phone NVARCHAR(12),  @date date,@date_old date

            SET @id = " + id + @"
            SET @fio = '" + fio + @"'
            SET @city = '" + city + @"'
            SET @email = '" + email + @"'
            SET @phone = '" + phone + @"'
            SET @date ='" + date + @"'

            select @count = count(id), @date_old = date 
            from Task
            where id = @id 
            group by date
            if @count > 0
	            BEGIN 
	            IF @date_old < @date
		            update Task set fio = @fio, city = @city, email = @email, phone = @phone, date = @date where id = @id
	            END;
            ELSE
	            INSERT INTO Task (id,fio,city,email,phone,date) VALUES (@id,@fio,@city,@email,@phone,@date)";

            using (SqlConnection connection = new SqlConnection(con))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                command.ExecuteNonQuery();
                Console.WriteLine(Task.CurrentId + " поток, сделал запись в базу" + id + " " + fio);
                File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + @"\log.txt", Task.CurrentId + " поток, сделал запись в базу" + id + " " + fio + "\n", Encoding.UTF8);
            }

        }


    }

    class Data
    {
        public int id { get; set; }
        public string fio { get; set; }
        public string city { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string date { get; set; }
    }

}
