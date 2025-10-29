﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StajTakipSistemi.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } // "Admin" veya "Student"
    }
}