﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;
using System.Linq.Expressions;

namespace Parking.Data
{
    public class FaultLogManager:BaseManager<FaultLog>
    {

        public List<FaultLog> FindList(Expression<Func<FaultLog, bool>> where)
        {
            return _repository.FindList(where).ToList();
        }
    }
}
