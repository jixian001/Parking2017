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
        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name="alarmLst"></param>
        /// <returns></returns>
        public async Task<Response> UpdateAlarmListAsync(List<Alarm> alarmLst)
        {
            for (int i = 0; i < alarmLst.Count; i++)
            {
                Alarm ar = alarmLst[i];
                Alarm newAlarm =await FindAsync(d => d.Warehouse == ar.Warehouse && d.DeviceCode == ar.DeviceCode && d.Address == ar.Address);
                if (newAlarm != null)
                {
                    newAlarm.Value = ar.Value;
                    Update(newAlarm);
                }
            }
            Response resp = new Response();
            resp.Code = 1;
            resp.Message = "提交成功";

            return resp;
        }        
        
    }
}
