using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestingSystem.Models
{
    public enum ques_type
    {
        [PgName("POL")]
        POL,

        [PgName("UPR")]
        UPR,

        [PgName("CHL")]
        CHL
    }
}
