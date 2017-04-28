using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Data;

namespace Parking.Auxiliary
{
    public static class DataTableExtensions
    {
        public static DataTable ToDataTable<T>(this IEnumerable<T> list)
        {
            //创建属性的集合  
            List<PropertyInfo> plist = new List<PropertyInfo>();
            //获得反射的入口  
            Type type = typeof(T);
            DataTable dt = new DataTable();
            //把所有的public属性加入到集合 并添加DataTable的列 
            Array.ForEach<PropertyInfo>(type.GetProperties(), p => {
                plist.Add(p);
                dt.Columns.Add(p.Name, p.PropertyType);
            });

            foreach (var item in list)
            {
                //创建一个DataRow实例  
                DataRow row = dt.NewRow();
                //赋值
                plist.ForEach(p => row[p.Name] = p.GetValue(item, null));
                dt.Rows.Add(row);
            }

            return dt;
        }
    }
}
