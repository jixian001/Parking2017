using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Data;
using Parking.Auxiliary;
using System.Linq.Expressions;

namespace Parking.Core
{
    /// <summary>
    /// 存取时，先从指纹库中找出对应的指纹编号，
    /// 再以这个编号作为凭证，进行存取操作，
    /// 每个指纹都要绑定于相关车主
    /// </summary>
    public class CWFingerPrint
    {
        private FingerPrintManager manager = new FingerPrintManager();

        public CWFingerPrint()
        {
        }

        public Response Add(FingerPrint finger)
        {
            return manager.Add(finger);
        }

        public FingerPrint Find(Expression<Func<FingerPrint, bool>> where)
        {
            return manager.Find(where);
        }

        public List<FingerPrint> FindFingersList()
        {
            return manager.FindList().ToList();
        }

        public List<FingerPrint> FindList(Expression<Func<FingerPrint, bool>> where)
        {
            return manager.FindList(where);
        }

        public Response Delete(int ID)
        {
            return manager.Delete(ID);
        }

    }
}
