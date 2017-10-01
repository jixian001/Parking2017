using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Parking.Auxiliary;
using System.Data;
using System.Transactions;

namespace Parking.Data
{
    public class Repository<TEntity> where TEntity : class
    {
        public DbContext _dbContext { get; set; }

        public Repository()
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

        public TEntity Find(Expression<Func<TEntity, bool>> where)
        {
            return _dbContext.Set<TEntity>().Where<TEntity>(where).FirstOrDefault();
        }

        public async Task<TEntity> FindAsync(int ID)
        {
            return await _dbContext.Set<TEntity>().FindAsync(ID);
        }

        public async Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> where)
        {
            return await _dbContext.Set<TEntity>().Where<TEntity>(where).FirstOrDefaultAsync();
        }
        #endregion

        #region FindList 查找实体列表
        public IQueryable<TEntity> FindList()
        {
            return _dbContext.Set<TEntity>();
        }

        public IQueryable<TEntity> FindList(Expression<Func<TEntity, bool>> where)
        {
            return _dbContext.Set<TEntity>().Where(where).AsQueryable();
        }

        public async Task<List<TEntity>> FindListAsync()
        {
            return await _dbContext.Set<TEntity>().Where(te => true).ToListAsync();
        }

        public async Task<List<TEntity>> FindListAsync(Expression<Func<TEntity, bool>> where)
        {
            return await _dbContext.Set<TEntity>().Where(where).ToListAsync();
        }

        public IQueryable<TEntity> FindList(Expression<Func<TEntity, bool>> where, int number)
        {
            return _dbContext.Set<TEntity>().Where(where).Take(number).AsQueryable();
        }

        /// <summary>
        /// 查找实体列表
        /// </summary>
        /// <param name="where">查询Lambda表达式</param>
        /// <param name="orderParams">排序参数</param>
        /// <param name="number">获取的记录数量【0-不启用】</param>
        /// <returns></returns>
        public IQueryable<TEntity> FindList(Expression<Func<TEntity, bool>> where, OrderParam[] orderParams, int number)
        {
            var _list = _dbContext.Set<TEntity>().Where(where);
            var _orderParames = Expression.Parameter(typeof(TEntity), "o");
            if (orderParams != null && orderParams.Length > 0)
            {
                bool _isFirstParam = true;
                for (int i = 0; i < orderParams.Length; i++)
                {
                    //根据属性名获取属性
                    var _property = typeof(TEntity).GetProperty(orderParams[i].PropertyName);
                    //创建一个访问属性的表达式
                    var _propertyAccess = Expression.MakeMemberAccess(_orderParames, _property);
                    var _orderByExp = Expression.Lambda(_propertyAccess, _orderParames);
                    string _orderName;
                    if (_isFirstParam)
                    {
                        _orderName = orderParams[i].Method == OrderMethod.Asc ? "OrderBy" : "OrderByDescending";
                        _isFirstParam = false;
                    }
                    else
                    {
                        _orderName = orderParams[i].Method == OrderMethod.Desc ? "ThenBy" : "ThenByDescending";
                    }

                    MethodCallExpression resultExp = Expression.Call(typeof(Queryable), _orderName, new Type[] { typeof(TEntity), _property.PropertyType }, _list.Expression, Expression.Quote(_orderByExp));
                    _list = _list.Provider.CreateQuery<TEntity>(resultExp);
                }
            }
            if (number > 0)
            {
                _list = _list.Take(number);
            }
            return _list;
        }

        public IQueryable<TEntity> FindList(Expression<Func<TEntity, bool>> where, OrderParam orderParam, int number)
        {
            OrderParam[] _orderParams = null;
            if (orderParam != null)
            {
                _orderParams = new OrderParam[] { orderParam };
            }
            return FindList(where, _orderParams, number);
        }

        public IQueryable<TEntity> FindList(Expression<Func<TEntity, bool>> where, OrderParam param)
        {
            return FindList(where, param, 0);
        }


        #endregion

        #region 分页 查找实体列表
        public IQueryable<TEntity> FindPageList(int pageSize, int pageIndex, out int totalNumber, OrderParam param)
        {
            return FindPageList(pageSize, pageIndex, out totalNumber, (TEntity) => true, new OrderParam[] { param });
        }

        /// <summary>
        /// 查找分页列表
        /// </summary>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="pageIndex">页码。首页从1开始</param>
        /// <param name="totalNum">总记录数</param>
        /// <param name="where">查询表达式</param>
        /// <returns></returns>
        public IQueryable<TEntity> FindPageList(int pageSize, int pageIndex, out int totalNum, Expression<Func<TEntity, bool>> where, OrderParam[] orderParams)
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
            var _orderParames = Expression.Parameter(typeof(TEntity), "o");
            if (orderParams != null && orderParams.Length > 0)
            {
                for (int i = 0; i < orderParams.Length; i++)
                {
                    //根据属性名获取属性
                    var _property = typeof(TEntity).GetProperty(orderParams[i].PropertyName);
                    //创建一个访问属性的表达式
                    var _propertyAccess = Expression.MakeMemberAccess(_orderParames, _property);
                    var _orderByExp = Expression.Lambda(_propertyAccess, _orderParames);
                    string _orderName = orderParams[i].Method == OrderMethod.Asc ? "OrderBy" : "OrderByDescending";
                    MethodCallExpression resultExp = Expression.Call(typeof(Queryable), _orderName, new Type[] { typeof(TEntity), _property.PropertyType }, _List.Expression, Expression.Quote(_orderByExp));
                    _List = _List.Provider.CreateQuery<TEntity>(resultExp);
                }
            }
            totalNum = _List.Count();
            return _List.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        #endregion

        #region 添加实体 Add
        public int Add(TEntity entity)
        {           
            using (var scope = new TransactionScope())
            {
                _dbContext.Entry<TEntity>(entity).State = EntityState.Added;
                int nback = _dbContext.SaveChanges();
                scope.Complete();
                return nback;
            }
        }

        /// <summary>
        /// 批量添加
        /// </summary>
        /// <param name="enities"></param>
        /// <returns></returns>
        public int Add(IEnumerable<TEntity> enities)
        {
            using (var scope = new TransactionScope())
            {
                _dbContext.Set<TEntity>().AddRange(enities);
                int nback = _dbContext.SaveChanges();
                scope.Complete();

                return nback;
            }
        }

        public async Task<int> AddAsync(TEntity entity)
        {
            //using (var scope = new TransactionScope())
            //{
                _dbContext.Entry<TEntity>(entity).State = EntityState.Added;
                int nback = await _dbContext.SaveChangesAsync();
                //scope.Complete();
                return nback;
            //}
        }

        #endregion

        #region 更新实体 update
       
        public int Update(TEntity entity)
        {            
            using (var scope = new TransactionScope())
            {
                _dbContext.Set<TEntity>().Attach(entity);
                _dbContext.Entry<TEntity>(entity).State = EntityState.Modified;
                scope.Complete();
            }
            return _dbContext.SaveChanges();
        }

        public async Task<int> UpdateAsync(TEntity entity)
        {
            _dbContext.Set<TEntity>().Attach(entity);
            _dbContext.Entry<TEntity>(entity).State = EntityState.Modified;

            return await _dbContext.SaveChangesAsync();
        }

        #endregion

        #region 删除
        public int Delete(TEntity entity)
        {            
            using (var scope = new TransactionScope())
            {
                _dbContext.Set<TEntity>().Attach(entity);
                _dbContext.Entry<TEntity>(entity).State = EntityState.Deleted;
               
                int nback= _dbContext.SaveChanges();
                scope.Complete();

                return nback;
            }          
        }

        public async Task<int> DeleteAsync(TEntity entity)
        {
            //using (var scope = new TransactionScope())
            //{
                _dbContext.Set<TEntity>().Attach(entity);
                _dbContext.Entry<TEntity>(entity).State = EntityState.Deleted;

                int nback =await _dbContext.SaveChangesAsync();
                //scope.Complete();

                return nback;
            //}
        }

        public int Delete(IEnumerable<TEntity> entities)
        {
            using (var scope = new TransactionScope())
            {
                _dbContext.Set<TEntity>().RemoveRange(entities);
                scope.Complete();
                int nback= _dbContext.SaveChanges();
                return nback;
            }
           
        }

        public async Task<int> DeleteAsync(IEnumerable<TEntity> entities)
        {
            using (var scope = new TransactionScope())
            {
                _dbContext.Set<TEntity>().RemoveRange(entities);
                scope.Complete();
                int nback =await _dbContext.SaveChangesAsync();
                return nback;
            }
        }

        #endregion

        #region 查询记录数 Count
        public int Count()
        {
            return _dbContext.Set<TEntity>().Count();
        }

        public int Count(Expression<Func<TEntity, bool>> predicate)
        {
            return _dbContext.Set<TEntity>().Count(predicate);
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbContext.Set<TEntity>().CountAsync(predicate);
        }

        /// <summary>
        /// 查询记录是否存在
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public bool IsContains(Expression<Func<TEntity, bool>> predicate)
        {
            return Count(predicate) > 0;
        }
        #endregion
        
    }
}
