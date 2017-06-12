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
    public abstract class BaseManager<TEntity> where TEntity:class
    {
        protected Repository<TEntity> _repository;

        public BaseManager(DbContext dbContext)
        {
            _repository = new Repository<TEntity>(dbContext);
        }

        public BaseManager() : 
            this(ContextFactory.CurrentContext())
        {
        }

        /// <summary>
        /// 查找数据列表
        /// </summary>
        public virtual IQueryable<TEntity> FindList()
        {
            return _repository.FindList();
        }

        /// <summary>
        /// 分页数据
        /// </summary>
        /// <param name="paging"></param>
        /// <returns></returns>
        public Page<TEntity> FindPageList(Page<TEntity> paging,OrderParam param)
        {
            int Num = 0;
            paging.ItemLists = _repository.FindPageList(paging.PageSize, paging.PageIndex, out Num, param).ToList();
            paging.TotalNumber = Num;
            return paging;
        }

        /// <summary>
        /// 总记录数
        /// </summary>
        /// <returns></returns>
        public virtual int Count()
        {
            return _repository.Count();
        }

        /// <summary>
        /// 添加
        /// </summary>
        public virtual Response Add(TEntity entity)
        {
            Response _res = new Response();
            if (_repository.Add(entity) > 0)
            {
                _res.Code = 1;
                _res.Message = "添加数据成功！";
                _res.Data = entity;
            }
            else
            {
                _res.Code = 0;
                _res.Message = "添加数据失败！";
            }
            return _res;
        }

        /// <summary>
        /// 添加（isSave）
        /// </summary>
        public virtual Response Add(TEntity entity, bool isSave)
        {
            Response _res = new Response();
            _repository.Add(entity, isSave);
            _res.Code = 1;
            _res.Message = "添加数据成功！";
            _res.Data = entity;
            return _res;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity">实体数据</param>
        /// <returns>成功时属性【Data】为更新后的数据实体</returns>
        public virtual Response Update(TEntity entity)
        {
            Response _response = new Response();
            if (_repository.Update(entity) > 0)
            {
                _response.Code = 1;
                _response.Message = "更新数据成功！";
                _response.Data = entity;
            }
            else
            {
                _response.Code = 0;
                _response.Message = "更新数据失败！";
            }

            return _response;
        }

        public virtual Response Update(TEntity entity,bool isSave)
        {
            Response _res = new Response();
            _repository.Update(entity, isSave);
            _res.Code = 1;
            _res.Message = "添加数据成功！";
            _res.Data = entity;
            return _res;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="ID">主键</param>
        /// <returns>Code：0-删除失败；1-删除陈功；10-记录不存在</returns>
        public virtual Response Delete(int ID)
        {
            Response _response = new Response();
            var _entity =_repository.Find(ID);
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
        public virtual Response Delete(int ID,bool isSave)
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
                _repository.Delete(_entity, isSave);
                _response.Code = 1;
                _response.Message = "删除数据成功！";
            }
            return _response;
        }

        /// <summary>
        /// 提交到数据库，更新
        /// </summary>
        public virtual Response SaveChanges()
        {
            Response resp = new Response();
            int nback= _repository.SaveChanges();
            if (nback > 0)
            {
                resp.Code = 1;
                resp.Message = "提交更新成功";
            }
            return resp;
        }
    }
}
