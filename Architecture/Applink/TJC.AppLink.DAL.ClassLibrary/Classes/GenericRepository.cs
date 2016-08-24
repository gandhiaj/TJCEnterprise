using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TJC.AppLink.DAL.ClassLibrary.Interfaces;

namespace TJC.AppLink.DAL.ClassLibrary
{
    public class GenericRepository : IGenericRepository
    {
        /// <summary>
        /// Concatenate user name with application name in connection string
        /// to know who is accessing the system
        /// </summary>
        /// <param name="cntxt"></param>
        /// <param name="user"></param>
        public GenericRepository(DbContext cntxt, string user)
        {
            Context = cntxt;
            if (!Context.Database.Connection.ConnectionString.Contains(user))
                Context.Database.Connection.ConnectionString = _entities.Database.Connection.ConnectionString + user;
        }
        private DbContext _entities;
        protected DbContext Context
        {
            get { return _entities; }
            private set { _entities = value; }
        }

        public GenericRepository(DbContext dbContext)
        {
            this.Context = dbContext;
            //this.dbSet = context.Set<TEntity>();
        }

        public virtual IQueryable<T> GetAll<T>() where T : class
        {

            IQueryable<T> query = Context.Set<T>();
            return query;
        }

        public virtual T GetSingle<T>(Func<T, bool> predicate) where T : class
        {
            return Context.Set<T>().FirstOrDefault(predicate);
        }

        public IQueryable<T> FindBy<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class
        {
            IQueryable<T> query = Context.Set<T>().Where(predicate);
            return query;
        }

        /// <summary>
        /// Execute stored proc without parameters
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="spName"></param>
        /// <returns>Enumerable results</returns>
        public IEnumerable<T> ExecuteStoredProc<T>(string spName)
        {

            return Context.Database.SqlQuery<T>(spName);
        }


        /// <summary>
        /// Execute stored proc with parameters
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="spName"></param>
        /// <param name="parameters"></param>
        /// <returns>Enumerable results</returns>
        public IEnumerable<T> ExecuteStoredProc<T>(string spName, params object[] parameters)
        {
            //return Context.Database.SqlQuery<T>(spName, GetParams(spName, parameters));             
            return Context.Database.SqlQuery<T>(spName, parameters);
        }

        /// <summary>
        /// Execute sql command
        /// </summary>
        /// <param name="sql"></param>
        public virtual void ExecuteSqlCommand(string sql)
        {
            Context.Database.ExecuteSqlCommand(sql);
        }

        public virtual void Add<T>(T entity) where T : class
        {
            Context.Set<T>().Add(entity);
        }

        public virtual void Delete<T>(T entity) where T : class
        {
            Context.Set<T>().Remove(entity);
        }

        public virtual void Edit<T>(T entity) where T : class
        {
            Context.Entry(entity).State = System.Data.Entity.EntityState.Modified;
        }

        public virtual void Save()
        {
            Context.SaveChanges();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Context != null)
                {
                    Context.Dispose();
                    _entities = null;
                }
            }
        }

    }
}
