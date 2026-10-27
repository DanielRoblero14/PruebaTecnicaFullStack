using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PruebaDanielRoblero.Data;
using PruebaDanielRoblero.Models;

namespace PruebaDanielRoblero.Controllers;

public class EmpleadosController : Controller
{
    private readonly ApplicationDbContext _context;

    public EmpleadosController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var empleados = await _context.Empleados.AsNoTracking()
            .Include(e => e.Departamento).OrderBy(e => e.Id).ToListAsync();
        return View(empleados);
    }

    // modulo Crear
    public async Task<IActionResult> Create()
    {
        await CargarDepartamentosAsync();
        return View(new Empleado());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombres,Apellidos,FechaNacimiento,DepartamentoId")] Empleado empleado)
    {
        ModelState.Remove(nameof(Empleado.Codigo));
        await ValidarEmpleadoAsync(empleado);

        if (ModelState.IsValid)
        {
            var numero = await _context.Database
                .SqlQueryRaw<long>("SELECT NEXT VALUE FOR dbo.SecuenciaCodigoEmpleado AS [Value]")
                .ToListAsync();
            empleado.Codigo = $"EMP-{numero[0]:D4}";
            _context.Empleados.Add(empleado);

            try
            {
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Empleado guardado correctamente.";
            return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
            {
                ModelState.AddModelError(nameof(Empleado.DepartamentoId), "El departamento seleccionado ya no existe.");
            }
        }

        await CargarDepartamentosAsync(empleado.DepartamentoId);
        return View(empleado);
    }
    // Modulo editar
    public async Task<IActionResult> Edit(int id)
    {
        var empleado = await _context.Empleados.FindAsync(id);
        if (empleado == null)
            return NotFound();

        await CargarDepartamentosAsync(empleado.DepartamentoId);
        return View(empleado);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nombres,Apellidos,FechaNacimiento,DepartamentoId")] Empleado empleado)
    {
        if (id != empleado.Id)
            return NotFound();

        var existente = await _context.Empleados.FindAsync(id);
        if (existente == null)
            return NotFound();

        // Se conserva el código guardado, aunque se intente enviar otro.
        empleado.Codigo = existente.Codigo;
        ModelState.Remove(nameof(Empleado.Codigo));
        await ValidarEmpleadoAsync(empleado);

        if (ModelState.IsValid)
        {
            existente.Nombres = empleado.Nombres;
            existente.Apellidos = empleado.Apellidos;
            existente.FechaNacimiento = empleado.FechaNacimiento;
            existente.DepartamentoId = empleado.DepartamentoId;

            try
            {
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Empleado guardado correctamente.";
            return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
            {
                ModelState.AddModelError(nameof(Empleado.DepartamentoId), "El departamento seleccionado ya no existe.");
            }
        }

        await CargarDepartamentosAsync(empleado.DepartamentoId);
        return View(empleado);
    }

    //Modulo Eliminar
    public async Task<IActionResult> Delete(int id)
    {
        var empleado = await _context.Empleados.AsNoTracking()
            .Include(e => e.Departamento).FirstOrDefaultAsync(e => e.Id == id);
        if (empleado == null)
            return NotFound();

        return View(empleado);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var empleado = await _context.Empleados.FindAsync(id);
        if (empleado == null)
            return NotFound();

        _context.Empleados.Remove(empleado);
        await _context.SaveChangesAsync();
        TempData["Exito"] = "Empleado eliminado correctamente.";
            return RedirectToAction(nameof(Index));
    }

    private async Task CargarDepartamentosAsync(int? seleccionado = null)
    {
        var departamentos = await _context.Departamentos.AsNoTracking()
            .OrderBy(d => d.Nombre).ToListAsync();
        ViewBag.DepartamentoId = new SelectList(departamentos, "Id", "Nombre", seleccionado);
    }

    private async Task ValidarEmpleadoAsync(Empleado empleado)
    {
        empleado.Nombres = empleado.Nombres?.Trim() ?? string.Empty;
        empleado.Apellidos = empleado.Apellidos?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(empleado.Nombres))
            ModelState.AddModelError(nameof(Empleado.Nombres), "Los nombres son obligatorios.");

        if (string.IsNullOrWhiteSpace(empleado.Apellidos))
            ModelState.AddModelError(nameof(Empleado.Apellidos), "Los apellidos son obligatorios.");

        if (!await _context.Departamentos.AnyAsync(d => d.Id == empleado.DepartamentoId))
            ModelState.AddModelError(nameof(Empleado.DepartamentoId), "Seleccione un departamento existente.");
    }
}
