﻿using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure;

public class DbPackageRepository : IPackageRepository
{
    private readonly ShopperContext _context;

    public DbPackageRepository(ShopperContext context)
    {
        _context = context;
    }

    public List<Package> GetAll()
    {
        return _context.Packages.ToList();
    }

    public Package GetById(int id)
    {
        return _context.Packages.Find(id);
    }

    public void Add(Package package)
    {
        _context.Packages.Add(package);
        _context.SaveChanges();
    }
}
