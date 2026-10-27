using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PruebaDanielRoblero.Data;
using PruebaDanielRoblero.Models;

namespace PruebaDanielRoblero.Controllers;

public class DepartamentosController : Controller
{
    private readonly ApplicationDbContext _context;

    public DepartamentosController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var departamentos = await _context.Departamentos
        .AsNoTracking()
        .OrderBy(d => d.Nombre)
        .ToListAsync();
        return View(departamentos);
    }

    // Modulo de crear
    public IActionResult Create()
    {
        return View(new Departamento());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Codigo,Nombre,Descripcion")] Departamento departamento)
    {
        await ValidarDepartamentoAsync(departamento);
        if (!ModelState.IsValid)
            return View(departamento);

        _context.Departamentos.Add(departamento);
        try
        {
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Departamento guardado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            ModelState.AddModelError(nameof(Departamento.Codigo), "El código ya existe.");
            return View(departamento);
        }
    }

    // Modulo de Editar
    public async Task<IActionResult> Edit(int id)
    {
        var departamento = await _context.Departamentos.FindAsync(id);
        if (departamento == null)
            return NotFound();

        return View(departamento);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Codigo,Nombre,Descripcion")] Departamento departamento)
    {
        if (id != departamento.Id)
            return NotFound();

        var existente = await _context.Departamentos.FindAsync(id);
        if (existente == null)
            return NotFound();

        await ValidarDepartamentoAsync(departamento);
        if (!ModelState.IsValid)
            return View(departamento);

        existente.Codigo = departamento.Codigo;
        existente.Nombre = departamento.Nombre;
        existente.Descripcion = departamento.Descripcion;

        try
        {
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Departamento guardado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            return NotFound();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            ModelState.AddModelError(nameof(Departamento.Codigo), "El código ya existe.");
            return View(departamento);
        }
    }

    //Modulo Eliminar
    public async Task<IActionResult> Delete(int id)
    {
        var departamento = await _context.Departamentos.AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);
        if (departamento == null)
            return NotFound();

        return View(departamento);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var departamento = await _context.Departamentos.FindAsync(id);
        if (departamento == null)
            return NotFound();

        if (await _context.Empleados.AnyAsync(e => e.DepartamentoId == id))
        {
            ModelState.AddModelError(string.Empty, "No se puede eliminar un departamento que tiene empleados asignados.");
            return View("Delete", departamento);
        }

        _context.Departamentos.Remove(departamento);
        try
        {
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Departamento eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
        {
            ModelState.AddModelError(string.Empty, "No se puede eliminar un departamento que tiene empleados asignados.");
            return View("Delete", departamento);
        }
    }

    ///
    private async Task ValidarDepartamentoAsync(Departamento departamento)
    {
        departamento.Codigo = departamento.Codigo?.Trim() ?? string.Empty;
        departamento.Nombre = departamento.Nombre?.Trim() ?? string.Empty;
        departamento.Descripcion = departamento.Descripcion?.Trim();

        if (string.IsNullOrWhiteSpace(departamento.Codigo))
            ModelState.AddModelError(nameof(Departamento.Codigo), "El código es obligatorio.");

        if (string.IsNullOrWhiteSpace(departamento.Nombre))
            ModelState.AddModelError(nameof(Departamento.Nombre), "El nombre es obligatorio.");

        if (await _context.Departamentos.AnyAsync(d => d.Codigo == departamento.Codigo && d.Id != departamento.Id))
            ModelState.AddModelError(nameof(Departamento.Codigo), "El código ya existe.");
    }
}
