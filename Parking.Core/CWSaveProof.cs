using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Auxiliary;
using Parking.Data;
using System.Linq.Expressions;

namespace Parking.Core
{
    public class CWSaveProof
    {
        private SaveProofManager manager = new SaveProofManager();

        public List<SaveCertificate> FindList()
        {
            return manager.FindList();
        }

        public async Task<List<SaveCertificate>> FindListAsync()
        {
            return await manager.FindListAsync();
        }

        public List<SaveCertificate> FindList(Expression<Func<SaveCertificate, bool>> where)
        {
            return manager.FindList(where);
        }

        public async Task<List<SaveCertificate>> FindListAsync(Expression<Func<SaveCertificate, bool>> where)
        {
            return await manager.FindListAsync(where);
        }

        public SaveCertificate Find(int id)
        {
            return manager.Find(id);
        }

        public async Task<SaveCertificate> FindAsync(int id)
        {
            return await manager.FindAsync(id);
        }

        public SaveCertificate Find(Expression<Func<SaveCertificate, bool>> where)
        {
            return manager.Find(where);
        }

        public async Task<SaveCertificate> FindAsync(Expression<Func<SaveCertificate, bool>> where)
        {
            return await manager.FindAsync(where);
        }

        public Response Add(SaveCertificate entity)
        {
            return manager.Add(entity);
        }

        public Response Update(SaveCertificate entity)
        {
            return manager.Update(entity);
        }

        public Response Delete(int id)
        {
            return manager.Delete(id);
        }

        public Response Delete(SaveCertificate proof)
        {
            return manager.Delete(proof);
        }

        /// <summary>
        /// 获取自增的编号
        /// </summary>
        /// <returns></returns>
        public int GetMaxSNO()
        {
            Int32 max = 22000;
            List<SaveCertificate> printList = FindList();
            if (printList.Count > 0)
            {
                 int select = printList.Select(m => m.SNO).Max();
                if (select > 22000)
                {
                    max = select;
                }
            }
            if (max > 32000)
            {
                max = 21900;
            }
            return ++max;
        }
    }
}
