using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Entities
{
    public abstract class Entity
    {
        [Key]
        public Int64 ID { get; set; }

        public Nullable<DateTime> DateIn { get; set; }
        [JsonIgnore]
        public Nullable<DateTime> DateUp { get; set; }
        [JsonIgnore]
        public String UserIn { get; set; }
        [JsonIgnore]
        public String UserUp { get; set; }
        public Nullable<Boolean> IsDeleted { get; set; }


        public Entity()
        {
            UserIn = "";
            DateIn = DateTime.Now.AddMinutes(-2);
        }

    }
}