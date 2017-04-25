using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;
using System.Linq.Expressions;

namespace Parking.Data
{
    public class AlarmManager:BaseManager<Alarm>
    {
        public List<Alarm> FindList(Expression<Func<Alarm, bool>> where)
        {
            return _repository.FindList(where).ToList();
        }

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name="alarmLst"></param>
        /// <returns></returns>
        public Response UpdateAlarmList(List<Alarm> alarmLst)
        {
            foreach(Alarm ar in alarmLst)
            {
                _repository.Update(ar, false);
            }
            int count= _repository.Save();
            Response resp = new Response() {
                Code=1,
                Message="批量更新成功！Count-"+count
            };
            return resp;
        }

    }
}
