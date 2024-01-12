﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    [Table(name: "Role")]
    public class Role : Entity
    {
        public String Name { get; set; }
        public ICollection<RoleDetail> RoleDetails { get; set; }
    }

    [Table(name: "RoleDetail")]
    public class RoleDetail : Entity
    {
        public Int64 RoleID { get; set; }
        public Int64 ModuleID { get; set; }

        public Boolean IsCreate { get; set; }
        public Boolean IsRead { get; set; }
        public Boolean IsUpdate { get; set; }
        public Boolean IsDelete { get; set; }
    }

}