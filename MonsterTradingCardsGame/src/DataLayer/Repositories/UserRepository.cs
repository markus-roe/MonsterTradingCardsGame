﻿using System.Data;
using Npgsql;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Interfaces;

namespace MonsterTradingCardsGame.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository() : base() { }

        protected override void Fill(User user, IDataRecord record)
        {
            user.Username = record.GetString(record.GetOrdinal("Username"));
            user.Password = record.GetString(record.GetOrdinal("password_hash"));
            user.Name = record.IsDBNull(record.GetOrdinal("Name")) ? null : record["Name"].ToString();
            user.Bio = record.IsDBNull(record.GetOrdinal("Bio")) ? null : record["Bio"].ToString();
            user.Image = record.IsDBNull(record.GetOrdinal("Image")) ? null : record["Image"].ToString();
        }

        public override List<User> GetAll()
        {
            var users = new List<User>();
            using (var command = new NpgsqlCommand("SELECT * FROM Users", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var user = new User();
                    Fill(user, reader);
                    users.Add(user);
                }
            }
            return users;
        }

        public User? GetUserByUsername(string username)
        {
            User? user = null;
            using (var command = new NpgsqlCommand("SELECT * FROM Users WHERE Username = @username", connection))
            {
                command.Parameters.AddWithValue("@username", username);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new User();
                        Fill(user, reader);
                    }
                }
            }
            return user;
        }

        public override void Update(User user)
        {
            using (var command = new NpgsqlCommand("UPDATE Users SET Name = @name, Bio = @bio, Image = @image WHERE Username = @username", connection))
            {
                command.Parameters.AddWithValue("@username", user.Username);
                command.Parameters.AddWithValue("@name", user.Name);
                command.Parameters.AddWithValue("@bio", user.Bio);
                command.Parameters.AddWithValue("@image", user.Image);

                command.ExecuteNonQuery();
            }
        }


        public override void Save(User user)
        {
            using (var command = new NpgsqlCommand("INSERT INTO Users (Username, Password_Hash, Name, Bio, Image) VALUES (@username, @password, @name, @bio, @image)", connection))
            {
                command.Parameters.AddWithValue("@username", user.Username);
                command.Parameters.AddWithValue("@password", user.Password);
                command.Parameters.AddWithValue("@name", (object)user.Name ?? DBNull.Value);
                command.Parameters.AddWithValue("@bio", (object)user.Bio ?? DBNull.Value);
                command.Parameters.AddWithValue("@image", (object)user.Image ?? DBNull.Value);

                command.ExecuteNonQuery();
            }
        }

        private bool VerifyPassword(string providedPassword, string storedPassword)
        {
            // Implement your password verification logic here
            // This typically involves hashing the provided password and comparing it with the stored hash
            return providedPassword == storedPassword; // Simplified for example purposes
        }

        private string GenerateToken(User user)
        {
            // Implement your token generation logic here
            // This could be a JWT token or any other format of your choice
            return "generated-token"; // Placeholder
        }



        public override void Delete(User user)
        {
            using (var command = new NpgsqlCommand("DELETE FROM Users WHERE Username = @Username", connection))
            {
                command.Parameters.AddWithValue("@Username", user.Username);

                command.ExecuteNonQuery();
            }
        }

    }
}
