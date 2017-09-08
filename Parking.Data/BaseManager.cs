using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Parking.Auxiliary;

namespace Parking.Data
{
    public abstract class BaseManager<TEntity> where TEntity : class
    {

        /// <summary>
        /// 数据仓储类
        /// </summary>
        protected Repository<TEntity> _repository;

        public BaseManager() :
         this(ContextFactory.CurrentContext())
        {
        }

        public BaseManager(DbContext dbContext)
        {
            _repository = new Repository<TEntity>(dbContext);
        }

        /// <summary>
        /// 查找实体
        /// </summary>
        /// <param name="ID">主键</param>
        /// <returns>实体</returns>
        public TEntity Find(int ID)
        {
            return _repository.Find(ID);
        }

        public async Task<TEntity> FindAsync(int ID)
        {
            return await _repository.FindAsync(ID);
        }

        public TEntity Find(Expression<Func<TEntity, bool>> where)
        {
            return _repository.Find(where);
        }

        public async Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> where)
        {
            return await _repository.FindAsync(where);
        }

        /// <summary>
        /// 查找数据列表
        /// </summary>
        public List<TEntity> FindList()
        {
            return _repository.FindList().ToList<TEntity>();
        }

        public async Task<List<TEntity>> FindListAsync()
        {
            return await _repository.FindListAsync();
        }

        public List<TEntity> FindList(Expression<Func<TEntity, bool>> where)
        {
            return _repository.FindList(where).ToList<TEntity>();
        }

        public async Task<List<TEntity>> FindListAsync(Expression<Func<TEntity, bool>> where)
        {
            return await _repository.FindListAsync(where);
        }

        /// <summary>
        /// 分页数据
        /// </summary>
        /// <param name="paging"></param>
        /// <returns></returns>
        public Page<TEntity> FindPageList(Page<TEntity> paging, OrderParam param)
        {
            int Num = 0;
            IQueryable<TEntity> iqueryLst = _repository.FindPageList(paging.PageSize, paging.PageIndex, out Num, param);
            List<TEntity> checkLst = new List<TEntity>();
            foreach (TEntity en in iqueryLst)
            {
                checkLst.Add(en);
            }
            paging.ItemLists = checkLst;
            paging.TotalNumber = Num;
            return paging;

        }

       
        /// <summary>
        /// 添加
        /// </summary>
        public Response Add(TEntity entity)
        {
            Response _res = new Response();
            int nback= _repository.Add(entity);
            if (nback > 0)
            {
                _res.Code = 1;
                _res.Message = "添加数据成功！";
            }
            return _res;
        }

        public async Task<Response> AddAsync(TEntity entity)
        {
            Response _res = new Response();
            int nback = await _repository.AddAsync(entity);
            if (nback > 0)
            {
                _res.Code = 1;
                _res.Message = "添加数据成功！";
            }
            return _res;
        }

        /// <summary>
        /// 批量添加
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public Response Add(IEnumerable<TEntity> entities)
        {
            Response _res = new Response();
            int nback = _repository.Add(entities);
            if (nback > 0)
            {
                _res.Code = 1;
                _res.Message = "添加数据成功！";
            }
            return _res;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity">实体数据</param>
        /// <returns>成功时属性【Data】为更新后的数据实体</returns>
        public Response Update(TEntity entity)
        {
            Response _res = new Response();
            int nback= _repository.Update(entity);
            if (nback > 0)
            {
                _res.Code = 1;
                _res.Message = "添加数据成功！";
                _res.Data = entity;
            }
            return _res;
        }

        public async Task<Response> UpdateAsync(TEntity entity)
        {
            Response _res = new Response();
            int nback =await _repository.UpdateAsync(entity);
            if (nback > 0)
            {
                _res.Code = 1;
                _res.Message = "添加数据成功！";
                _res.Data = entity;
            }
            return _res;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="ID">主键</param>
        /// <returns>Code：0-删除失败；1-删除陈功；10-记录不存在</returns>
        public Response Delete(int ID)
        {
            Response _response = new Response();
            var _entity = _repository.Find(ID);
            if (_entity == null)
            {
                _response.Code = 10;
                _response.Message = "ID为" + ID + "的记录不存在！";
            }
            else
            {
                if (_repository.Delete(_entity) > 0)
                {
                    _response.Code = 1;
                    _response.Message = "删除数据成功！";
                                                            
                }
                else
                {
                    _response.Code = 0;
                    _response.Message = "删除数据失败！";
                }
            }
            return _response;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="ID">主键</param>
        /// <returns>Code：0-删除失败；1-删除陈功；10-记录不存在</returns>
        public Response Delete(TEntity _entity)
        {
            Response _response = new Response();
            if (_repository.Delete(_entity) > 0)
            {
                _response.Code = 1;
                _response.Message = "删除数据成功！";

            }
            else
            {
                _response.Code = 0;
                _response.Message = "删除数据失败！";
            }
            return _response;
        }

        public async Task<Response> DeleteAsync(TEntity _entity)
        {
            Response _response = new Response();
            int nback = await _repository.DeleteAsync(_entity);
            if (nback > 0)
            {
                _response.Code = 1;
                _response.Message = "删除数据成功！";
            }
            else
            {
                _response.Code = 0;
                _response.Message = "删除数据失败！";
            }
            return _response;
        }
    }
}
