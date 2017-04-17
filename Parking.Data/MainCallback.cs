using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Data
{
    public delegate void WatchEventHandler<TEntity>(TEntity entity);
    public class MainCallback<TEntity>
    {
        public event WatchEventHandler<TEntity> WatchEvent;

        public void OnChange(TEntity entity)
        {
            if (WatchEvent != null)
            {
                WatchEvent(entity);
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
