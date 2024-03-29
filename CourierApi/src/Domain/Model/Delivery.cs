﻿namespace Domain.Model;

public class Delivery : BaseEntity
{
    public int DeliveryId { get; set; }
    public Guid DeliveryGuid { get; set; }
    //public int OfferId { get; set; }
    //public Offer Offer { get; set; }
    //public string? CourierId { get; set; }
    //public User? Courier { get; set; }

    public DateTime PickupDate { get; set; }
    public DateTime DeliveryDate { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; }

}

public enum DeliveryStatus
{
    PickedUp,
    Delivered,
    Failed,
     AcceptedByWorker,
     AcceptedByKurier,
     CancelledByClient,
     CancelledByKurier,
     CancelledByWorker,
     
}