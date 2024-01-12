﻿using Domain.Model;

namespace Infrastructure;


public interface IOfferRepository
{
    public void Add(Offer offer);
    public Offer? GetByInquiryId(string id);

}
public class DbOfferRepository : IOfferRepository
{
    private readonly ShopperContext _context;

    public DbOfferRepository(ShopperContext context)
    {
        _context = context;
    }





    public Offer? GetByInquiryId(string id)
    {
        return _context.Offers.FirstOrDefault(a => a.InquiryId == id);

    }

    public void Add(Offer offer)
    {
        _context.Offers.Add(offer);
        _context.SaveChanges();
    }
}

