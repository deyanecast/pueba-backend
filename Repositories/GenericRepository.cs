using Microsoft.EntityFrameworkCore;
using MiBackend.Data;
using MiBackend.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace MiBackend.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;
    private readonly ILogger<GenericRepository<T>> _logger;

    public GenericRepository(ApplicationDbContext context, ILogger<GenericRepository<T>> logger)
    {
        _context = context;
        _dbSet = context.Set<T>();
        _logger = logger;
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.FindAsync(id) != null;
    }

    public async Task<T> CreateAsync(T entity)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Iniciando creación de entidad {EntityType}", typeof(T).Name);
                await _dbSet.AddAsync(entity);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("Entidad {EntityType} creada exitosamente", typeof(T).Name);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear entidad {EntityType}", typeof(T).Name);
                if (transaction.GetDbTransaction().Connection?.State == System.Data.ConnectionState.Open)
                {
                    await transaction.RollbackAsync();
                }
                throw;
            }
        });
    }

    public async Task<T> UpdateAsync(T entity)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Iniciando actualización de entidad {EntityType}", typeof(T).Name);
                _context.Entry(entity).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("Entidad {EntityType} actualizada exitosamente", typeof(T).Name);
                return entity;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (transaction.GetDbTransaction().Connection?.State == System.Data.ConnectionState.Open)
                {
                    await transaction.RollbackAsync();
                }
                throw new InvalidOperationException("El registro ha sido modificado o eliminado por otro usuario.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar entidad {EntityType}", typeof(T).Name);
                if (transaction.GetDbTransaction().Connection?.State == System.Data.ConnectionState.Open)
                {
                    await transaction.RollbackAsync();
                }
                throw;
            }
        });
    }

    public async Task DeleteAsync(int id)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Iniciando eliminación de entidad {EntityType} con ID {Id}", typeof(T).Name, id);
                var entity = await _dbSet.FindAsync(id);
                if (entity == null)
                {
                    throw new KeyNotFoundException($"No se encontró el registro con ID {id}");
                }

                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("Entidad {EntityType} con ID {Id} eliminada exitosamente", typeof(T).Name, id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (transaction.GetDbTransaction().Connection?.State == System.Data.ConnectionState.Open)
                {
                    await transaction.RollbackAsync();
                }
                throw new InvalidOperationException("El registro ha sido modificado o eliminado por otro usuario.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar entidad {EntityType} con ID {Id}", typeof(T).Name, id);
                if (transaction.GetDbTransaction().Connection?.State == System.Data.ConnectionState.Open)
                {
                    await transaction.RollbackAsync();
                }
                throw;
            }
        });
    }
} 