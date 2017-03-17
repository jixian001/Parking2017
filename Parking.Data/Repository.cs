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
    public class Repository<TEntity> where TEntity:class
    {
        public DbContext _dbContext { get; set; }

        private Repository()
        {
        }

        public Repository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Find 查找实体
        public TEntity Find(int ID)
        {
            return _dbContext.Set<TEntity>().Find(ID);
        }
        public TEntity Find(Expression<Func<TEntity,bool>> where)
        {
            return _dbContext.Set<TEntity>().SingleOrDefault(where);
        }
        #endregion

        #region FindList 查找实体列表
        public IQueryable<TEntity> FindList()
        {
            return _dbContext.Set<TEntity>();
        }

        public IQueryable<TEntity> FindList(Expression<Func<TEntity,bool>> where)
        {
            return _dbContext.Set<TEntity>().Where(where);
        }

        public IQueryable<TEntity> FindList(Expression<Func<TEntity,bool>> where,int number)
        {
            return _dbContext.Set<TEntity>().Where(where).Take(number);
        }

        public IQueryable<TEntity> FindList(Expression<Func<TEntity,bool>> where,OrderParam param,int num)
        {
            var _list = _dbContext.Set<TEntity>().Where(where);
            //待写

            return _list;
        }

        public IQueryable<TEntity> FindList(Expression<Func<TEntity,bool>> where,OrderParam param)
        {
            return FindList(where, param, 0);
        }
        #endregion

        #region 分页 查找实体列表
        public IQueryable<TEntity> FindPageList(int pageSize, int pageIndex, out int totalNumber)
        {
            return FindPageList(pageSize, pageIndex, out totalNumber, (TEntity) => true);
        }

        /// <summary>
        /// 查找分页列表
        /// </summary>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="pageIndex">页码。首页从1开始</param>
        /// <param name="totalNum">总记录数</param>
        /// <param name="where">查询表达式</param>
        /// <returns></returns>
        public IQueryable<TEntity> FindPageList(int pageSize,int pageIndex,out int totalNum,Expression<Func<TEntity,bool>> where)
        {
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }
            if (pageSize < 1)
            {
                pageSize = 10;
            }
            IQueryable<TEntity> _List = _dbContext.Set<TEntity>().Where(where);
            totalNum = _List.Count();
            return _List.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }
        #endregion

        #region 添加实体 Add
        public int Add(TEntity entity)
        {
            return Add(entity, true);
        }

        public int Add(TEntity entity,bool isSave)
        {
            _dbContext.Set<TEntity>().Add(entity);
            return isSave ? _dbContext.SaveChanges() : 0;
        }
        #endregion

        #region 更新实体 update
        public int Update(TEntity entity,bool isSave)
        {
            _dbContext.Set<TEntity>().Attach(entity);
            _dbContext.Entry<TEntity>(entity).State = EntityState.Modified;
            return isSave ? _dbContext.SaveChanges() : 0;
        }

        public int Update(TEntity entity)
        {
            return Update(entity, true);
        }
        #endregion

        #region 删除
        public int Delete(TEntity entity,bool isSave)
        {
            _dbContext.Set<TEntity>().Attach(entity);
            _dbContext.Entry<TEntity>(entity).State = EntityState.Deleted;
            return isSave ? _dbContext.SaveChanges() : 0;
        }

        public int Delete(TEntity entity)
        {
            return Delete(entity, true);
        }

        public int Delete(IEnumerable<TEntity> entities)
        {
            _dbContext.Set<TEntity>().RemoveRange(entities);
            return _dbContext.SaveChanges();
        }

        #endregion

        #region 查询记录数 Count
        public int Count()
        {
            return _dbContext.Set<TEntity>().Count();
        }

        public int Count(Expression<Func<TEntity,bool>> predicate)
        {
            return _dbContext.Set<TEntity>().Count(predicate);
        }
        /// <summary>
        /// 查询记录是否存在
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public bool IsContains(Expression<Func<TEntity,bool>> predicate)
        {
            return Count(predicate) > 0;
        }

        #endregion

        public int Save()
        {
            return _dbContext.SaveChanges();
        }
    }
}
