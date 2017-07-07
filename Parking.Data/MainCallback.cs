using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Data
{
    public delegate void WatchEventHandler<TEntity>(int type, TEntity entity);
   
    public class MainCallback<TEntity>
    {
        public event WatchEventHandler<TEntity> WatchEvent;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">1-增，2-改，3-删</param>
        /// <param name="entity"></param>
        public void OnChange(int type, TEntity entity)
        {
            if (WatchEvent != null)
            {
                WatchEvent(type, entity);
            }
        }

        //定义单例模式,延时初始化
        private static MainCallback<TEntity> _singleton;
        public static MainCallback<TEntity> Instance()
        {
            return _singleton ?? (_singleton = new MainCallback<TEntity>());
        }


    }
}
