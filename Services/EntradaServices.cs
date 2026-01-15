using GestionEntradaInventario.Data;
using GestionEntradaInventario.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GestionEntradaInventario.Services;

public class EntradasServices(IDbContextFactory<ApplicationDbContext> dbFactory)
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory = dbFactory;

    public async Task<List<Productos>> ListarProductos()
    {
        await using var contexto = await _dbFactory.CreateDbContextAsync();
        return await contexto.Productos
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Entradas>> Listar(Expression<Func<Entradas, bool>> criterio)
    {
        await using var contexto = await _dbFactory.CreateDbContextAsync();
        return await contexto.Entradas
            .Include(e => e.EntradasDetalle)
            .AsNoTracking()
            .Where(criterio)
            .ToListAsync();
    }

    public async Task<Entradas?> Buscar(int entradaId)
    {
        await using var contexto = await _dbFactory.CreateDbContextAsync();
        return await contexto.Entradas
            .Include(e => e.EntradasDetalle)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EntradaId == entradaId);
    }

    public async Task<bool> Insertar(Entradas entrada)
    {
        await using var contexto = await _dbFactory.CreateDbContextAsync();
        await AfectarExistencia(contexto, [.. entrada.EntradasDetalle], TipoOperacion.Suma);
        contexto.Entradas.Add(entrada);
        return await contexto.SaveChangesAsync() > 0;
    }

    private async Task<bool> Modificar(Entradas entrada)
    {
        await using var contexto = await _dbFactory.CreateDbContextAsync();

        var anterior = await contexto.Entradas
            .Include(e => e.EntradasDetalle)
            .FirstOrDefaultAsync(e => e.EntradaId == entrada.EntradaId);

        if (anterior == null) return false;

        await AfectarExistencia(contexto, [.. anterior.EntradasDetalle], TipoOperacion.Resta);

        foreach (var detAnterior in anterior.EntradasDetalle.ToList())
        {
            if (!entrada.EntradasDetalle.Any(d => d.DetalleId == detAnterior.DetalleId))
            {
                contexto.EntradasDetalle.Remove(detAnterior);
            }
        }

        foreach (var detNuevo in entrada.EntradasDetalle)
        {
            if (detNuevo.DetalleId == 0)
            {
                contexto.EntradasDetalle.Add(detNuevo);
            }
            else
            {
                var detExistente = anterior.EntradasDetalle.First(d => d.DetalleId == detNuevo.DetalleId);
                contexto.Entry(detExistente).CurrentValues.SetValues(detNuevo);
            }
        }

        await AfectarExistencia(contexto, [.. entrada.EntradasDetalle], TipoOperacion.Suma);
        contexto.Entry(anterior).CurrentValues.SetValues(entrada);

        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<bool> Eliminar(int entradaId)
    {
        await using var contexto = await _dbFactory.CreateDbContextAsync();

        var entrada = await contexto.Entradas
            .Include(e => e.EntradasDetalle)
            .FirstOrDefaultAsync(e => e.EntradaId == entradaId);

        if (entrada == null) return false;

        await AfectarExistencia(contexto, [.. entrada.EntradasDetalle], TipoOperacion.Resta);
        contexto.Entradas.Remove(entrada);

        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<bool> Existe(int entradaId)
    {
        await using var contexto = await _dbFactory.CreateDbContextAsync();
        return await contexto.Entradas.AnyAsync(e => e.EntradaId == entradaId);
    }

    public async Task<bool> Guardar(Entradas entrada)
    {
        if (!await Existe(entrada.EntradaId))
            return await Insertar(entrada);
        else
            return await Modificar(entrada);
    }

    public async Task<bool> ExisteProducto(int productoId)
    {
        await using var contexto = await _dbFactory.CreateDbContextAsync();
        return await contexto.Productos.AnyAsync(p => p.ProductoId == productoId);
    }

    public async Task<bool> GuardarProducto(Productos producto)
    {
        if (!await ExisteProducto(producto.ProductoId))
        {
            await using var contexto = await _dbFactory.CreateDbContextAsync();
            contexto.Productos.Add(producto);
            return await contexto.SaveChangesAsync() > 0;
        }
        else
        {
            await using var contexto = await _dbFactory.CreateDbContextAsync();
            contexto.Update(producto);
            return await contexto.SaveChangesAsync() > 0;
        }
    }

    public async Task<bool> EliminarProducto(int productoId)
    {
        await using var contexto = await _dbFactory.CreateDbContextAsync();
        var producto = await contexto.Productos
            .FirstOrDefaultAsync(p => p.ProductoId == productoId);

        if (producto == null) return false;

        contexto.Productos.Remove(producto);
        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task AfectarExistencia(ApplicationDbContext contexto, EntradasDetalle[] detalles, TipoOperacion tipo)
    {
        foreach (var det in detalles)
        {
            var producto = await contexto.Productos
                .SingleAsync(p => p.ProductoId == det.ProductoId);

            if (tipo == TipoOperacion.Suma)
                producto.Existencia += det.Cantidad;
            else
                producto.Existencia -= det.Cantidad;
        }
    }

    public enum TipoOperacion { Suma, Resta }
}